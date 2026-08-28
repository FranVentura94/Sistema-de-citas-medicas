using Core.Features.Pacientes.Commands;
using Core.Features.Pacientes.Queries;
using Domain.Entities;
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
    public async Task<IActionResult> GetPacientes(
        CancellationToken cancellationToken)
    {
        var pacientes = await _mediator.Send(
            new GetPacientesQuery(),
            cancellationToken);

        return Ok(pacientes);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePaciente(
        CreatePacienteCommand command,
        CancellationToken cancellationToken)
    {
        var paciente = await _mediator.Send(
            command,
            cancellationToken);

        return Ok(paciente);
    }
}