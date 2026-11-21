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


    // ---------- Administración ----------
    public Task<bool> ExisteEspecialidadAsync(string codigo, string nombre, CancellationToken cancellationToken) =>
        _context.Especialidades.AnyAsync(e => e.Codigo == codigo || e.Nombre == nombre, cancellationToken);

    public async Task AddEspecialidadAsync(Especialidad especialidad, CancellationToken cancellationToken)
    {
        await _context.Especialidades.AddAsync(especialidad, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Dictionary<int, string?>> GetGenerosAsync(IEnumerable<int> medicoIds, CancellationToken cancellationToken)
    {
        var ids = medicoIds.Distinct().ToList();
        var filas = await _context.Empleados.AsNoTracking()
            .Where(e => e.MedicoID != null && ids.Contains(e.MedicoID.Value))
            .Select(e => new { MedicoID = e.MedicoID!.Value, e.Genero })
            .ToListAsync(cancellationToken);
        return filas.GroupBy(f => f.MedicoID).ToDictionary(g => g.Key, g => g.First().Genero);
    }
}
