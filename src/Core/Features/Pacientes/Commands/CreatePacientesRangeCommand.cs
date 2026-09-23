using Domain.Entities;
using MediatR;

namespace Core.Features.Pacientes.Commands;

/// <summary>
/// Guarda por rango (lista) de pacientes en una sola operación, usando
/// AddRangeAsync del repositorio genérico. Reutiliza los mismos campos que
/// CreatePacienteCommand, uno por cada paciente a crear.
/// </summary>
public record CreatePacientesRangeCommand(
    List<CreatePacienteCommand> Pacientes
) : IRequest<List<Paciente>>;
