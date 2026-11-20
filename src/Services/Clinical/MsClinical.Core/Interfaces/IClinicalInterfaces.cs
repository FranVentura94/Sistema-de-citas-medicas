using MsClinical.Core.Dtos;
using MsClinical.Core.Entities;

namespace MsClinical.Core.Interfaces;

// Acceso a la base MsClinicalDB (lo implementa MsClinical.Persistence)
public interface IClinicalRepository
{
    Task<ExpedienteClinico?> GetExpedienteAsync(long pacienteId, bool conAtenciones, CancellationToken ct);
    Task AddExpedienteAsync(ExpedienteClinico expediente, CancellationToken ct);

    // Trae la atención con todo su detalle (signos, notas, diagnósticos, receta)
    Task<Atencion?> GetAtencionAsync(long atencionId, CancellationToken ct);
    Task<Atencion?> GetAtencionPorCitaAsync(long citaId, CancellationToken ct);
    Task<List<Atencion>> GetAtencionesPorMedicoAsync(int medicoId, DateTime? desde, DateTime? hasta, CancellationToken ct);
    Task AddAtencionAsync(Atencion atencion, CancellationToken ct);

    Task<Diagnostico?> GetDiagnosticoAsync(int id, CancellationToken ct);
    Task<List<Diagnostico>> BuscarDiagnosticosAsync(string? buscar, CancellationToken ct);
    Task<List<Medicamento>> GetMedicamentosAsync(IEnumerable<int> ids, CancellationToken ct);
    Task<List<Medicamento>> BuscarMedicamentosAsync(string? buscar, CancellationToken ct);

    void RemoveDetalles(IEnumerable<PrescripcionDetalle> detalles);
    void RemoveDiagnostico(AtencionDiagnostico diagnostico);

    Task GuardarCambiosAsync(CancellationToken ct);
}

// Comunicación REST con MS_Agenda (lo implementa MsClinical.Infrastructure)
public interface IAgendaService
{
    Task<CitaRemotaDto?> GetCitaAsync(long citaId, CancellationToken ct);
    Task<CitaRemotaDto> CambiarEstadoCitaAsync(long citaId, string estado, string? notas, CancellationToken ct);
}