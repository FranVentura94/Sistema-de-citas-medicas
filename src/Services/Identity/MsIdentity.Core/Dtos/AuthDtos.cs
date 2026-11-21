namespace MsIdentity.Core.Dtos;

public record LoginRequest(string NombreUsuario, string Password);

public record UsuarioInfo(
    int UsuarioId,
    string NombreUsuario,
    string NombreCompleto,
    string Email,
    string Rol,
    int? MedicoId,
    IReadOnlyList<string> Permisos);

// Si RequiereCambioPassword = true no se entrega token: la persona debe crear su contraseña primero
public record LoginResponse(string? Token, DateTime? ExpiraEn, UsuarioInfo? Usuario, bool RequiereCambioPassword);

// Primer ingreso: cambia la contraseña temporal por la suya
public record PrimerIngresoRequest(string NombreUsuario, string PasswordTemporal, string NuevaPassword);