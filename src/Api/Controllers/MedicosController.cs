using Core.Features.Medicos.Commands;
using Core.Features.Medicos.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MedicosController : ControllerBase
{
    private readonly IMediator _mediator;

    public MedicosController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // GET api/Medicos  o  api/Medicos?especialidadId=2
    [HttpGet]
    public async Task<IActionResult> GetMedicos(int? especialidadId, CancellationToken cancellationToken)
    {
        var medicos = await _mediator.Send(new GetMedicosQuery(especialidadId), cancellationToken);
        return Ok(medicos);
    }

    // GET api/Medicos/1
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetMedico(int id, CancellationToken cancellationToken)
    {
        var medico = await _mediator.Send(new GetMedicoByIdQuery(id), cancellationToken);
        return Ok(medico);
    }

    // POST api/Medicos  (solo Administrador)
    [HttpPost]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> CreateMedico(CreateMedicoCommand command, CancellationToken cancellationToken)
    {
        var medico = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetMedico), new { id = medico.MedicoID }, medico);
    }
}