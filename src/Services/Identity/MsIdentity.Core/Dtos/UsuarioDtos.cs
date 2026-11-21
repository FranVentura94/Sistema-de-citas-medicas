namespace MsIdentity.Core.Dtos;

// ---------- Respuestas ----------
public record UsuarioDto(
    int UsuarioId,
    string NombreUsuario,
    string NombreCompleto,
    string Email,
    int RolId,
    string Rol,
    int? MedicoId,
    bool Activo,
    bool TieneContrasena,          // false = todavía no se le generó contraseña temporal
    bool DebeCambiarPassword,      // true = tiene una temporal y aún no entra por primera vez
    DateTime? PasswordTemporalVence,
    DateTime FechaCreacion,
    DateTime? UltimoAcceso);

public record RolDto(int RolId, string Nombre, string? Descripcion);

// Se muestra UNA sola vez al Administrador para que se la dicte a la persona
public record PasswordTemporalDto(string PasswordTemporal, DateTime VenceEn);

// ---------- Requests (solo Administrador) ----------
// Ya no lleva contraseña: el sistema genera una temporal y la persona crea la suya
public record CrearUsuarioRequest(
    string NombreUsuario,
    string NombreCompleto,
    string? Email,
    int RolId,
    int? MedicoId);

public record ActualizarUsuarioRequest(
    string NombreCompleto,
    string? Email,
    int RolId,
    int? MedicoId,
    bool Activo);