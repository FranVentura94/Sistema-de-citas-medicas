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

public record LoginResponse(string Token, DateTime ExpiraEn, UsuarioInfo Usuario);