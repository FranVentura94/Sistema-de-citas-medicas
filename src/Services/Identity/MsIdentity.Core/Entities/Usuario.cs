namespace MsIdentity.Core.Entities;

public class Usuario
{
    public int UsuarioId { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public int RolId { get; set; }

    // Si el usuario es médico, se enlaza con el MedicoID de MS_Directory
    public int? MedicoId { get; set; }

    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public DateTime? UltimoAcceso { get; set; }

    public Rol? Rol { get; set; }
}