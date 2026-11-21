namespace BuildingBlocks.Auditoria;

// Un renglón de la bitácora: quién hizo qué, cuándo, sobre qué y desde dónde.
// No se puede editar ni borrar (lo bloquea un trigger en la base de datos).
public class RegistroAuditoria
{
    public long Id { get; set; }
    public DateTime Fecha { get; set; }

    public string Servicio { get; set; } = string.Empty;       // Identity, Directory, Agenda, Clinical
    public string Categoria { get; set; } = string.Empty;      // Datos, Seguridad, Consulta
    public string Accion { get; set; } = string.Empty;         // Crear, Editar, Eliminar, InicioSesion...
    public string Descripcion { get; set; } = string.Empty;    // Texto entendible: "Editó Paciente #5 (María Hernández)"

    // Sobre qué registro (tabla y su Id)
    public string? Entidad { get; set; }
    public string? EntidadId { get; set; }

    // Antes y después de cada campo, en JSON: [{ "campo": "Telefono", "antes": "7012", "despues": "7999" }]
    public string? Cambios { get; set; }

    // Quién
    public int? UsuarioId { get; set; }
    public string? UsuarioLogin { get; set; }
    public string? UsuarioNombre { get; set; }
    public string? Rol { get; set; }

    // Desde dónde
    public string? Ip { get; set; }
    public string? Dispositivo { get; set; }   // "Chrome · Windows"
    public string? Ruta { get; set; }          // "PUT /api/Pacientes/5"

    public bool Exito { get; set; } = true;
}

public static class CategoriasAuditoria
{
    public const string Datos = "Datos";
    public const string Seguridad = "Seguridad";
    public const string Consulta = "Consulta";
}

public static class AccionesAuditoria
{
    // Cambios de datos (automáticos)
    public const string Crear = "Crear";
    public const string Editar = "Editar";
    public const string Eliminar = "Eliminar";

    // Consultas de información sensible
    public const string Consultar = "Consultar";

    // Seguridad
    public const string InicioSesion = "InicioSesion";
    public const string InicioSesionFallido = "InicioSesionFallido";
    public const string CierreSesion = "CierreSesion";
    public const string PrimerIngreso = "PrimerIngreso";
    public const string ContrasenaTemporal = "ContrasenaTemporal";
    public const string AccesoDenegado = "AccesoDenegado";
    public const string SesionInvalida = "SesionInvalida";
}

// Un campo que cambió
public record CambioCampo(string Campo, object? Antes, object? Despues);

// Quién hizo la acción (si no se indica, se toma del token de la petición)
public record UsuarioAuditado(int? UsuarioId, string? Login, string? Nombre, string? Rol);

public class AuditoriaOpciones
{
    public string Servicio { get; set; } = string.Empty;

    // Campos que cambian solos y no aportan (ej. la fecha de último acceso en cada login)
    public HashSet<string> PropiedadesIgnoradas { get; } = new(StringComparer.OrdinalIgnoreCase) { "UltimoAcceso" };

    // Campos cuyo valor nunca se guarda: se muestra "••••••"
    //   - los que contienen estas palabras (PasswordHash, RefreshToken, ClientSecret...)
    public string[] PalabrasSensibles { get; set; } = { "Hash", "Token", "Secret" };
    //   - y los que se llaman exactamente así
    public HashSet<string> CamposSensibles { get; } = new(StringComparer.OrdinalIgnoreCase) { "Password", "Contrasena", "Clave" };

    public bool EsSensible(string campo) =>
        CamposSensibles.Contains(campo) || PalabrasSensibles.Any(p => campo.Contains(p, StringComparison.OrdinalIgnoreCase));
}