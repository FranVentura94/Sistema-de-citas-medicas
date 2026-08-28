using Domain.Entities;

namespace Core.Features.Pacientes.Interfaces;

public interface IPacienteRepository
{
    Task<List<Paciente>> GetAllAsync(CancellationToken cancellationToken);

    Task<Paciente> AddAsync(Paciente paciente, CancellationToken cancellationToken);
}