using Core.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repositories;

/// <summary>
/// Implementación genérica de <see cref="IRepository{TEntity, TKey}"/> sobre
/// Entity Framework Core sobre el ClinicaDbContext. Al ser genérica en
/// TEntity y TKey, una sola clase resuelve el CRUD de Paciente, Medico,
/// Citas, Atenciones (y cualquier entidad futura) sin duplicar código.
/// </summary>
public class Repository<TEntity, TKey> : IRepository<TEntity, TKey>
    where TEntity : class
{
    private readonly ClinicaDbContext _context;
    private readonly DbSet<TEntity> _dbSet;

    public Repository(ClinicaDbContext context)
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

    public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return entity;
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
}
