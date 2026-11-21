namespace MsAgenda.Core.Entities;

// Un médico (propietario) le comparte su agenda a otro médico (autorizado):
//   - TODA: puede ver y gestionar todas sus citas y pacientes
//   - PACIENTES: solo los pacientes elegidos
// Puede tener vencimiento y se puede revocar en cualquier momento.
public class AccesoAgenda
{
    public int AccesoAgendaId { get; set; }

    public int MedicoPropietarioId { get; set; }
    public string MedicoPropietarioNombre { get; set; } = string.Empty;

    public int MedicoAutorizadoId { get; set; }
    public string MedicoAutorizadoNombre { get; set; } = string.Empty;

    public string Alcance { get; set; } = AlcancesAcceso.TODA;
    public DateTime? FechaVencimiento { get; set; }
    public string? Motivo { get; set; }

    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public DateTime? FechaRevocacion { get; set; }

    public ICollection<AccesoPaciente> Pacientes { get; set; } = new List<AccesoPaciente>();

    public bool Vigente(DateTime ahora) => Activo && (FechaVencimiento is null || FechaVencimiento > ahora);
}

// Paciente incluido en un acceso de tipo PACIENTES
public class AccesoPaciente
{
    public int AccesoAgendaId { get; set; }
    public long PacienteId { get; set; }
    public string PacienteNombre { get; set; } = string.Empty;

    public AccesoAgenda? Acceso { get; set; }
}

public static class AlcancesAcceso
{
    public const string TODA = "TODA";
    public const string PACIENTES = "PACIENTES";
}
