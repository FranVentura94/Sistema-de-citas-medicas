using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MsAgenda.Core.Dtos;
using MsAgenda.Core.Services;

namespace MsAgenda.Api.Controllers;

// Compartir mi agenda con otro médico (toda o pacientes específicos), con vencimiento y revocación
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccesosController : ControllerBase
{
    private readonly IAccesoService _service;

    public AccesosController(IAccesoService service)
    {
        _service = service;
    }

    // GET api/Accesos/compartidos  -> lo que yo compartí
    [HttpGet("compartidos")]
    [Authorize(Roles = "Medico")]
    public async Task<IActionResult> Compartidos(CancellationToken ct) =>
        Ok(await _service.CompartidosPorMiAsync(User.Agenda(), ct));

    // GET api/Accesos/recibidos  -> lo que me compartieron
    [HttpGet("recibidos")]
    [Authorize(Roles = "Medico")]
    public async Task<IActionResult> Recibidos(CancellationToken ct) =>
        Ok(await _service.CompartidosConmigoAsync(User.Agenda(), ct));

    // GET api/Accesos/mis-pacientes  -> pacientes que puedo compartir (tienen o tuvieron cita conmigo)
    [HttpGet("mis-pacientes")]
    [Authorize(Roles = "Medico")]
    public async Task<IActionResult> MisPacientes(CancellationToken ct) =>
        Ok(await _service.MisPacientesAsync(User.Agenda(), ct));

    // POST api/Accesos  -> compartir (si ya había un acceso vigente con ese médico, se actualiza)
    [HttpPost]
    [Authorize(Roles = "Medico")]
    public async Task<IActionResult> Compartir(CompartirAgendaRequest request, CancellationToken ct) =>
        Ok(await _service.CompartirAsync(request, User.Agenda(), ct));

    // DELETE api/Accesos/5  -> revocar
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Medico,Administrador")]
    public async Task<IActionResult> Revocar(int id, CancellationToken ct) =>
        Ok(await _service.RevocarAsync(id, User.Agenda(), ct));

    // GET api/Accesos/medicos-permitidos  -> agendas que puedo ver/gestionar (lista vacía = todas)
    [HttpGet("medicos-permitidos")]
    public async Task<IActionResult> MedicosPermitidos(CancellationToken ct) =>
        Ok(await _service.MedicosPermitidosAsync(User.Agenda(), ct));

    // GET api/Accesos/puede-ver/3  -> ¿puedo ver el historial clínico de este paciente? (lo usa MS_Clinical)
    [HttpGet("puede-ver/{pacienteId:long}")]
    public async Task<IActionResult> PuedeVer(long pacienteId, CancellationToken ct) =>
        Ok(await _service.PuedeVerPacienteAsync(pacienteId, User.Agenda(), ct));
}
