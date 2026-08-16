namespace Domain.Entities;

public class Citas
{
    public long CitaID { get; set; }
    public long PacienteID { get; set; }
    public int MedicoID { get; set; }
    public int? EspecialidadID { get; set; }
    public DateTime FechaHoraInicio { get; set; }
    public DateTime FechaHoraFin { get; set; }
    public string Estado { get; set; } = "PROGRAMADA";
    public string? MotivoConsulta { get; set; }
    public string? NotasCancelacion { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    // Propiedades de navegación de la arquitectura
    public Paciente? Paciente { get; set; }
    public Medico? Medico { get; set; }
}