using Core.Features.Pacientes.Commands;
using Core.Features.Pacientes.Queries;
using Domain.Entities;
using Domain.Interfaces; // IMPORTANTE: Agregamos esta referencia
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PacientesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IGenericRepository<Paciente> _pacienteRepo; // Agregamos el repositorio genérico

    public PacientesController(IMediator mediator, IGenericRepository<Paciente> pacienteRepo)
    {
        _mediator = mediator;
        _pacienteRepo = pacienteRepo; // Inyectamos el servicio
    }

    // --- TUS METODOS ORIGINALES (INTACTOS) ---
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

    // --- METODO NUEVO DE PRUEBA PARA VALIDAR EL REPOSITORIO GENERICO ---
    [HttpGet("test-generico")]
    public async Task<IActionResult> GetPacientesGenerico()
    {
        var pacientes = await _pacienteRepo.GetAllAsync();
        return Ok(pacientes);
    }
}