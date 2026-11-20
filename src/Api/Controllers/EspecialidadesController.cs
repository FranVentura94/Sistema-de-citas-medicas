using Core.Features.Medicos.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
}