using Core.Features.Pacientes.Commands;
using Core.Features.Pacientes.Queries;
using Domain.Entities;
using BuildingBlocks.Persistence;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BuildingBlocks.Auditoria;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Todos los endpoints requieren token, salvo los marcados con [AllowAnonymous]
public class PacientesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IGenericRepository<Paciente> _pacienteRepo;

    public PacientesController(IMediator mediator, IGenericRepository<Paciente> pacienteRepo)
    {
        _mediator = mediator;
        _pacienteRepo = pacienteRepo;
    }

    // GET api/Pacientes
    [HttpGet]
    public async Task<IActionResult> GetPacientes(CancellationToken cancellationToken)
    {
        var pacientes = await _mediator.Send(new GetPacientesQuery(), cancellationToken);
        return Ok(pacientes);
    }

    // GET api/Pacientes/5
    [HttpGet("{id:long}")]

    [Auditar("Consultó la ficha del paciente", "Paciente", "id")]
    public async Task<IActionResult> GetPaciente(long id, CancellationToken cancellationToken)
    {
        var paciente = await _mediator.Send(new GetPacienteByIdQuery(id), cancellationToken);
        return Ok(paciente);
    }

    // POST api/Pacientes
    [HttpPost]
    public async Task<IActionResult> CreatePaciente(CreatePacienteCommand command, CancellationToken cancellationToken)
    {
        var paciente = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetPaciente), new { id = paciente.PacienteID }, paciente);
    }

    // PUT api/Pacientes/5
    [HttpPut("{id:long}")]
    public async Task<IActionResult> UpdatePaciente(long id, UpdatePacienteCommand command, CancellationToken cancellationToken)
    {
        if (id != command.PacienteID)
            return BadRequest(new { mensaje = "El id de la ruta no coincide con el del paciente." });

        var paciente = await _mediator.Send(command, cancellationToken);
        return Ok(paciente);
    }

    // --- PRUEBA DEL REPOSITORIO GENERICO ---
    [HttpGet("test-generico")]
    public async Task<IActionResult> GetPacientesGenerico()
    {
        var pacientes = await _pacienteRepo.GetAllAsync();
        return Ok(pacientes);
    }

    // --- GUARDADO GENERICO POR RANGO (LISTA) ---
    [HttpPost("rango")]
    public async Task<IActionResult> CrearPacientesEnRango(
        List<Paciente> pacientes,
        CancellationToken cancellationToken)
    {
        await _pacienteRepo.AddRangeAsync(pacientes);
        await _pacienteRepo.SaveChangesAsync();

        return Ok(pacientes);
    }

    // GET api/Pacientes/paginado?pageNumber=1&pageSize=10&filtro=...&ordenarPor=...
    [HttpGet("paginado")]
    public async Task<IActionResult> GetPacientesPaginado(
        int pageNumber = 1,
        int pageSize = 10,
        string? filtro = null,
        string? ordenarPor = null,
        CancellationToken cancellationToken = default)
    {
        var filterExpression = string.IsNullOrWhiteSpace(filtro)
            ? null
            : Filter.FromStringExpression<Paciente>(filtro);

        var resultado = await _pacienteRepo.GetPagedAsync(
            pageNumber,
            pageSize,
            filter: filterExpression,
            orderBy: ordenarPor,
            cancellationToken: cancellationToken);

        return Ok(resultado);
    }

    // GET api/Pacientes/uno?numeroDocumento=...
    // Lo consume el microservicio Notificaciones (llamada interna, sin token)
    [HttpGet("uno")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPacienteUno(
        string numeroDocumento,
        CancellationToken cancellationToken)
    {
        var paciente = await _pacienteRepo.GetOneByAsync(
            p => p.NumeroDocumento == numeroDocumento,
            cancellationToken: cancellationToken);

        if (paciente == null)
            return NotFound();

        return Ok(paciente);
    }
}