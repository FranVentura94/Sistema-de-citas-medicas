using MsClinical.Core.Dtos;
using MsClinical.Core.Entities;
using MsClinical.Core.Exceptions;
using MsClinical.Core.Interfaces;

namespace MsClinical.Core.Services;

public interface IExpedienteService
{
    Task<ExpedienteDto> GetPorPacienteAsync(long pacienteId, UsuarioActual usuario, CancellationToken ct);
    Task<ExpedienteDto> ActualizarAsync(long pacienteId, ExpedienteRequest request, UsuarioActual usuario, CancellationToken ct);
    Task<List<DiagnosticoDto>> BuscarDiagnosticosAsync(string? buscar, CancellationToken ct);
    Task<List<MedicamentoDto>> BuscarMedicamentosAsync(string? buscar, CancellationToken ct);
}

public class ExpedienteService : IExpedienteService
{
    private readonly IClinicalRepository _repo;
    private readonly IAgendaService _agenda;
    private readonly IPacientesService _pacientes;

    public ExpedienteService(IClinicalRepository repo, IAgendaService agenda, IPacientesService pacientes)
    {
        _repo = repo;
        _agenda = agenda;
        _pacientes = pacientes;
    }

    // Expediente + historial de atenciones del paciente.
    // Si todavía no existe, se abre en ese momento (así se pueden registrar alergias y antecedentes desde el inicio).
    public async Task<ExpedienteDto> GetPorPacienteAsync(long pacienteId, UsuarioActual usuario, CancellationToken ct)
    {
        await ValidarAccesoAsync(pacienteId, usuario, ct);
        await ObtenerOCrearAsync(pacienteId, ct);

        var e = (await _repo.GetExpedienteAsync(pacienteId, true, ct))!;
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

    public async Task<ExpedienteDto> ActualizarAsync(long pacienteId, ExpedienteRequest request, UsuarioActual usuario, CancellationToken ct)
    {
        await ValidarAccesoAsync(pacienteId, usuario, ct);
        var e = await ObtenerOCrearAsync(pacienteId, ct);

        e.Alergias = request.Alergias;
        e.AntecedentesPersonales = request.AntecedentesPersonales;
        e.AntecedentesFamiliares = request.AntecedentesFamiliares;
        e.Observaciones = request.Observaciones;

        await _repo.GuardarCambiosAsync(ct);
        return await GetPorPacienteAsync(pacienteId, usuario, ct);
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

    // ---------- Privacidad del historial ----------
    // Administrador: todos. Médico: pacientes con cita con él o compartidos. Enfermería: pacientes de sus médicos.
    private async Task ValidarAccesoAsync(long pacienteId, UsuarioActual usuario, CancellationToken ct)
    {
        if (usuario.EsAdministrador) return;
        if (!await _agenda.PuedeVerPacienteAsync(pacienteId, ct))
            throw new DomainException(Errores.SIN_PERMISO,
                "No tienes acceso al historial de este paciente: no tiene citas contigo y nadie te lo ha compartido.", 403);
    }

    private async Task<ExpedienteClinico> ObtenerOCrearAsync(long pacienteId, CancellationToken ct)
    {
        var e = await _repo.GetExpedienteAsync(pacienteId, false, ct);
        if (e is not null) return e;

        var paciente = await _pacientes.GetPacienteAsync(pacienteId, ct)
            ?? throw new DomainException(Errores.NO_ENCONTRADO, "El paciente no existe.", 404);

        e = new ExpedienteClinico
        {
            PacienteId = pacienteId,
            PacienteNombre = $"{paciente.Nombres} {paciente.Apellidos}".Trim(),
            NumeroExpediente = $"EXP-{pacienteId:D6}",
            FechaApertura = DateTime.Now
        };
        await _repo.AddExpedienteAsync(e, ct);
        return e;
    }
}
