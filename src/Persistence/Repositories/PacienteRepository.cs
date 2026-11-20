using Core.Features.Pacientes.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repositories;

public class PacienteRepository : IPacienteRepository
{
    private readonly ClinicaDbContext _context;

    public PacienteRepository(ClinicaDbContext context)
    {
        _context = context;
    }

    public async Task<List<Paciente>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _context.Pacientes
            .AsNoTracking()
            .OrderByDescending(p => p.PacienteID)
            .ToListAsync(cancellationToken);
    }

    public async Task<Paciente?> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        return await _context.Pacientes
            .FirstOrDefaultAsync(p => p.PacienteID == id, cancellationToken);
    }

    public async Task<bool> ExisteDocumentoAsync(string numeroDocumento, long? excluirPacienteId, CancellationToken cancellationToken)
    {
        return await _context.Pacientes.AnyAsync(p =>
            p.NumeroDocumento == numeroDocumento &&
            (excluirPacienteId == null || p.PacienteID != excluirPacienteId), cancellationToken);
    }

    public async Task<Paciente> AddAsync(
        Paciente paciente,
        CancellationToken cancellationToken)
    {
        await _context.Pacientes.AddAsync(paciente, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return paciente;
    }

    public async Task UpdateAsync(Paciente paciente, CancellationToken cancellationToken)
    {
        _context.Pacientes.Update(paciente);
        await _context.SaveChangesAsync(cancellationToken);
    }
}