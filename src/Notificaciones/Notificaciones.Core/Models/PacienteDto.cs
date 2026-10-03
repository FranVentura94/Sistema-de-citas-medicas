namespace Notificaciones.Core.Models;

public class PacienteDto
{
    public long PacienteID { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string NumeroDocumento { get; set; } = string.Empty;
    public string? Email { get; set; }
}