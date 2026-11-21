namespace MsClinical.Core.Entities;

// Expediente principal del paciente: se crea solo en su primera atención
public class ExpedienteClinico
{
    public long ExpedienteClinicoId { get; set; }
    public long PacienteId { get; set; }
    public string PacienteNombre { get; set; } = string.Empty;
    public string NumeroExpediente { get; set; } = string.Empty;
    public DateTime FechaApertura { get; set; } = DateTime.Now;

    public string? Alergias { get; set; }
    public string? AntecedentesPersonales { get; set; }
    public string? AntecedentesFamiliares { get; set; }
    public string? Observaciones { get; set; }

    public ICollection<Atencion> Atenciones { get; set; } = new List<Atencion>();
}

// Consulta médica realizada a partir de una cita de MS_Agenda
public class Atencion
{
    public long AtencionId { get; set; }
    public long ExpedienteClinicoId { get; set; }

    // Datos copiados de la cita (MS_Agenda) al iniciar la atención
    public long CitaId { get; set; }
    public long PacienteId { get; set; }
    public string PacienteNombre { get; set; } = string.Empty;
    public int MedicoId { get; set; }
    public string MedicoNombre { get; set; } = string.Empty;
    public string? EspecialidadNombre { get; set; }

    public DateTime FechaInicio { get; set; } = DateTime.Now;
    public DateTime? FechaFin { get; set; }

    // EN_PROCESO o FINALIZADA
    public string Estado { get; set; } = EstadosAtencion.EN_PROCESO;
    public string? MotivoConsulta { get; set; }
    public string? Observaciones { get; set; }

    // Si los signos vinieron del triaje: "Enf. Ana Pérez"
    public string? SignosTomadosPor { get; set; }

    public ExpedienteClinico? Expediente { get; set; }
    public SignosVitales? SignosVitales { get; set; }
    public Prescripcion? Prescripcion { get; set; }
    public ICollection<NotaMedica> Notas { get; set; } = new List<NotaMedica>();
    public ICollection<AtencionDiagnostico> Diagnosticos { get; set; } = new List<AtencionDiagnostico>();
}

public static class EstadosAtencion
{
    public const string EN_PROCESO = "EN_PROCESO";
    public const string FINALIZADA = "FINALIZADA";
}

public class SignosVitales
{
    public long SignosVitalesId { get; set; }
    public long AtencionId { get; set; }

    public int? PresionSistolica { get; set; }      // mmHg
    public int? PresionDiastolica { get; set; }     // mmHg
    public int? FrecuenciaCardiaca { get; set; }    // lpm
    public int? FrecuenciaRespiratoria { get; set; } // rpm
    public decimal? Temperatura { get; set; }       // °C
    public int? SaturacionOxigeno { get; set; }     // %
    public decimal? Peso { get; set; }              // kg
    public decimal? Talla { get; set; }             // cm

    public DateTime FechaRegistro { get; set; } = DateTime.Now;
}

public class NotaMedica
{
    public long NotaMedicaId { get; set; }
    public long AtencionId { get; set; }

    // EVOLUCION, VALORACION, HALLAZGO u OBSERVACION
    public string Tipo { get; set; } = "OBSERVACION";
    public string Contenido { get; set; } = string.Empty;
    public string? Autor { get; set; }
    public DateTime FechaRegistro { get; set; } = DateTime.Now;
}

// Catálogo de diagnósticos (CIE-10)
public class Diagnostico
{
    public int DiagnosticoId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
}

public class AtencionDiagnostico
{
    public long AtencionId { get; set; }
    public int DiagnosticoId { get; set; }

    // PRINCIPAL o SECUNDARIO
    public string Tipo { get; set; } = "PRINCIPAL";
    public string? Observacion { get; set; }

    public Diagnostico? Diagnostico { get; set; }
}

// Catálogo de medicamentos
public class Medicamento
{
    public int MedicamentoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string PrincipioActivo { get; set; } = string.Empty;
    public string Presentacion { get; set; } = string.Empty;
    public string Concentracion { get; set; } = string.Empty;
}

// Cabecera de la receta
public class Prescripcion
{
    public long PrescripcionId { get; set; }
    public long AtencionId { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Now;
    public string? IndicacionesGenerales { get; set; }

    public ICollection<PrescripcionDetalle> Detalles { get; set; } = new List<PrescripcionDetalle>();
}

// Cada medicamento recetado
public class PrescripcionDetalle
{
    public long PrescripcionDetalleId { get; set; }
    public long PrescripcionId { get; set; }
    public int MedicamentoId { get; set; }

    public string Dosis { get; set; } = string.Empty;       // ej: 1 tableta
    public string Frecuencia { get; set; } = string.Empty;  // ej: cada 8 horas
    public string Duracion { get; set; } = string.Empty;    // ej: 5 días
    public string Via { get; set; } = "Oral";
    public string? Indicaciones { get; set; }

    public Medicamento? Medicamento { get; set; }
}