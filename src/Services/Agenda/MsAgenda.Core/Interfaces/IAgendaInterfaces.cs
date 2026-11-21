using MsAgenda.Core.Dtos;
using MsAgenda.Core.Entities;

namespace MsAgenda.Core.Interfaces;

// Acceso a la base MsAgendaDB (lo implementa MsAgenda.Persistence)
public interface IAgendaRepository
{
    Task<List<Consultorio>> GetConsultoriosAsync(CancellationToken ct);
    Task<Consultorio?> GetConsultorioAsync(int id, CancellationToken ct);

    Task<List<HorarioMedico>> GetHorariosAsync(int? medicoId, CancellationToken ct);
    Task<List<HorarioMedico>> GetHorariosDelDiaAsync(int medicoId, int diaSemana, CancellationToken ct);
    Task AddHorarioAsync(HorarioMedico horario, CancellationToken ct);
    Task<HorarioMedico?> GetHorarioAsync(int horarioId, CancellationToken ct);

    Task<List<Cita>> GetCitasAsync(CitaFiltro filtro, CancellationToken ct);
    Task<Cita?> GetCitaAsync(long id, CancellationToken ct);
    Task<bool> MedicoTieneCruceAsync(int medicoId, DateTime inicio, DateTime fin, CancellationToken ct);
    Task<bool> PacienteTieneCruceAsync(long pacienteId, DateTime inicio, DateTime fin, CancellationToken ct);
    Task AddCitaAsync(Cita cita, CancellationToken ct);

    Task<List<EstadoCita>> GetEstadosAsync(CancellationToken ct);

    // ---------- Agendas compartidas ----------
    Task<List<AccesoAgenda>> GetAccesosComoPropietarioAsync(int medicoId, CancellationToken ct);
    Task<List<AccesoAgenda>> GetAccesosComoAutorizadoAsync(int medicoId, CancellationToken ct);
    Task<AccesoAgenda?> GetAccesoAsync(int accesoId, CancellationToken ct);
    Task<AccesoAgenda?> GetAccesoActivoEntreAsync(int propietarioId, int autorizadoId, CancellationToken ct);
    Task AddAccesoAsync(AccesoAgenda acceso, CancellationToken ct);
    void QuitarPacientesDeAcceso(AccesoAgenda acceso);
    Task<bool> ExisteCitaAsync(IEnumerable<int> medicoIds, long pacienteId, CancellationToken ct);
    Task<List<AccesoPacienteDto>> GetPacientesDeMedicoAsync(int medicoId, CancellationToken ct);

    Task GuardarCambiosAsync(CancellationToken ct);
}

// Comunicación REST con MS_Directory (lo implementa MsAgenda.Infrastructure)
public interface IDirectoryService
{
    Task<PacienteRemotoDto?> GetPacienteAsync(long pacienteId, CancellationToken ct);
    Task<MedicoRemotoDto?> GetMedicoAsync(int medicoId, CancellationToken ct);

    // Perfil de quien llama (para saber a qué médicos asiste una enfermera)
    Task<MiPerfilRemotoDto?> GetMiPerfilAsync(CancellationToken ct);
}