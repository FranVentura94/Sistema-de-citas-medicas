using Core.Features.Pacientes.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PacientesController : ControllerBase
{
    private readonly IMediator _mediator;

    public PacientesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetPacientes(CancellationToken cancellationToken)
    {
        var pacientes = await _mediator.Send(
            new GetPacientesQuery(),
            cancellationToken);

        return Ok(pacientes);
    }
}