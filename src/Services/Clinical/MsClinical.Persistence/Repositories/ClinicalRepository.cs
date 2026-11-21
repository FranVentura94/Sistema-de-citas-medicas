using Microsoft.EntityFrameworkCore;
using MsClinical.Core.Entities;
using MsClinical.Core.Interfaces;
using MsClinical.Persistence.Context;

namespace MsClinical.Persistence.Repositories;

public class ClinicalRepository : IClinicalRepository
{
    private readonly ClinicalDbContext _db;

    public ClinicalRepository(ClinicalDbContext db)
    {
        _db = db;
    }

    // ---------- Expedientes ----------
    public Task<ExpedienteClinico?> GetExpedienteAsync(long pacienteId, bool conAtenciones, CancellationToken ct)
    {
        var query = _db.Expedientes.AsQueryable();

        if (conAtenciones)
            query = query
                .Include(e => e.Atenciones)
                    .ThenInclude(a => a.Diagnosticos)
                        .ThenInclude(d => d.Diagnostico)
                .AsSplitQuery();

        return query.FirstOrDefaultAsync(e => e.PacienteId == pacienteId, ct);
    }

    public async Task AddExpedienteAsync(ExpedienteClinico expediente, CancellationToken ct)
    {
        await _db.Expedientes.AddAsync(expediente, ct);
        await _db.SaveChangesAsync(ct);
    }

    // ---------- Atenciones (con todo su detalle) ----------
    private IQueryable<Atencion> AtencionesCompletas() =>
        _db.Atenciones
            .Include(a => a.Expediente)
            .Include(a => a.SignosVitales)
            .Include(a => a.Notas)
            .Include(a => a.Diagnosticos).ThenInclude(d => d.Diagnostico)
            .Include(a => a.Prescripcion).ThenInclude(p => p!.Detalles).ThenInclude(d => d.Medicamento)
            .AsSplitQuery();

    public Task<Atencion?> GetAtencionAsync(long atencionId, CancellationToken ct) =>
        AtencionesCompletas().FirstOrDefaultAsync(a => a.AtencionId == atencionId, ct);

    public Task<Atencion?> GetAtencionPorCitaAsync(long citaId, CancellationToken ct) =>
        AtencionesCompletas().FirstOrDefaultAsync(a => a.CitaId == citaId, ct);

    public Task<List<Atencion>> GetAtencionesPorMedicoAsync(int medicoId, DateTime? desde, DateTime? hasta, CancellationToken ct)
    {
        var query = _db.Atenciones.AsNoTracking()
            .Include(a => a.Diagnosticos).ThenInclude(d => d.Diagnostico)
            .Where(a => a.MedicoId == medicoId);

        if (desde.HasValue)
            query = query.Where(a => a.FechaInicio >= desde.Value);

        if (hasta.HasValue)
            query = query.Where(a => a.FechaInicio < hasta.Value);

        return query.OrderByDescending(a => a.FechaInicio).ToListAsync(ct);
    }

    public async Task AddAtencionAsync(Atencion atencion, CancellationToken ct)
    {
        await _db.Atenciones.AddAsync(atencion, ct);
        await _db.SaveChangesAsync(ct);
    }

    // ---------- Catálogos ----------
    public Task<Diagnostico?> GetDiagnosticoAsync(int id, CancellationToken ct) =>
        _db.Diagnosticos.FirstOrDefaultAsync(d => d.DiagnosticoId == id, ct);

    public Task<List<Diagnostico>> BuscarDiagnosticosAsync(string? buscar, CancellationToken ct)
    {
        var query = _db.Diagnosticos.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(buscar))
            query = query.Where(d => d.Codigo.Contains(buscar) || d.Descripcion.Contains(buscar));

        return query.OrderBy(d => d.Codigo).Take(50).ToListAsync(ct);
    }

    public Task<List<Medicamento>> GetMedicamentosAsync(IEnumerable<int> ids, CancellationToken ct)
    {
        var lista = ids.ToList();
        return _db.Medicamentos.Where(m => lista.Contains(m.MedicamentoId)).ToListAsync(ct);
    }

    public Task<List<Medicamento>> BuscarMedicamentosAsync(string? buscar, CancellationToken ct)
    {
        var query = _db.Medicamentos.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(buscar))
            query = query.Where(m => m.Nombre.Contains(buscar) || m.PrincipioActivo.Contains(buscar));

        return query.OrderBy(m => m.Nombre).Take(50).ToListAsync(ct);
    }

    // ---------- Borrados ----------
    public void RemoveDetalles(IEnumerable<PrescripcionDetalle> detalles) => _db.PrescripcionDetalles.RemoveRange(detalles);

    public void RemoveDiagnostico(AtencionDiagnostico diagnostico) => _db.AtencionDiagnosticos.Remove(diagnostico);

    public Task GuardarCambiosAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);

    // ---------- Triaje ----------
    public Task<Triaje?> GetTriajePorCitaAsync(long citaId, CancellationToken ct) =>
        _db.Triajes.FirstOrDefaultAsync(t => t.CitaId == citaId, ct);

    public async Task AddTriajeAsync(Triaje triaje, CancellationToken ct)
    {
        await _db.Triajes.AddAsync(triaje, ct);
        await _db.SaveChangesAsync(ct);
    }
}
