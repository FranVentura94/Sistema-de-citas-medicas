namespace MsClinical.Core.Services;

// Datos del usuario que hace la petición (se leen del token JWT en el controller)
public record UsuarioActual(string? NombreUsuario, string? Rol, int? MedicoId, int? UsuarioId = null, string? Nombre = null)
{
    public bool EsAdministrador => Rol == "Administrador";
    public bool EsMedico => Rol == "Medico";
    public bool EsEnfermeria => Rol == "Enfermeria";
}