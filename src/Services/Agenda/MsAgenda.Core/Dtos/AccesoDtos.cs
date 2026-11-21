namespace MsAgenda.Core.Dtos;

// Quién hace la petición (se arma con los datos del token)
public record UsuarioAgenda(int? UsuarioId, string? Login, string? Nombre, string Rol, int? MedicoId)
{
    public bool EsAdmin => Rol == "Administrador";
    public bool EsRecepcion => Rol == "Recepcion";
    public bool EsMedico => Rol == "Medico";
    public bool EsEnfermeria => Rol == "Enfermeria";
}

// ---------- Compartir agenda ----------
// Alcance: "TODA" o "PACIENTES". Vence: opcional (fecha).
public record CompartirAgendaRequest(
    int MedicoAutorizadoId,
    string Alcance,
    List<long>? PacienteIds,
    DateTime? FechaVencimiento,
    string? Motivo);

public record AccesoPacienteDto(long PacienteId, string PacienteNombre);

public record AccesoAgendaDto(
    int AccesoAgendaId,
    int MedicoPropietarioId,
    string MedicoPropietarioNombre,
    int MedicoAutorizadoId,
    string MedicoAutorizadoNombre,
    string Alcance,
    DateTime? FechaVencimiento,
    string? Motivo,
    string Estado,              // VIGENTE, VENCIDO, REVOCADO
    DateTime FechaCreacion,
    DateTime? FechaRevocacion,
    List<AccesoPacienteDto> Pacientes);

// Médicos cuya agenda puede ver/gestionar quien pregunta (para "Agendar cita" y filtros)
public record MedicoPermitidoDto(int MedicoId, bool Propio, bool AgendaCompleta, List<long> PacienteIds);

public record PuedeVerDto(bool PuedeVer, string Motivo);

// ---------- Datos que llegan de MS_Directory ----------
public record MiPerfilRemotoDto(int EmpleadoID, string Nombres, string Cargo, List<MedicoAsignadoRemotoDto> MedicosAsignados);

public record MedicoAsignadoRemotoDto(int MedicoID, string Nombre);
