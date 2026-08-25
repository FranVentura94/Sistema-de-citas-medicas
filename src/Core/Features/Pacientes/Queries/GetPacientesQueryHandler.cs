using Core.Features.Pacientes.Interfaces;
using Domain.Entities;
using MediatR;

namespace Core.Features.Pacientes.Queries;

public class GetPacientesQueryHandler : IRequestHandler<GetPacientesQuery, List<Paciente>>
{
    private readonly IPacienteRepository _repository;

    public GetPacientesQueryHandler(IPacienteRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<Paciente>> Handle(
        GetPacientesQuery request,
        CancellationToken cancellationToken)
    {
        return await _repository.GetAllAsync(cancellationToken);
    }
}