using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MsClinical.Core.Dtos;
using MsClinical.Core.Services;

namespace MsClinical.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class ExpedientesController : ControllerBase
{
    private readonly IExpedienteService _service;

    public ExpedientesController(IExpedienteService service)
    {
        _service = service;
    }

    // GET api/expedientes/paciente/3   (expediente + historial de atenciones)
    [HttpGet("expedientes/paciente/{pacienteId:long}")]
    [Authorize(Roles = "Medico,Administrador")]
    public async Task<IActionResult> GetExpediente(long pacienteId, CancellationToken ct) =>
        Ok(await _service.GetPorPacienteAsync(pacienteId, ct));

    // PUT api/expedientes/paciente/3   (alergias y antecedentes)
    [HttpPut("expedientes/paciente/{pacienteId:long}")]
    [Authorize(Roles = "Medico,Administrador")]
    public async Task<IActionResult> ActualizarExpediente(long pacienteId, ExpedienteRequest request, CancellationToken ct) =>
        Ok(await _service.ActualizarAsync(pacienteId, request, ct));

    // GET api/catalogos/diagnosticos?buscar=faring
    [HttpGet("catalogos/diagnosticos")]
    public async Task<IActionResult> Diagnosticos(string? buscar, CancellationToken ct) =>
        Ok(await _service.BuscarDiagnosticosAsync(buscar, ct));

    // GET api/catalogos/medicamentos?buscar=amox
    [HttpGet("catalogos/medicamentos")]
    public async Task<IActionResult> Medicamentos(string? buscar, CancellationToken ct) =>
        Ok(await _service.BuscarMedicamentosAsync(buscar, ct));
}