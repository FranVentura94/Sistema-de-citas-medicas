using Core.Features.Pacientes.Interfaces;
using Domain.Entities;
using GenericPersistence.FiltroDinamico;
using MediatR;

namespace Core.Features.Pacientes.Queries;

public class GetPacienteOneQueryHandler : IRequestHandler<GetPacienteOneQuery, Paciente?>
{
    private readonly IPacienteRepository _pacienteRepository;

    public GetPacienteOneQueryHandler(IPacienteRepository pacienteRepository)
    {
        _pacienteRepository = pacienteRepository;
    }

    public async Task<Paciente?> Handle(GetPacienteOneQuery request, CancellationToken cancellationToken)
    {
        var filtro = Filter.FromStringExpression<Paciente>(request.Filter);

        return await _pacienteRepository.GetOneAsync(filtro, cancellationToken: cancellationToken);
    }
}
