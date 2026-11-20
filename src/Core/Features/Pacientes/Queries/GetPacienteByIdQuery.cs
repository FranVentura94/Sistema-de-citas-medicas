using Core.Exceptions;
using Core.Features.Pacientes.Interfaces;
using Domain.Entities;
using MediatR;

namespace Core.Features.Pacientes.Queries;

public record GetPacienteByIdQuery(long PacienteID) : IRequest<Paciente>;

public class GetPacienteByIdQueryHandler : IRequestHandler<GetPacienteByIdQuery, Paciente>
{
    private readonly IPacienteRepository _repository;

    public GetPacienteByIdQueryHandler(IPacienteRepository repository)
    {
        _repository = repository;
    }

    public async Task<Paciente> Handle(GetPacienteByIdQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetByIdAsync(request.PacienteID, cancellationToken)
            ?? throw new DomainException(Errores.RECURSO_NO_ENCONTRADO,
                $"No existe el paciente {request.PacienteID}.");
    }
}