using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MsClinical.Core.Dtos;
using MsClinical.Core.Services;

namespace MsClinical.Api.Controllers;

// Triaje (preconsulta): signos vitales que toma Enfermería antes de que el médico abra la consulta
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Enfermeria,Medico,Administrador")]
public class TriajesController : ControllerBase
{
    private readonly ITriajeService _service;

    public TriajesController(ITriajeService service)
    {
        _service = service;
    }

    // POST api/Triajes  -> registra (o corrige) el triaje y deja al paciente "En espera"
    [HttpPost]
    public async Task<IActionResult> Registrar(TriajeRequest request, CancellationToken ct) =>
        Ok(await _service.RegistrarAsync(request, User.Clinico(), ct));

    // GET api/Triajes/cita/5  -> 204 si todavía no tiene triaje
    [HttpGet("cita/{citaId:long}")]
    public async Task<IActionResult> PorCita(long citaId, CancellationToken ct)
    {
        var triaje = await _service.GetPorCitaAsync(citaId, ct);
        return triaje is null ? NoContent() : Ok(triaje);
    }
}
