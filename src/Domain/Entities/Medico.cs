namespace Domain.Entities;

public class Medico
{
    // Clave primaria
    public int MedicoID { get; set; }

    // Datos Personales y Profesionales
    public int? UsuarioID { get; set; }
    public string Nombres { get; set; } = null!;
    public string Apellidos { get; set; } = null!;
    public string NumeroColegiatura { get; set; } = null!;
    public string Identification { get; set; } = null!;

    // Contacto
    public string Telefono { get; set; } = null!;
    public string Email { get; set; } = null!;

    // Auditoría y Estado
    public DateTime FechaIngreso { get; set; } = DateTime.Now;
    public bool Activo { get; set; } = true;
}