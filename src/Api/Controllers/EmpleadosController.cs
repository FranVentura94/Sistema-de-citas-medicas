using System.Security.Claims;
using Core.Features.Empleados.Commands;
using Core.Features.Empleados.Dtos;
using Core.Features.Empleados.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

// Personal de la clínica: SOLO el Administrador registra y edita.
// (mi-perfil lo puede usar cualquiera con sesión, para saludarlo por su nombre)
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmpleadosController : ControllerBase
{
    private const string ADMIN = "Administrador";

    private readonly IMediator _mediator;

    public EmpleadosController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // GET api/Empleados
    [HttpGet]
    [Authorize(Roles = ADMIN)]
    public async Task<IActionResult> GetAll(CancellationToken ct) =>
        Ok(await _mediator.Send(new GetEmpleadosQuery(), ct));

    // GET api/Empleados/5
    [HttpGet("{id:int}")]
    [Authorize(Roles = ADMIN)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct) =>
        Ok(await _mediator.Send(new GetEmpleadoByIdQuery(id), ct));

    // POST api/Empleados
    [HttpPost]
    [Authorize(Roles = ADMIN)]
    public async Task<IActionResult> Crear(EmpleadoDatos datos, CancellationToken ct) =>
        Ok(await _mediator.Send(new GuardarEmpleadoCommand(null, datos), ct));

    // PUT api/Empleados/5
    [HttpPut("{id:int}")]
    [Authorize(Roles = ADMIN)]
    public async Task<IActionResult> Actualizar(int id, EmpleadoDatos datos, CancellationToken ct) =>
        Ok(await _mediator.Send(new GuardarEmpleadoCommand(id, datos), ct));

    // PUT api/Empleados/5/usuario  -> enlaza la ficha con su usuario del portal
    [HttpPut("{id:int}/usuario")]
    [Authorize(Roles = ADMIN)]
    public async Task<IActionResult> AsignarUsuario(int id, AsignarUsuarioRequest request, CancellationToken ct) =>
        Ok(await _mediator.Send(new AsignarUsuarioCommand(id, request.UsuarioID), ct));

    // GET api/Empleados/mi-perfil  -> cualquier usuario con sesión: sus datos para saludarlo
    [HttpGet("mi-perfil")]
    public async Task<IActionResult> MiPerfil(CancellationToken ct)
    {
        var usuarioId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
        return Ok(await _mediator.Send(new GetMiPerfilQuery(usuarioId), ct));
    }
}