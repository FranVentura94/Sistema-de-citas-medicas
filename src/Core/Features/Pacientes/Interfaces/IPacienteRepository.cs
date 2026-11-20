using Domain.Entities;

namespace Core.Features.Pacientes.Interfaces;

public interface IPacienteRepository
{
    Task<List<Paciente>> GetAllAsync(CancellationToken cancellationToken);

    Task<Paciente?> GetByIdAsync(long id, CancellationToken cancellationToken);

    Task<bool> ExisteDocumentoAsync(string numeroDocumento, long? excluirPacienteId, CancellationToken cancellationToken);

    Task<Paciente> AddAsync(Paciente paciente, CancellationToken cancellationToken);

    Task UpdateAsync(Paciente paciente, CancellationToken cancellationToken);
}