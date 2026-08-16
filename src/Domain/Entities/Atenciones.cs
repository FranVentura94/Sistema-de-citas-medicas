namespace Domain.Entities;

public class Atenciones
{
    public long AtencionID { get; set; }
    public long PacienteID { get; set; }
    public int MedicoID { get; set; }
    public long? CitaID { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public string Estado { get; set; } = "EN_PROCESO";
    public string? MotivoAtencion { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    // Propiedades de navegación
    public Paciente? Paciente { get; set; }
    public Medico? Medico { get; set; }
    public Citas? Cita { get; set; }
}
