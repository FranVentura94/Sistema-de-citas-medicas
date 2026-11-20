using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MsAgenda.Core.Dtos;
using MsAgenda.Core.Services;

namespace MsAgenda.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CitasController : ControllerBase
{
    private readonly ICitaService _service;

    public CitasController(ICitaService service)
    {
        _service = service;
    }

    // GET api/Citas?desde=2026-11-20&hasta=2026-11-21&medicoId=1&pacienteId=2&estado=PROGRAMADA
    [HttpGet]
    public async Task<IActionResult> GetCitas(
        DateTime? desde, DateTime? hasta, int? medicoId, long? pacienteId, string? estado, CancellationToken ct)
    {
        var filtro = new CitaFiltro(desde, hasta, medicoId, pacienteId, estado);
        return Ok(await _service.GetCitasAsync(filtro, ct));
    }

    // GET api/Citas/mis-citas?fecha=2026-11-20  (el médico ve solo sus citas, según su token)
    [HttpGet("mis-citas")]
    [Authorize(Roles = "Medico")]
    public async Task<IActionResult> GetMisCitas(DateOnly? fecha, CancellationToken ct)
    {
        if (!int.TryParse(User.FindFirst("medicoId")?.Value, out var medicoId))
            return BadRequest(new { codigo = "SIN_MEDICO", mensaje = "Tu usuario no está enlazado a un médico." });

        DateTime? desde = fecha?.ToDateTime(TimeOnly.MinValue);
        var filtro = new CitaFiltro(desde, desde?.AddDays(1), medicoId, null, null);
        return Ok(await _service.GetCitasAsync(filtro, ct));
    }

    // GET api/Citas/5
    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetCita(long id, CancellationToken ct)
    {
        return Ok(await _service.GetCitaAsync(id, ct));
    }

    // POST api/Citas  (Recepción o Administrador)
    [HttpPost]
    [Authorize(Roles = "Administrador,Recepcion")]
    public async Task<IActionResult> CrearCita(CrearCitaRequest request, CancellationToken ct)
    {
        var cita = await _service.CrearCitaAsync(request, User.Identity?.Name, ct);
        return CreatedAtAction(nameof(GetCita), new { id = cita.CitaId }, cita);
    }

    // PATCH api/Citas/5/estado   body: { "estado": "EN_ATENCION", "notas": "..." }
    [HttpPatch("{id:long}/estado")]
    public async Task<IActionResult> CambiarEstado(long id, CambiarEstadoRequest request, CancellationToken ct)
    {
        return Ok(await _service.CambiarEstadoAsync(id, request, ct));
    }

    // GET api/Citas/estados
    [HttpGet("estados")]
    public async Task<IActionResult> GetEstados(CancellationToken ct)
    {
        return Ok(await _service.GetEstadosAsync(ct));
    }

    // GET api/Citas/resumen?fecha=2026-11-20&medicoId=1  (para el dashboard)
    [HttpGet("resumen")]
    public async Task<IActionResult> GetResumen(DateOnly? fecha, int? medicoId, CancellationToken ct)
    {
        var dia = fecha ?? DateOnly.FromDateTime(DateTime.Today);
        return Ok(await _service.GetResumenAsync(dia, medicoId, ct));
    }
}