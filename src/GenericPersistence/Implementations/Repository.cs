using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using GenericPersistence.Abstractions;
using GenericPersistence.Models;
using Microsoft.EntityFrameworkCore;

namespace GenericPersistence.Implementations;

/// <summary>
/// Implementación genérica de <see cref="IRepository{TEntity, TKey}"/> sobre
/// Entity Framework Core. Recibe un <see cref="DbContext"/> por inyección de
/// dependencias, de modo que cada microservicio aporta su propio contexto y
/// una sola clase resuelve la persistencia de cualquier entidad.
/// </summary>
public class Repository<TEntity, TKey> : IRepository<TEntity, TKey>
    where TEntity : class
{
    private readonly DbContext _context;
    private readonly DbSet<TEntity> _dbSet;

    public Repository(DbContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }

    public async Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _dbSet
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken)
    {
        return await _dbSet.FindAsync(new object?[] { id }, cancellationToken);
    }

    public async Task<TEntity?> GetOneAsync(
        Expression<Func<TEntity, bool>> filter,
        bool asNoTracking = true,
        CancellationToken cancellationToken = default,
        params Expression<Func<TEntity, object>>[] includes)
    {
        var query = ApplyIncludes(Query(asNoTracking), includes).Where(filter);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return entity;
    }

    public async Task<List<TEntity>> AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken)
    {
        var lista = entities.ToList();

        await _dbSet.AddRangeAsync(lista, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return lista;
    }

    public async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity;
    }

    public async Task<bool> DeleteAsync(TKey id, CancellationToken cancellationToken)
    {
        var entity = await _dbSet.FindAsync(new object?[] { id }, cancellationToken);

        if (entity is null)
        {
            return false;
        }

        _dbSet.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    // PagedResult se califica con el namespace completo porque
    // System.Linq.Dynamic.Core también define un tipo PagedResult<T> propio;
    // sin calificarlo el compilador no sabe a cuál de los dos nos referimos.
    public async Task<GenericPersistence.Models.PagedResult<TEntity>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<TEntity, bool>>? filter = null,
        string? orderBy = null,
        bool asNoTracking = true,
        bool splitQuery = false,
        CancellationToken cancellationToken = default,
        params Expression<Func<TEntity, object>>[] includes)
    {
        if (pageNumber < 1) throw new ArgumentOutOfRangeException(nameof(pageNumber), "pageNumber must be 1 or greater.");
        if (pageSize < 1) throw new ArgumentOutOfRangeException(nameof(pageSize), "pageSize must be 1 or greater.");

        var query = ApplyIncludes(Query(asNoTracking), includes);
        if (filter != null) query = query.Where(filter);
        if (splitQuery) query = query.AsSplitQuery();

        // Count against the filtered (but not yet ordered/paged) query — computed
        // with CountAsync so it doesn't block a thread pool thread synchronously.
        int totalRecords = await query.CountAsync(cancellationToken);

        // Orden dinámico por cualquier campo: "Nombres", "Nombres desc",
        // "Apellidos, Nombres desc", etc. — resuelto en tiempo de ejecución con
        // System.Linq.Dynamic.Core, igual que el filtro dinámico.
        if (!string.IsNullOrWhiteSpace(orderBy)) query = query.OrderBy(orderBy);

        query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);

        var data = await query.ToListAsync(cancellationToken);

        return new GenericPersistence.Models.PagedResult<TEntity>
        {
            Data = data,
            TotalRecords = totalRecords,
            PageSize = pageSize,
            CurrentPage = pageNumber
        };
    }

    private IQueryable<TEntity> Query(bool asNoTracking) =>
        asNoTracking ? _dbSet.AsNoTracking() : _dbSet;

    private static IQueryable<TEntity> ApplyIncludes(IQueryable<TEntity> query, Expression<Func<TEntity, object>>[] includes)
    {
        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return query;
    }
}
