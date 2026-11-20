using Microsoft.EntityFrameworkCore;
using MsAgenda.Core.Dtos;
using MsAgenda.Core.Entities;
using MsAgenda.Core.Interfaces;
using MsAgenda.Persistence.Context;

namespace MsAgenda.Persistence.Repositories;

public class AgendaRepository : IAgendaRepository
{
    private readonly AgendaDbContext _db;

    public AgendaRepository(AgendaDbContext db)
    {
        _db = db;
    }

    // ---------- Consultorios ----------
    public Task<List<Consultorio>> GetConsultoriosAsync(CancellationToken ct) =>
        _db.Consultorios.AsNoTracking().Where(c => c.Activo).OrderBy(c => c.Codigo).ToListAsync(ct);

    public async Task<Consultorio?> GetConsultorioAsync(int id, CancellationToken ct) =>
        await _db.Consultorios.FirstOrDefaultAsync(c => c.ConsultorioId == id && c.Activo, ct);

    // ---------- Horarios ----------
    public Task<List<HorarioMedico>> GetHorariosAsync(int? medicoId, CancellationToken ct)
    {
        var query = _db.HorariosMedicos.AsNoTracking().Include(h => h.Consultorio).Where(h => h.Activo);

        if (medicoId.HasValue)
            query = query.Where(h => h.MedicoId == medicoId.Value);

        return query.OrderBy(h => h.MedicoId).ThenBy(h => h.DiaSemana).ThenBy(h => h.HoraInicio).ToListAsync(ct);
    }

    public Task<List<HorarioMedico>> GetHorariosDelDiaAsync(int medicoId, int diaSemana, CancellationToken ct) =>
        _db.HorariosMedicos.AsNoTracking()
            .Include(h => h.Consultorio)
            .Where(h => h.Activo && h.MedicoId == medicoId && h.DiaSemana == diaSemana)
            .ToListAsync(ct);

    public async Task AddHorarioAsync(HorarioMedico horario, CancellationToken ct)
    {
        await _db.HorariosMedicos.AddAsync(horario, ct);
        await _db.SaveChangesAsync(ct);
    }

    // ---------- Citas ----------
    public Task<List<Cita>> GetCitasAsync(CitaFiltro filtro, CancellationToken ct)
    {
        var query = _db.Citas.AsNoTracking()
            .Include(c => c.Estado)
            .Include(c => c.Consultorio)
            .AsQueryable();

        if (filtro.Desde.HasValue)
            query = query.Where(c => c.FechaHoraInicio >= filtro.Desde.Value);

        if (filtro.Hasta.HasValue)
            query = query.Where(c => c.FechaHoraInicio < filtro.Hasta.Value);

        if (filtro.MedicoId.HasValue)
            query = query.Where(c => c.MedicoId == filtro.MedicoId.Value);

        if (filtro.PacienteId.HasValue)
            query = query.Where(c => c.PacienteId == filtro.PacienteId.Value);

        if (!string.IsNullOrWhiteSpace(filtro.Estado))
            query = query.Where(c => c.Estado!.Codigo == filtro.Estado);

        return query.OrderBy(c => c.FechaHoraInicio).ToListAsync(ct);
    }

    public Task<Cita?> GetCitaAsync(long id, CancellationToken ct) =>
        _db.Citas
            .Include(c => c.Estado)
            .Include(c => c.Consultorio)
            .FirstOrDefaultAsync(c => c.CitaId == id, ct);

    public Task<bool> MedicoTieneCruceAsync(int medicoId, DateTime inicio, DateTime fin, CancellationToken ct) =>
        _db.Citas.AnyAsync(c =>
            c.MedicoId == medicoId &&
            !EstadosCita.Liberan.Contains(c.EstadoCitaId) &&
            inicio < c.FechaHoraFin && fin > c.FechaHoraInicio, ct);

    public Task<bool> PacienteTieneCruceAsync(long pacienteId, DateTime inicio, DateTime fin, CancellationToken ct) =>
        _db.Citas.AnyAsync(c =>
            c.PacienteId == pacienteId &&
            !EstadosCita.Liberan.Contains(c.EstadoCitaId) &&
            inicio < c.FechaHoraFin && fin > c.FechaHoraInicio, ct);

    public async Task AddCitaAsync(Cita cita, CancellationToken ct)
    {
        await _db.Citas.AddAsync(cita, ct);
        await _db.SaveChangesAsync(ct);
    }

    // ---------- Estados ----------
    public Task<List<EstadoCita>> GetEstadosAsync(CancellationToken ct) =>
        _db.Estados.OrderBy(e => e.Orden).ToListAsync(ct);

    public Task GuardarCambiosAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}