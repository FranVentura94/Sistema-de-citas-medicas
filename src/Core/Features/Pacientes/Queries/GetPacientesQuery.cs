using Domain.Entities;
using MediatR;

namespace Core.Features.Pacientes.Queries;

public record GetPacientesQuery : IRequest<List<Paciente>>;