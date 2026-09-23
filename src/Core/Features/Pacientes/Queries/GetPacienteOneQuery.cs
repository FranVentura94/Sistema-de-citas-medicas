using Domain.Entities;
using MediatR;

namespace Core.Features.Pacientes.Queries;

/// <summary>
/// GetOneBy genérico aplicado a Paciente: recibe un filtro dinámico (texto,
/// ej. x.NumeroDocumento == "12345678-9") y devuelve el primer paciente que
/// lo cumple, o null si ninguno lo cumple.
/// </summary>
public record GetPacienteOneQuery(string Filter) : IRequest<Paciente?>;
