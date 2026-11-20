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

    Task<List<Cita>> GetCitasAsync(CitaFiltro filtro, CancellationToken ct);
    Task<Cita?> GetCitaAsync(long id, CancellationToken ct);
    Task<bool> MedicoTieneCruceAsync(int medicoId, DateTime inicio, DateTime fin, CancellationToken ct);
    Task<bool> PacienteTieneCruceAsync(long pacienteId, DateTime inicio, DateTime fin, CancellationToken ct);
    Task AddCitaAsync(Cita cita, CancellationToken ct);

    Task<List<EstadoCita>> GetEstadosAsync(CancellationToken ct);

    Task GuardarCambiosAsync(CancellationToken ct);
}

// Comunicación REST con MS_Directory (lo implementa MsAgenda.Infrastructure)
public interface IDirectoryService
{
    Task<PacienteRemotoDto?> GetPacienteAsync(long pacienteId, CancellationToken ct);
    Task<MedicoRemotoDto?> GetMedicoAsync(int medicoId, CancellationToken ct);
}