using MsClinical.Core.Dtos;
using MsClinical.Core.Exceptions;
using MsClinical.Core.Interfaces;

namespace MsClinical.Core.Services;

public interface IExpedienteService
{
    Task<ExpedienteDto> GetPorPacienteAsync(long pacienteId, CancellationToken ct);
    Task<ExpedienteDto> ActualizarAsync(long pacienteId, ExpedienteRequest request, CancellationToken ct);
    Task<List<DiagnosticoDto>> BuscarDiagnosticosAsync(string? buscar, CancellationToken ct);
    Task<List<MedicamentoDto>> BuscarMedicamentosAsync(string? buscar, CancellationToken ct);
}

public class ExpedienteService : IExpedienteService
{
    private readonly IClinicalRepository _repo;

    public ExpedienteService(IClinicalRepository repo)
    {
        _repo = repo;
    }

    // Expediente + historial de atenciones del paciente
    public async Task<ExpedienteDto> GetPorPacienteAsync(long pacienteId, CancellationToken ct)
    {
        var e = await _repo.GetExpedienteAsync(pacienteId, true, ct)
            ?? throw new DomainException(Errores.NO_ENCONTRADO,
                "El paciente todavía no tiene expediente (se crea en su primera atención).", 404);

        return new ExpedienteDto(
            e.ExpedienteClinicoId,
            e.PacienteId,
            e.PacienteNombre,
            e.NumeroExpediente,
            e.FechaApertura,
            e.Alergias,
            e.AntecedentesPersonales,
            e.AntecedentesFamiliares,
            e.Observaciones,
            e.Atenciones.OrderByDescending(a => a.FechaInicio).Select(a => a.ToResumen()).ToList());
    }

    public async Task<ExpedienteDto> ActualizarAsync(long pacienteId, ExpedienteRequest request, CancellationToken ct)
    {
        var e = await _repo.GetExpedienteAsync(pacienteId, false, ct)
            ?? throw new DomainException(Errores.NO_ENCONTRADO, "El paciente todavía no tiene expediente.", 404);

        e.Alergias = request.Alergias;
        e.AntecedentesPersonales = request.AntecedentesPersonales;
        e.AntecedentesFamiliares = request.AntecedentesFamiliares;
        e.Observaciones = request.Observaciones;

        await _repo.GuardarCambiosAsync(ct);
        return await GetPorPacienteAsync(pacienteId, ct);
    }

    public async Task<List<DiagnosticoDto>> BuscarDiagnosticosAsync(string? buscar, CancellationToken ct)
    {
        var lista = await _repo.BuscarDiagnosticosAsync(buscar, ct);
        return lista.Select(d => new DiagnosticoDto(d.DiagnosticoId, d.Codigo, d.Descripcion)).ToList();
    }

    public async Task<List<MedicamentoDto>> BuscarMedicamentosAsync(string? buscar, CancellationToken ct)
    {
        var lista = await _repo.BuscarMedicamentosAsync(buscar, ct);
        return lista.Select(m => new MedicamentoDto(m.MedicamentoId, m.Nombre, m.PrincipioActivo, m.Presentacion, m.Concentracion)).ToList();
    }
}