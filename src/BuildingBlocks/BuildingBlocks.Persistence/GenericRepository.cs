using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BuildingBlocks.Persistence;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly DbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(DbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    public async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public void Delete(T entity)
    {
        _dbSet.Remove(entity);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<PagedResult<T>> GetPagedAsync(
    int pageNumber,
    int pageSize,
    Expression<Func<T, bool>>? filter = null,
    Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
    bool asNoTracking = true,
    CancellationToken cancellationToken = default,
    params Expression<Func<T, object>>[] includes)
    {
        if (pageNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(pageNumber), "pageNumber debe ser 1 o mayor.");
        if (pageSize < 1)
            throw new ArgumentOutOfRangeException(nameof(pageSize), "pageSize debe ser 1 o mayor.");

        IQueryable<T> query = asNoTracking ? _dbSet.AsNoTracking() : _dbSet;

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        if (filter != null)
            query = query.Where(filter);

        int totalRecords = await query.CountAsync(cancellationToken);

        if (orderBy != null)
            query = orderBy(query);

        query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);

        var data = await query.ToListAsync(cancellationToken);

        return new PagedResult<T>
        {
            Data = data,
            TotalRecords = totalRecords,
            PageSize = pageSize,
            CurrentPage = pageNumber
        };
    }
}