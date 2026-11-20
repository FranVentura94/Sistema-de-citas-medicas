namespace MsAgenda.Core.Entities;

// Espacio físico donde se atiende
public class Consultorio
{
    public int ConsultorioId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Ubicacion { get; set; }
    public bool Activo { get; set; } = true;
}

// Disponibilidad semanal de un médico (ej: lunes de 08:00 a 12:00 en el consultorio 1)
public class HorarioMedico
{
    public int HorarioMedicoId { get; set; }

    // El médico vive en MS_Directory: aquí solo guardamos su Id y una copia del nombre
    public int MedicoId { get; set; }
    public string MedicoNombre { get; set; } = string.Empty;

    public int ConsultorioId { get; set; }

    // 0 = domingo, 1 = lunes ... 6 = sábado (igual que DayOfWeek de .NET)
    public int DiaSemana { get; set; }
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin { get; set; }
    public int DuracionCitaMinutos { get; set; } = 30;
    public bool Activo { get; set; } = true;

    public Consultorio? Consultorio { get; set; }
}

// Catálogo de estados: PROGRAMADA, CONFIRMADA, EN_ESPERA, EN_ATENCION, ATENDIDA, CANCELADA, NO_ASISTIO
public class EstadoCita
{
    public int EstadoCitaId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;

    // Color para mostrar el estado en el frontend (ej: #3B82F6)
    public string Color { get; set; } = string.Empty;
    public int Orden { get; set; }
}

public class Cita
{
    public long CitaId { get; set; }

    // Datos del paciente (copia tomada de MS_Directory al agendar)
    public long PacienteId { get; set; }
    public string PacienteNombre { get; set; } = string.Empty;
    public string PacienteDocumento { get; set; } = string.Empty;

    // Datos del médico (copia tomada de MS_Directory al agendar)
    public int MedicoId { get; set; }
    public string MedicoNombre { get; set; } = string.Empty;
    public int? EspecialidadId { get; set; }
    public string? EspecialidadNombre { get; set; }

    public int ConsultorioId { get; set; }

    public DateTime FechaHoraInicio { get; set; }
    public DateTime FechaHoraFin { get; set; }

    public int EstadoCitaId { get; set; }
    public string? MotivoConsulta { get; set; }
    public string? Notas { get; set; }

    public string? UsuarioRegistro { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public DateTime? FechaActualizacion { get; set; }

    public EstadoCita? Estado { get; set; }
    public Consultorio? Consultorio { get; set; }
}