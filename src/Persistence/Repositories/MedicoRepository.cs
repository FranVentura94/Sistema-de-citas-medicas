using Core.Features.Medicos.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repositories;

public class MedicoRepository : IMedicoRepository
{
    private readonly ClinicaDbContext _context;

    public MedicoRepository(ClinicaDbContext context)
    {
        _context = context;
    }

    public async Task<List<Medico>> GetAllAsync(int? especialidadId, CancellationToken cancellationToken)
    {
        var query = _context.Medicos
            .AsNoTracking()
            .Include(m => m.Especialidades)
                .ThenInclude(me => me.Especialidad)
            .AsQueryable();

        if (especialidadId.HasValue)
            query = query.Where(m => m.Especialidades.Any(e => e.EspecialidadID == especialidadId.Value));

        return await query
            .OrderBy(m => m.Apellidos)
            .ToListAsync(cancellationToken);
    }

    public async Task<Medico?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _context.Medicos
            .AsNoTracking()
            .Include(m => m.Especialidades)
                .ThenInclude(me => me.Especialidad)
            .FirstOrDefaultAsync(m => m.MedicoID == id, cancellationToken);
    }

    public async Task<Medico> AddAsync(Medico medico, CancellationToken cancellationToken)
    {
        await _context.Medicos.AddAsync(medico, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return medico;
    }

    public async Task<List<Especialidad>> GetEspecialidadesAsync(CancellationToken cancellationToken)
    {
        return await _context.Especialidades
            .AsNoTracking()
            .Where(e => e.Activo)
            .OrderBy(e => e.Nombre)
            .ToListAsync(cancellationToken);
    }
}