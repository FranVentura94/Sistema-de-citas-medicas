using Core.Features.Empleados.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repositories;

public class EmpleadoRepository : IEmpleadoRepository
{
    private readonly ClinicaDbContext _context;

    public EmpleadoRepository(ClinicaDbContext context)
    {
        _context = context;
    }

    // Empleado con su ficha de médico, especialidades y médicos asignados
    private IQueryable<Empleado> Completo() =>
        _context.Empleados
            .Include(e => e.Medico!).ThenInclude(m => m.Especialidades)
            .Include(e => e.MedicosAsignados).ThenInclude(a => a.Medico);

    public Task<List<Empleado>> GetAllAsync(CancellationToken ct) =>
        Completo().AsNoTracking().OrderBy(e => e.Nombres).ThenBy(e => e.Apellidos).ToListAsync(ct);

    public Task<Empleado?> GetByIdAsync(int id, CancellationToken ct) =>
        Completo().FirstOrDefaultAsync(e => e.EmpleadoID == id, ct);

    public Task<Empleado?> GetByUsuarioAsync(int usuarioId, CancellationToken ct) =>
        Completo().AsNoTracking().FirstOrDefaultAsync(e => e.UsuarioID == usuarioId, ct);

    public Task<bool> ExisteIdentificacionAsync(string identificacion, int? excluirId, CancellationToken ct) =>
        _context.Empleados.AnyAsync(e => e.Identificacion == identificacion && (excluirId == null || e.EmpleadoID != excluirId), ct);

    public Task<bool> UsuarioYaAsignadoAsync(int usuarioId, int excluirId, CancellationToken ct) =>
        _context.Empleados.AnyAsync(e => e.UsuarioID == usuarioId && e.EmpleadoID != excluirId, ct);

    public Task<List<int>> MedicosExistentesAsync(IEnumerable<int> ids, CancellationToken ct) =>
        _context.Medicos.Where(m => ids.Contains(m.MedicoID)).Select(m => m.MedicoID).ToListAsync(ct);

    public async Task AddAsync(Empleado empleado, CancellationToken ct)
    {
        await _context.Empleados.AddAsync(empleado, ct);
        await _context.SaveChangesAsync(ct);
    }

    public Task GuardarCambiosAsync(CancellationToken ct) => _context.SaveChangesAsync(ct);
}