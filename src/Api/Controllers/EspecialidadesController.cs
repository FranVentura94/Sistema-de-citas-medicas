using Core.Features.Medicos.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Core.Features.Medicos.Commands;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EspecialidadesController : ControllerBase
{
    private readonly IMediator _mediator;

    public EspecialidadesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // GET api/Especialidades
    [HttpGet]
    public async Task<IActionResult> GetEspecialidades(CancellationToken cancellationToken)
    {
        var especialidades = await _mediator.Send(new GetEspecialidadesQuery(), cancellationToken);
        return Ok(especialidades);
    }

    // POST api/Especialidades  (solo Administrador)
    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> CreateEspecialidad(CreateEspecialidadCommand command, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.Send(command, cancellationToken));
    }
}