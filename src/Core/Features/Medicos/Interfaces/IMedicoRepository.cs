using Domain.Entities;

namespace Core.Features.Medicos.Interfaces;

public interface IMedicoRepository
{
    // Trae los médicos con sus especialidades; opcionalmente filtra por especialidad
    Task<List<Medico>> GetAllAsync(int? especialidadId, CancellationToken cancellationToken);

    Task<Medico?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<Medico> AddAsync(Medico medico, CancellationToken cancellationToken);

    Task<List<Especialidad>> GetEspecialidadesAsync(CancellationToken cancellationToken);
}