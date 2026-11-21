using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MsClinical.Core.Dtos;
using MsClinical.Core.Services;
using BuildingBlocks.Auditoria;

namespace MsClinical.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Medico,Administrador")]
public class AtencionesController : ControllerBase
{
    private readonly IAtencionService _service;

    public AtencionesController(IAtencionService service)
    {
        _service = service;
    }

    // Datos del usuario tomados del token JWT
    private UsuarioActual Usuario => User.Clinico();

    // POST api/Atenciones/iniciar   body: { "citaId": 1 }
    [HttpPost("iniciar")]
    public async Task<IActionResult> Iniciar(IniciarAtencionRequest request, CancellationToken ct) =>
        Ok(await _service.IniciarAsync(request.CitaId, Usuario, ct));

    // GET api/Atenciones/5
    [HttpGet("{id:long}")]
    [Auditar("Consultó una atención médica", "Atencion", "id")]
    public async Task<IActionResult> Get(long id, CancellationToken ct) =>
        Ok(await _service.GetAsync(id, Usuario, ct));

    // GET api/Atenciones/por-cita/1
    [HttpGet("por-cita/{citaId:long}")]
    [Auditar("Consultó la atención médica de una cita", "Cita", "citaId")]
    public async Task<IActionResult> GetPorCita(long citaId, CancellationToken ct) =>
        Ok(await _service.GetPorCitaAsync(citaId, Usuario, ct));

    // GET api/Atenciones/mis-atenciones?fecha=2026-11-20
    [HttpGet("mis-atenciones")]
    public async Task<IActionResult> MisAtenciones(DateOnly? fecha, CancellationToken ct) =>
        Ok(await _service.GetMisAtencionesAsync(Usuario, fecha, ct));

    // PUT api/Atenciones/5/signos-vitales
    [HttpPut("{id:long}/signos-vitales")]
    public async Task<IActionResult> SignosVitales(long id, SignosVitalesRequest request, CancellationToken ct) =>
        Ok(await _service.RegistrarSignosAsync(id, request, Usuario, ct));

    // POST api/Atenciones/5/notas
    [HttpPost("{id:long}/notas")]
    public async Task<IActionResult> AgregarNota(long id, NotaRequest request, CancellationToken ct) =>
        Ok(await _service.AgregarNotaAsync(id, request, Usuario, ct));

    // POST api/Atenciones/5/diagnosticos
    [HttpPost("{id:long}/diagnosticos")]
    public async Task<IActionResult> AgregarDiagnostico(long id, DiagnosticoRequest request, CancellationToken ct) =>
        Ok(await _service.AgregarDiagnosticoAsync(id, request, Usuario, ct));

    // DELETE api/Atenciones/5/diagnosticos/3
    [HttpDelete("{id:long}/diagnosticos/{diagnosticoId:int}")]
    public async Task<IActionResult> QuitarDiagnostico(long id, int diagnosticoId, CancellationToken ct) =>
        Ok(await _service.QuitarDiagnosticoAsync(id, diagnosticoId, Usuario, ct));

    // PUT api/Atenciones/5/prescripcion
    [HttpPut("{id:long}/prescripcion")]
    public async Task<IActionResult> Prescripcion(long id, PrescripcionRequest request, CancellationToken ct) =>
        Ok(await _service.GuardarPrescripcionAsync(id, request, Usuario, ct));

    // POST api/Atenciones/5/finalizar
    [HttpPost("{id:long}/finalizar")]
    public async Task<IActionResult> Finalizar(long id, FinalizarAtencionRequest request, CancellationToken ct) =>
        Ok(await _service.FinalizarAsync(id, request, Usuario, ct));
}