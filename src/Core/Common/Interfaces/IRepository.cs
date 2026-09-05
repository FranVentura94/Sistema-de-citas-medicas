namespace Core.Common.Interfaces;

/// <summary>
/// Contrato genérico de persistencia. Define las operaciones CRUD que puede
/// ejecutar cualquier entidad (TEntity) identificada por una clave (TKey),
/// sin necesidad de escribir un repositorio específico por cada entidad
/// (ej. IPacienteRepository, IMedicoRepository, ICitasRepository, etc.).
/// </summary>
/// <typeparam name="TEntity">Tipo de la entidad de dominio (ej. Paciente, Medico, Citas).</typeparam>
/// <typeparam name="TKey">Tipo de la clave primaria de la entidad (ej. long, int).</typeparam>
public interface IRepository<TEntity, TKey> where TEntity : class
{
    /// <summary>Obtiene todos los registros de la entidad.</summary>
    Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>Obtiene un registro por su clave primaria, o null si no existe.</summary>
    Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken);

    /// <summary>Inserta un nuevo registro y devuelve la entidad creada.</summary>
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken);

    /// <summary>Actualiza un registro existente y devuelve la entidad actualizada.</summary>
    Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken);

    /// <summary>Elimina el registro con la clave indicada. Devuelve false si no existía.</summary>
    Task<bool> DeleteAsync(TKey id, CancellationToken cancellationToken);
}
