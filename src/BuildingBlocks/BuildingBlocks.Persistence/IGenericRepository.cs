using System.Linq.Expressions;

namespace BuildingBlocks.Persistence;

public interface IGenericRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    void Update(T entity);
    void Delete(T entity);
    Task SaveChangesAsync();

    Task<PagedResult<T>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<T, bool>>? filter = null,
        string? orderBy = null,
        bool asNoTracking = true,
        CancellationToken cancellationToken = default,
        params Expression<Func<T, object>>[] includes);

    Task<T?> GetOneByAsync(
    Expression<Func<T, bool>> filter,
    bool asNoTracking = true,
    CancellationToken cancellationToken = default,
    params Expression<Func<T, object>>[] includes);
}