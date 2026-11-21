namespace MsClinical.Core.Dtos;

// ---------- Requests ----------
public record IniciarAtencionRequest(long CitaId);

public record SignosVitalesRequest(
    int? PresionSistolica,
    int? PresionDiastolica,
    int? FrecuenciaCardiaca,
    int? FrecuenciaRespiratoria,
    decimal? Temperatura,
    int? SaturacionOxigeno,
    decimal? Peso,
    decimal? Talla);

public record NotaRequest(string Tipo, string Contenido);

public record DiagnosticoRequest(int DiagnosticoId, string Tipo, string? Observacion);

public record PrescripcionDetalleRequest(
    int MedicamentoId,
    string Dosis,
    string Frecuencia,
    string Duracion,
    string? Via,
    string? Indicaciones);

public record PrescripcionRequest(string? IndicacionesGenerales, List<PrescripcionDetalleRequest> Detalles);

public record FinalizarAtencionRequest(string? Observaciones);

public record ExpedienteRequest(
    string? Alergias,
    string? AntecedentesPersonales,
    string? AntecedentesFamiliares,
    string? Observaciones);

// ---------- Respuestas ----------
public record SignosVitalesDto(
    int? PresionSistolica,
    int? PresionDiastolica,
    int? FrecuenciaCardiaca,
    int? FrecuenciaRespiratoria,
    decimal? Temperatura,
    int? SaturacionOxigeno,
    decimal? Peso,
    decimal? Talla,
    decimal? Imc,
    DateTime FechaRegistro);

public record NotaDto(long NotaMedicaId, string Tipo, string Contenido, string? Autor, DateTime FechaRegistro);

public record AtencionDiagnosticoDto(int DiagnosticoId, string Codigo, string Descripcion, string Tipo, string? Observacion);

public record PrescripcionDetalleDto(
    int MedicamentoId,
    string Medicamento,
    string Presentacion,
    string Concentracion,
    string Dosis,
    string Frecuencia,
    string Duracion,
    string Via,
    string? Indicaciones);

public record PrescripcionDto(long PrescripcionId, DateTime Fecha, string? IndicacionesGenerales, List<PrescripcionDetalleDto> Detalles);

public record AtencionDto(
    long AtencionId,
    long CitaId,
    long PacienteId,
    string PacienteNombre,
    string NumeroExpediente,
    int MedicoId,
    string MedicoNombre,
    string? EspecialidadNombre,
    DateTime FechaInicio,
    DateTime? FechaFin,
    string Estado,
    string? MotivoConsulta,
    string? Observaciones,
    SignosVitalesDto? SignosVitales,
    List<NotaDto> Notas,
    List<AtencionDiagnosticoDto> Diagnosticos,
    PrescripcionDto? Prescripcion,
    string? SignosTomadosPor = null);

public record AtencionResumenDto(
    long AtencionId,
    long CitaId,
    DateTime FechaInicio,
    string MedicoNombre,
    string? EspecialidadNombre,
    string Estado,
    string? MotivoConsulta,
    List<string> Diagnosticos);

public record ExpedienteDto(
    long ExpedienteClinicoId,
    long PacienteId,
    string PacienteNombre,
    string NumeroExpediente,
    DateTime FechaApertura,
    string? Alergias,
    string? AntecedentesPersonales,
    string? AntecedentesFamiliares,
    string? Observaciones,
    List<AtencionResumenDto> Atenciones);

public record DiagnosticoDto(int DiagnosticoId, string Codigo, string Descripcion);

public record MedicamentoDto(int MedicamentoId, string Nombre, string PrincipioActivo, string Presentacion, string Concentracion);

// ---------- Datos que llegan de MS_Agenda ----------
public record CitaRemotaDto(
    long CitaId,
    long PacienteId,
    string PacienteNombre,
    int MedicoId,
    string MedicoNombre,
    string? EspecialidadNombre,
    DateTime FechaHoraInicio,
    string EstadoCodigo,
    string EstadoNombre,
    string? MotivoConsulta);

// ---------- Triaje (preconsulta de enfermería) ----------
public record TriajeRequest(
    long CitaId,
    int? PresionSistolica,
    int? PresionDiastolica,
    int? FrecuenciaCardiaca,
    int? FrecuenciaRespiratoria,
    decimal? Temperatura,
    int? SaturacionOxigeno,
    decimal? Peso,
    decimal? Talla,
    string? Notas);

public record TriajeDto(
    long TriajeId,
    long CitaId,
    long PacienteId,
    string PacienteNombre,
    int MedicoId,
    string MedicoNombre,
    SignosVitalesDto Signos,
    string? Notas,
    string RegistradoPorNombre,
    string RegistradoPorRol,
    DateTime FechaRegistro);

// ---------- Datos que llegan de MS_Directory ----------
public record PacienteRemotoDto(long PacienteID, string Nombres, string Apellidos);

public record PuedeVerRemotoDto(bool PuedeVer, string Motivo);
