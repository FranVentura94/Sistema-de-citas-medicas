using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MsAgenda.Core.Dtos;
using MsAgenda.Core.Services;

namespace MsAgenda.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HorariosController : ControllerBase
{
    private readonly IHorarioService _service;

    public HorariosController(IHorarioService service)
    {
        _service = service;
    }

    // GET api/Horarios  o  api/Horarios?medicoId=1
    [HttpGet]
    public async Task<IActionResult> GetHorarios(int? medicoId, CancellationToken ct)
    {
        return Ok(await _service.GetHorariosAsync(medicoId, ct));
    }

    // GET api/Horarios/disponibilidad?medicoId=1&fecha=2026-11-20
    [HttpGet("disponibilidad")]
    public async Task<IActionResult> GetDisponibilidad(int medicoId, DateOnly fecha, CancellationToken ct)
    {
        return Ok(await _service.GetDisponibilidadAsync(medicoId, fecha, ct));
    }

    // GET api/Horarios/consultorios
    [HttpGet("consultorios")]
    public async Task<IActionResult> GetConsultorios(CancellationToken ct)
    {
        return Ok(await _service.GetConsultoriosAsync(ct));
    }

    // POST api/Horarios  (solo Administrador)
    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> CrearHorario(CrearHorarioRequest request, CancellationToken ct)
    {
        return Ok(await _service.CrearHorarioAsync(request, ct));
    }

    // DELETE api/Horarios/5  (solo Administrador)
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> EliminarHorario(int id, CancellationToken ct)
    {
        await _service.EliminarHorarioAsync(id, ct);
        return Ok(new { mensaje = "Horario eliminado." });
    }
}