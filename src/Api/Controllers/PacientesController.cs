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

    [HttpGet("paginado")]
    public async Task<IActionResult> GetPacientesPaginado(
        int pageNumber = 1,
        int pageSize = 10,
        string? filter = null,
        string? orderBy = null,
        CancellationToken cancellationToken = default)
    {
        var resultado = await _mediator.Send(
            new GetPacientesPagedQuery { PageNumber = pageNumber, PageSize = pageSize, Filter = filter, OrderBy = orderBy },
            cancellationToken);

        return Ok(resultado);
    }

    [HttpGet("uno")]
    public async Task<IActionResult> GetPacienteUno(
        string filter,
        CancellationToken cancellationToken)
    {
        var paciente = await _mediator.Send(
            new GetPacienteOneQuery(filter),
            cancellationToken);

        return paciente is null ? NotFound() : Ok(paciente);
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

    [HttpPost("rango")]
    public async Task<IActionResult> CreatePacientesRango(
        List<CreatePacienteCommand> pacientes,
        CancellationToken cancellationToken)
    {
        var creados = await _mediator.Send(
            new CreatePacientesRangeCommand(pacientes),
            cancellationToken);

        return Ok(creados);
    }
}
