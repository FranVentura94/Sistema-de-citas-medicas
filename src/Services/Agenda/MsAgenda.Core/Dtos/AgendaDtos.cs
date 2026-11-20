namespace MsAgenda.Core.Dtos;

// ---------- Consultorios y horarios ----------
public record ConsultorioDto(int ConsultorioId, string Codigo, string Nombre, string? Ubicacion);

public record HorarioDto(
    int HorarioMedicoId,
    int MedicoId,
    string MedicoNombre,
    int ConsultorioId,
    string ConsultorioNombre,
    int DiaSemana,
    string DiaNombre,
    string HoraInicio,
    string HoraFin,
    int DuracionCitaMinutos);

// Horas en formato "HH:mm", ej: "08:00"
public record CrearHorarioRequest(
    int MedicoId,
    int ConsultorioId,
    int DiaSemana,
    string HoraInicio,
    string HoraFin,
    int DuracionCitaMinutos = 30);

public record EspacioDisponibleDto(
    DateTime Inicio,
    DateTime Fin,
    bool Disponible,
    int ConsultorioId,
    string ConsultorioNombre);

// ---------- Citas ----------
public record CitaDto(
    long CitaId,
    long PacienteId,
    string PacienteNombre,
    string PacienteDocumento,
    int MedicoId,
    string MedicoNombre,
    int? EspecialidadId,
    string? EspecialidadNombre,
    int ConsultorioId,
    string ConsultorioNombre,
    DateTime FechaHoraInicio,
    DateTime FechaHoraFin,
    string EstadoCodigo,
    string EstadoNombre,
    string EstadoColor,
    string? MotivoConsulta,
    string? Notas,
    DateTime FechaCreacion);

public record CrearCitaRequest(
    long PacienteId,
    int MedicoId,
    int? EspecialidadId,
    DateTime FechaHoraInicio,
    string? MotivoConsulta);

// Estado = código, ej: "EN_ATENCION"
public record CambiarEstadoRequest(string Estado, string? Notas);

public record EstadoCitaDto(string Codigo, string Nombre, string Color);

public record CitaFiltro(
    DateTime? Desde,
    DateTime? Hasta,
    int? MedicoId,
    long? PacienteId,
    string? Estado);

public record ResumenEstadoDto(string Codigo, string Nombre, string Color, int Cantidad);

public record ResumenCitasDto(DateOnly Fecha, int Total, List<ResumenEstadoDto> PorEstado);

// ---------- Datos que llegan de MS_Directory ----------
public record PacienteRemotoDto(long PacienteID, string Nombres, string Apellidos, string NumeroDocumento, bool Activo);

public record EspecialidadRemotaDto(int EspecialidadID, string Nombre, bool Principal);

public record MedicoRemotoDto(int MedicoID, string NombreCompleto, bool Activo, List<EspecialidadRemotaDto> Especialidades);