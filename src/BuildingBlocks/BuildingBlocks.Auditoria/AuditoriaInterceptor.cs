using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;

namespace BuildingBlocks.Auditoria;

// Se engancha a SaveChanges de Entity Framework: todo lo que se crea, edita o elimina
// queda registrado con el antes y el después de cada campo, en la MISMA transacción.
// Si la auditoría no se puede guardar, el cambio tampoco se guarda.
public class AuditoriaInterceptor : SaveChangesInterceptor
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private const string OCULTO = "••••••";

    private readonly ContextoAuditoria _contexto;

    private List<Pendiente>? _pendientes;
    private IDbContextTransaction? _transaccionPropia;
    private bool _guardandoAuditoria;

    private sealed class Pendiente
    {
        public required EntityEntry Entrada { get; init; }
        public required string Accion { get; init; }
        public required string Entidad { get; init; }
        public string? Id { get; set; }
        public string? Nombre { get; init; }
        public required List<CambioCampo> Cambios { get; init; }
    }

    public AuditoriaInterceptor(ContextoAuditoria contexto)
    {
        _contexto = contexto;
    }

    // ---------- Antes de guardar: se toma la foto de los cambios ----------
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (Capturar(eventData.Context) && eventData.Context!.Database.CurrentTransaction is null)
            _transaccionPropia = eventData.Context.Database.BeginTransaction();
        return result;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (Capturar(eventData.Context) && eventData.Context!.Database.CurrentTransaction is null)
            _transaccionPropia = await eventData.Context.Database.BeginTransactionAsync(cancellationToken);
        return result;
    }

    // ---------- Después de guardar: se escriben los registros (ya con los Id nuevos) ----------
    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        if (_guardandoAuditoria || _pendientes is null || eventData.Context is null) return result;
        try
        {
            _guardandoAuditoria = true;
            eventData.Context.Set<RegistroAuditoria>().AddRange(Construir());
            eventData.Context.SaveChanges();
            _transaccionPropia?.Commit();
        }
        catch
        {
            _transaccionPropia?.Rollback();
            throw;
        }
        finally
        {
            Limpiar();
        }
        return result;
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        if (_guardandoAuditoria || _pendientes is null || eventData.Context is null) return result;
        try
        {
            _guardandoAuditoria = true;
            eventData.Context.Set<RegistroAuditoria>().AddRange(Construir());
            await eventData.Context.SaveChangesAsync(cancellationToken);
            if (_transaccionPropia is not null) await _transaccionPropia.CommitAsync(cancellationToken);
        }
        catch
        {
            if (_transaccionPropia is not null) await _transaccionPropia.RollbackAsync(CancellationToken.None);
            throw;
        }
        finally
        {
            Limpiar();
        }
        return result;
    }

    // ---------- Si falla el guardado, se deshace todo ----------
    public override void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        if (_guardandoAuditoria) return;
        _transaccionPropia?.Rollback();
        Limpiar();
    }

    public override async Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        if (_guardandoAuditoria) return;
        if (_transaccionPropia is not null) await _transaccionPropia.RollbackAsync(CancellationToken.None);
        Limpiar();
    }

    // ===================== Captura de cambios =====================
    private bool Capturar(DbContext? db)
    {
        if (_guardandoAuditoria || db is null) return false;

        db.ChangeTracker.DetectChanges();
        var pendientes = new List<Pendiente>();

        foreach (var e in db.ChangeTracker.Entries())
        {
            if (e.Entity is RegistroAuditoria) continue;
            if (e.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted)) continue;

            var cambios = new List<CambioCampo>();
            foreach (var p in e.Properties)
            {
                var campo = p.Metadata.Name;
                if (_contexto.Opciones.PropiedadesIgnoradas.Contains(campo)) continue;

                switch (e.State)
                {
                    case EntityState.Added:
                        if (p.Metadata.IsPrimaryKey() && p.IsTemporary) continue;
                        if (p.CurrentValue is not null) cambios.Add(new(campo, null, Valor(campo, p.CurrentValue)));
                        break;
                    case EntityState.Deleted:
                        cambios.Add(new(campo, Valor(campo, p.OriginalValue), null));
                        break;
                    default:
                        if (p.IsModified && !Equals(p.OriginalValue, p.CurrentValue))
                            cambios.Add(new(campo, Valor(campo, p.OriginalValue), Valor(campo, p.CurrentValue)));
                        break;
                }
            }

            // Si solo cambiaron campos ignorados (o nada), no se registra
            if (e.State == EntityState.Modified && cambios.Count == 0) continue;

            pendientes.Add(new Pendiente
            {
                Entrada = e,
                Accion = e.State switch
                {
                    EntityState.Added => AccionesAuditoria.Crear,
                    EntityState.Deleted => AccionesAuditoria.Eliminar,
                    _ => AccionesAuditoria.Editar
                },
                Entidad = e.Metadata.ClrType.Name,
                Id = e.State == EntityState.Added ? null : Llave(e),
                Nombre = NombreVisible(e),
                Cambios = cambios
            });
        }

        _pendientes = pendientes.Count > 0 ? pendientes : null;
        return _pendientes is not null;
    }

    private IEnumerable<RegistroAuditoria> Construir()
    {
        foreach (var p in _pendientes!)
        {
            // Los registros nuevos ya tienen su Id real
            p.Id ??= Llave(p.Entrada);

            var verbo = p.Accion switch
            {
                AccionesAuditoria.Crear => "Creó",
                AccionesAuditoria.Eliminar => "Eliminó",
                _ => "Editó"
            };
            var descripcion = $"{verbo} {p.Entidad} #{p.Id}" + (string.IsNullOrWhiteSpace(p.Nombre) ? "" : $" ({p.Nombre})");

            var registro = _contexto.Nuevo(CategoriasAuditoria.Datos, p.Accion, descripcion);
            registro.Entidad = p.Entidad;
            registro.EntidadId = p.Id;
            registro.Cambios = JsonSerializer.Serialize(p.Cambios, Json);
            yield return registro;
        }
    }

    private void Limpiar()
    {
        _pendientes = null;
        _transaccionPropia?.Dispose();
        _transaccionPropia = null;
        _guardandoAuditoria = false;
    }

    // ===================== Ayudantes =====================
    private object? Valor(string campo, object? valor)
    {
        if (valor is null) return null;
        if (_contexto.Opciones.EsSensible(campo))
            return OCULTO;
        return valor switch
        {
            DateTime d => d.ToString("yyyy-MM-ddTHH:mm:ss"),
            DateOnly d => d.ToString("yyyy-MM-dd"),
            TimeOnly t => t.ToString("HH:mm"),
            TimeSpan t => t.ToString(@"hh\:mm"),
            _ => valor
        };
    }

    // Llave del registro; si es compuesta: "3-1"
    private static string Llave(EntityEntry e) =>
        string.Join("-", e.Properties.Where(p => p.Metadata.IsPrimaryKey()).Select(p => p.CurrentValue ?? p.OriginalValue));

    // Un nombre entendible del registro, si lo tiene
    private static string? NombreVisible(EntityEntry e)
    {
        object? Leer(string nombre)
        {
            var p = e.Properties.FirstOrDefault(x => x.Metadata.Name == nombre);
            return p is null ? null : e.State == EntityState.Deleted ? p.OriginalValue : p.CurrentValue;
        }

        if (Leer("NombreCompleto") is string completo && completo.Length > 0) return completo;
        if (Leer("Nombres") is string nombres) return $"{nombres} {Leer("Apellidos") as string}".Trim();
        foreach (var campo in new[] { "Nombre", "NombreUsuario", "NumeroExpediente", "Codigo" })
            if (Leer(campo) is string texto && texto.Length > 0) return texto;
        return null;
    }
}