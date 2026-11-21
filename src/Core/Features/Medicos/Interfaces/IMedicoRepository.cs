using Domain.Entities;

namespace Core.Features.Medicos.Interfaces;

public interface IMedicoRepository
{
    // Trae los médicos con sus especialidades; opcionalmente filtra por especialidad
    Task<List<Medico>> GetAllAsync(int? especialidadId, CancellationToken cancellationToken);

    Task<Medico?> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<Medico> AddAsync(Medico medico, CancellationToken cancellationToken);

    Task<List<Especialidad>> GetEspecialidadesAsync(CancellationToken cancellationToken);

    // Género de cada médico según su ficha de empleado (para "Dr." / "Dra.")
    Task<Dictionary<int, string?>> GetGenerosAsync(IEnumerable<int> medicoIds, CancellationToken cancellationToken);

    // ---------- Administración ----------
    Task<bool> ExisteEspecialidadAsync(string codigo, string nombre, CancellationToken cancellationToken);
    Task AddEspecialidadAsync(Especialidad especialidad, CancellationToken cancellationToken);
}