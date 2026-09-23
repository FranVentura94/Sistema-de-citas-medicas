using System.Linq.Expressions;
using GenericPersistence.Models;

namespace GenericPersistence.Abstractions;

/// <summary>
/// Contrato genérico de persistencia. Define las operaciones que puede
/// ejecutar cualquier entidad (TEntity) identificada por una clave (TKey),
/// sin necesidad de escribir un repositorio específico por cada entidad.
/// Vive en una biblioteca independiente para poder reutilizarse desde
/// distintos microservicios con solo agregar la referencia de proyecto.
/// </summary>
/// <typeparam name="TEntity">Tipo de la entidad de dominio (ej. Paciente, Medico, Citas).</typeparam>
/// <typeparam name="TKey">Tipo de la clave primaria de la entidad (ej. long, int).</typeparam>
public interface IRepository<TEntity, TKey> where TEntity : class
{
    /// <summary>Obtiene todos los registros de la entidad.</summary>
    Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>Obtiene un registro por su clave primaria, o null si no existe.</summary>
    Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken);

    /// <summary>
    /// GetOneBy genérico: obtiene el primer registro que cumple un filtro
    /// (Expression), igual al filtro genérico que usa GetPagedAsync. Devuelve
    /// null si ningún registro cumple la condición.
    /// </summary>
    Task<TEntity?> GetOneAsync(
        Expression<Func<TEntity, bool>> filter,
        bool asNoTracking = true,
        CancellationToken cancellationToken = default,
        params Expression<Func<TEntity, object>>[] includes);

    /// <summary>Inserta un nuevo registro y devuelve la entidad creada.</summary>
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken);

    /// <summary>
    /// Inserta un rango (lista) de registros en una sola operación y devuelve
    /// las entidades creadas.
    /// </summary>
    Task<List<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken);

    /// <summary>Actualiza un registro existente y devuelve la entidad actualizada.</summary>
    Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken);

    /// <summary>Elimina el registro con la clave indicada. Devuelve false si no existía.</summary>
    Task<bool> DeleteAsync(TKey id, CancellationToken cancellationToken);

    /// <summary>
    /// Paginación genérica: devuelve una página de resultados con el total de
    /// registros. Admite filtro genérico (Expression), orden dinámico por
    /// cualquier campo (texto, ej. "Nombres desc" o "Apellidos, Nombres desc"
    /// vía System.Linq.Dynamic.Core), seguimiento de cambios, consulta
    /// dividida e includes de navegación.
    /// </summary>
    Task<PagedResult<TEntity>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<TEntity, bool>>? filter = null,
        string? orderBy = null,
        bool asNoTracking = true,
        bool splitQuery = false,
        CancellationToken cancellationToken = default,
        params Expression<Func<TEntity, object>>[] includes);
}
