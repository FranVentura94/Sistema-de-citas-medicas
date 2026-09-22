using Core.Features.Pacientes.Commands;
using Core.Features.Pacientes.Queries;
using Domain.Entities;
using BuildingBlocks.Persistence;
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

    // --- METODO NUEVO: PAGINACION GENERICA CON FILTRO Y ORDEN DINAMICOS ---
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

    // --- METODO NUEVO: BUSQUEDA GENERICA DE UN SOLO REGISTRO ---
    [HttpGet("uno")]
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

    // --- METODO NUEVO: GUARDADO GENERICO POR RANGO (LISTA) ---
    [HttpPost("rango")]
    public async Task<IActionResult> CrearPacientesEnRango(
        List<Paciente> pacientes,
        CancellationToken cancellationToken)
    {
        await _pacienteRepo.AddRangeAsync(pacientes);
        await _pacienteRepo.SaveChangesAsync();

        return Ok(pacientes);
    }
}