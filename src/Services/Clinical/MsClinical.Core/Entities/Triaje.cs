namespace MsClinical.Core.Entities;

// Preconsulta: la enfermera (o el médico) toma los signos vitales cuando el paciente llega,
// antes de que el médico abra la consulta. Al iniciar la atención se copian a la atención.
public class Triaje
{
    public long TriajeId { get; set; }
    public long CitaId { get; set; }
    public long PacienteId { get; set; }
    public string PacienteNombre { get; set; } = string.Empty;
    public int MedicoId { get; set; }
    public string MedicoNombre { get; set; } = string.Empty;

    public int? PresionSistolica { get; set; }
    public int? PresionDiastolica { get; set; }
    public int? FrecuenciaCardiaca { get; set; }
    public int? FrecuenciaRespiratoria { get; set; }
    public decimal? Temperatura { get; set; }
    public int? SaturacionOxigeno { get; set; }
    public decimal? Peso { get; set; }
    public decimal? Talla { get; set; }

    public string? Notas { get; set; }

    // Quién lo tomó
    public int? RegistradoPorUsuarioId { get; set; }
    public string RegistradoPorNombre { get; set; } = string.Empty;
    public string RegistradoPorRol { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; } = DateTime.Now;
}
