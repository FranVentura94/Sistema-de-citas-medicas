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
            .ToListAsync(cancellationToken);
    }

    public async Task<Paciente> AddAsync(
        Paciente paciente,
        CancellationToken cancellationToken)
    {
        await _context.Pacientes.AddAsync(paciente, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return paciente;
    }
}