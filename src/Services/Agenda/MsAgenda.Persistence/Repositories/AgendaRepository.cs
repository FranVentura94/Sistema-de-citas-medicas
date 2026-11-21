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

    public Task<HorarioMedico?> GetHorarioAsync(int horarioId, CancellationToken ct) =>
    _db.HorariosMedicos.FirstOrDefaultAsync(h => h.HorarioMedicoId == horarioId && h.Activo, ct);


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

    // ---------- Agendas compartidas ----------
    public Task<List<AccesoAgenda>> GetAccesosComoPropietarioAsync(int medicoId, CancellationToken ct) =>
        _db.AccesosAgenda.AsNoTracking().Include(a => a.Pacientes)
            .Where(a => a.MedicoPropietarioId == medicoId)
            .OrderByDescending(a => a.Activo).ThenByDescending(a => a.FechaCreacion)
            .ToListAsync(ct);

    public Task<List<AccesoAgenda>> GetAccesosComoAutorizadoAsync(int medicoId, CancellationToken ct) =>
        _db.AccesosAgenda.AsNoTracking().Include(a => a.Pacientes)
            .Where(a => a.MedicoAutorizadoId == medicoId)
            .OrderByDescending(a => a.Activo).ThenByDescending(a => a.FechaCreacion)
            .ToListAsync(ct);

    public Task<AccesoAgenda?> GetAccesoAsync(int accesoId, CancellationToken ct) =>
        _db.AccesosAgenda.Include(a => a.Pacientes).FirstOrDefaultAsync(a => a.AccesoAgendaId == accesoId, ct);

    public Task<AccesoAgenda?> GetAccesoActivoEntreAsync(int propietarioId, int autorizadoId, CancellationToken ct) =>
        _db.AccesosAgenda.Include(a => a.Pacientes)
            .Where(a => a.MedicoPropietarioId == propietarioId && a.MedicoAutorizadoId == autorizadoId && a.Activo
                        && (a.FechaVencimiento == null || a.FechaVencimiento > DateTime.Now))
            .OrderByDescending(a => a.FechaCreacion)
            .FirstOrDefaultAsync(ct);

    public async Task AddAccesoAsync(AccesoAgenda acceso, CancellationToken ct)
    {
        await _db.AccesosAgenda.AddAsync(acceso, ct);
        await _db.SaveChangesAsync(ct);
    }

    public void QuitarPacientesDeAcceso(AccesoAgenda acceso)
    {
        foreach (var p in acceso.Pacientes.ToList())
        {
            acceso.Pacientes.Remove(p);
            if (_db.Entry(p).State != EntityState.Detached) _db.AccesoPacientes.Remove(p);
        }
    }

    public Task<bool> ExisteCitaAsync(IEnumerable<int> medicoIds, long pacienteId, CancellationToken ct)
    {
        var ids = medicoIds.ToList();
        return _db.Citas.AnyAsync(c => c.PacienteId == pacienteId && ids.Contains(c.MedicoId), ct);
    }

    // Pacientes que tienen o tuvieron cita con el médico (los únicos que puede compartir)
    public async Task<List<AccesoPacienteDto>> GetPacientesDeMedicoAsync(int medicoId, CancellationToken ct)
    {
        var filas = await _db.Citas.AsNoTracking()
            .Where(c => c.MedicoId == medicoId)
            .Select(c => new { c.PacienteId, c.PacienteNombre })
            .Distinct()
            .ToListAsync(ct);
        return filas
            .GroupBy(f => f.PacienteId)
            .Select(g => new AccesoPacienteDto(g.Key, g.First().PacienteNombre))
            .OrderBy(p => p.PacienteNombre)
            .ToList();
    }
}
