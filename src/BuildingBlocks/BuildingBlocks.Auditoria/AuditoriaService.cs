using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Auditoria;

// Para registrar a mano lo que no es un cambio de datos: inicios de sesión, consultas, accesos denegados...
public interface IAuditoria
{
    Task RegistrarAsync(
        string categoria,
        string accion,
        string descripcion,
        string? entidad = null,
        string? entidadId = null,
        bool exito = true,
        UsuarioAuditado? usuario = null,
        CancellationToken ct = default);
}

// Para leer la bitácora (pantalla de Auditoría del Administrador)
public interface IAuditoriaLector
{
    Task<List<RegistroAuditoria>> BuscarAsync(FiltroAuditoria filtro, CancellationToken ct);
}

public record FiltroAuditoria(
    DateTime? Desde,
    DateTime? Hasta,
    string? Categoria,
    string? Entidad,
    string? EntidadId,
    int? UsuarioId,
    int? Max);

public class AuditoriaService<TContext> : IAuditoria where TContext : DbContext
{
    private readonly ContextoAuditoria _contexto;
    private readonly IServiceScopeFactory _scopes;
    private readonly ILogger<AuditoriaService<TContext>> _logger;

    public AuditoriaService(ContextoAuditoria contexto, IServiceScopeFactory scopes, ILogger<AuditoriaService<TContext>> logger)
    {
        _contexto = contexto;
        _scopes = scopes;
        _logger = logger;
    }

    public async Task RegistrarAsync(string categoria, string accion, string descripcion, string? entidad = null,
        string? entidadId = null, bool exito = true, UsuarioAuditado? usuario = null, CancellationToken ct = default)
    {
        var registro = _contexto.Nuevo(categoria, accion, descripcion, usuario);
        registro.Entidad = entidad;
        registro.EntidadId = entidadId;
        registro.Exito = exito;

        try
        {
            // Se guarda con un DbContext aparte: así no arrastra otros cambios pendientes
            // y queda registrado aunque la operación principal falle (ej. login incorrecto)
            using var scope = _scopes.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TContext>();
            db.Set<RegistroAuditoria>().Add(registro);
            await db.SaveChangesAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo guardar el registro de auditoría: {Descripcion}", descripcion);
        }
    }
}

public class AuditoriaLector<TContext> : IAuditoriaLector where TContext : DbContext
{
    private readonly TContext _db;

    public AuditoriaLector(TContext db)
    {
        _db = db;
    }

    public Task<List<RegistroAuditoria>> BuscarAsync(FiltroAuditoria f, CancellationToken ct)
    {
        var q = _db.Set<RegistroAuditoria>().AsNoTracking().AsQueryable();

        if (f.Desde.HasValue) q = q.Where(r => r.Fecha >= f.Desde.Value);
        if (f.Hasta.HasValue) q = q.Where(r => r.Fecha < f.Hasta.Value);
        if (!string.IsNullOrWhiteSpace(f.Categoria)) q = q.Where(r => r.Categoria == f.Categoria);
        if (!string.IsNullOrWhiteSpace(f.Entidad)) q = q.Where(r => r.Entidad == f.Entidad);
        if (!string.IsNullOrWhiteSpace(f.EntidadId)) q = q.Where(r => r.EntidadId == f.EntidadId);
        if (f.UsuarioId.HasValue) q = q.Where(r => r.UsuarioId == f.UsuarioId);

        var max = Math.Clamp(f.Max ?? 500, 1, 2000);
        return q.OrderByDescending(r => r.Fecha).ThenByDescending(r => r.Id).Take(max).ToListAsync(ct);
    }
}