namespace MsClinical.Core.Services;

// Datos del usuario que hace la petición (se leen del token JWT en el controller)
public record UsuarioActual(string? NombreUsuario, string? Rol, int? MedicoId)
{
    public bool EsAdministrador => Rol == "Administrador";
}