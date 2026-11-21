using MsAgenda.Core.Dtos;
using MsAgenda.Core.Entities;
using MsAgenda.Core.Exceptions;
using MsAgenda.Core.Interfaces;

namespace MsAgenda.Core.Services;

// Compartir mi agenda (toda o pacientes específicos) con otro médico, con vencimiento opcional y revocación
public interface IAccesoService
{
    Task<List<AccesoAgendaDto>> CompartidosPorMiAsync(UsuarioAgenda usuario, CancellationToken ct);
    Task<List<AccesoAgendaDto>> CompartidosConmigoAsync(UsuarioAgenda usuario, CancellationToken ct);
    Task<AccesoAgendaDto> CompartirAsync(CompartirAgendaRequest request, UsuarioAgenda usuario, CancellationToken ct);
    Task<AccesoAgendaDto> RevocarAsync(int accesoId, UsuarioAgenda usuario, CancellationToken ct);
    Task<List<MedicoPermitidoDto>> MedicosPermitidosAsync(UsuarioAgenda usuario, CancellationToken ct);
    Task<List<AccesoPacienteDto>> MisPacientesAsync(UsuarioAgenda usuario, CancellationToken ct);
    Task<PuedeVerDto> PuedeVerPacienteAsync(long pacienteId, UsuarioAgenda usuario, CancellationToken ct);
}

public class AccesoService : IAccesoService
{
    private readonly IAgendaRepository _repo;
    private readonly IDirectoryService _directory;
    private readonly IPermisosAgenda _permisos;

    public AccesoService(IAgendaRepository repo, IDirectoryService directory, IPermisosAgenda permisos)
    {
        _repo = repo;
        _directory = directory;
        _permisos = permisos;
    }

    public async Task<List<AccesoAgendaDto>> CompartidosPorMiAsync(UsuarioAgenda u, CancellationToken ct)
    {
        var accesos = await _repo.GetAccesosComoPropietarioAsync(MedicoDe(u), ct);
        return accesos.Select(ToDto).ToList();
    }

    public async Task<List<AccesoAgendaDto>> CompartidosConmigoAsync(UsuarioAgenda u, CancellationToken ct)
    {
        var accesos = await _repo.GetAccesosComoAutorizadoAsync(MedicoDe(u), ct);
        return accesos.Select(ToDto).ToList();
    }

    public async Task<AccesoAgendaDto> CompartirAsync(CompartirAgendaRequest r, UsuarioAgenda u, CancellationToken ct)
    {
        var propio = MedicoDe(u);

        if (r.MedicoAutorizadoId == propio)
            throw new DomainException(Errores.DATOS_INVALIDOS, "No puedes compartir tu agenda contigo mismo.");

        var alcance = (r.Alcance ?? string.Empty).Trim().ToUpperInvariant();
        if (alcance is not (AlcancesAcceso.TODA or AlcancesAcceso.PACIENTES))
            throw new DomainException(Errores.DATOS_INVALIDOS, "Elige si compartes toda la agenda o pacientes específicos.");

        if (r.FechaVencimiento.HasValue && r.FechaVencimiento.Value <= DateTime.Now)
            throw new DomainException(Errores.DATOS_INVALIDOS, "La fecha de vencimiento debe ser futura.");

        var autorizado = await _directory.GetMedicoAsync(r.MedicoAutorizadoId, ct)
            ?? throw new DomainException(Errores.NO_ENCONTRADO, "El médico con quien quieres compartir no existe.", 404);
        if (!autorizado.Activo)
            throw new DomainException(Errores.DATOS_INVALIDOS, "Ese médico está inactivo.");

        var propietario = await _directory.GetMedicoAsync(propio, ct);

        // Solo se pueden compartir pacientes que son míos (que tienen o tuvieron cita conmigo)
        var pacientes = new List<AccesoPacienteDto>();
        if (alcance == AlcancesAcceso.PACIENTES)
        {
            var ids = (r.PacienteIds ?? new List<long>()).Distinct().ToList();
            if (ids.Count == 0)
                throw new DomainException(Errores.DATOS_INVALIDOS, "Elige al menos un paciente para compartir.");

            var mios = await _repo.GetPacientesDeMedicoAsync(propio, ct);
            var ajenos = ids.Where(id => mios.All(m => m.PacienteId != id)).ToList();
            if (ajenos.Count > 0)
                throw new DomainException(Errores.DATOS_INVALIDOS, "Solo puedes compartir pacientes que tienen o tuvieron cita contigo.");

            pacientes = mios.Where(m => ids.Contains(m.PacienteId)).ToList();
        }

        // Si ya existe un acceso vigente con ese médico, se actualiza (no se duplica)
        var acceso = await _repo.GetAccesoActivoEntreAsync(propio, r.MedicoAutorizadoId, ct);
        var nuevo = acceso is null;
        acceso ??= new AccesoAgenda
        {
            MedicoPropietarioId = propio,
            MedicoAutorizadoId = r.MedicoAutorizadoId,
            FechaCreacion = DateTime.Now,
            Activo = true
        };

        acceso.MedicoPropietarioNombre = propietario?.NombreCompleto ?? u.Nombre ?? string.Empty;
        acceso.MedicoAutorizadoNombre = autorizado.NombreCompleto;
        acceso.Alcance = alcance;
        acceso.FechaVencimiento = r.FechaVencimiento;
        acceso.Motivo = string.IsNullOrWhiteSpace(r.Motivo) ? null : r.Motivo.Trim();

        _repo.QuitarPacientesDeAcceso(acceso);
        foreach (var p in pacientes)
            acceso.Pacientes.Add(new AccesoPaciente { PacienteId = p.PacienteId, PacienteNombre = p.PacienteNombre });

        if (nuevo) await _repo.AddAccesoAsync(acceso, ct);
        else await _repo.GuardarCambiosAsync(ct);

        return ToDto(acceso);
    }

    public async Task<AccesoAgendaDto> RevocarAsync(int accesoId, UsuarioAgenda u, CancellationToken ct)
    {
        var acceso = await _repo.GetAccesoAsync(accesoId, ct)
            ?? throw new DomainException(Errores.NO_ENCONTRADO, "Ese acceso no existe.", 404);

        // Lo revoca quien compartió (o el Administrador)
        if (!u.EsAdmin && acceso.MedicoPropietarioId != u.MedicoId)
            throw new DomainException(Errores.SIN_PERMISO, "Solo quien compartió la agenda puede revocar el acceso.", 403);

        if (acceso.Activo)
        {
            acceso.Activo = false;
            acceso.FechaRevocacion = DateTime.Now;
            await _repo.GuardarCambiosAsync(ct);
        }
        return ToDto(acceso);
    }

    public async Task<List<MedicoPermitidoDto>> MedicosPermitidosAsync(UsuarioAgenda u, CancellationToken ct)
    {
        var permiso = await _permisos.ObtenerAsync(u, ct);
        if (permiso.Todo) return new List<MedicoPermitidoDto>();   // lista vacía = todos

        var lista = permiso.MedicosCompletos
            .Select(id => new MedicoPermitidoDto(id, permiso.EsPropia(id), true, new List<long>()))
            .ToList();
        lista.AddRange(permiso.PacientesPorMedico
            .Where(p => !permiso.MedicosCompletos.Contains(p.Key))
            .Select(p => new MedicoPermitidoDto(p.Key, false, false, p.Value.ToList())));
        return lista.OrderByDescending(m => m.Propio).ToList();
    }

    public Task<List<AccesoPacienteDto>> MisPacientesAsync(UsuarioAgenda u, CancellationToken ct) =>
        _repo.GetPacientesDeMedicoAsync(MedicoDe(u), ct);

    // ¿Puede esta persona ver el historial clínico de este paciente? (lo pregunta MS_Clinical)
    public async Task<PuedeVerDto> PuedeVerPacienteAsync(long pacienteId, UsuarioAgenda u, CancellationToken ct)
    {
        if (u.EsAdmin) return new PuedeVerDto(true, "Administrador");
        if (u.EsRecepcion) return new PuedeVerDto(false, "Recepción no tiene acceso al historial clínico.");

        var permiso = await _permisos.ObtenerAsync(u, ct);

        // Pacientes compartidos explícitamente
        if (permiso.PacientesPorMedico.Values.Any(p => p.Contains(pacienteId)))
            return new PuedeVerDto(true, "Paciente compartido contigo.");

        // Pacientes con cita (pasada o futura) con el médico, o con un médico que le compartió toda su agenda,
        // o (enfermería) con un médico al que asiste
        if (permiso.MedicosCompletos.Count > 0 && await _repo.ExisteCitaAsync(permiso.MedicosCompletos, pacienteId, ct))
            return new PuedeVerDto(true, "El paciente tiene cita contigo o con una agenda autorizada.");

        return new PuedeVerDto(false, "Este paciente no tiene citas contigo y nadie te ha compartido su historial.");
    }

    // ---------- Ayudantes ----------
    private static int MedicoDe(UsuarioAgenda u) =>
        u.EsMedico && u.MedicoId is int id
            ? id
            : throw new DomainException(Errores.SIN_PERMISO, "Solo los médicos pueden compartir su agenda.", 403);

    private static AccesoAgendaDto ToDto(AccesoAgenda a)
    {
        var estado = !a.Activo ? "REVOCADO" : a.Vigente(DateTime.Now) ? "VIGENTE" : "VENCIDO";
        return new AccesoAgendaDto(
            a.AccesoAgendaId, a.MedicoPropietarioId, a.MedicoPropietarioNombre, a.MedicoAutorizadoId, a.MedicoAutorizadoNombre,
            a.Alcance, a.FechaVencimiento, a.Motivo, estado, a.FechaCreacion, a.FechaRevocacion,
            a.Pacientes.OrderBy(p => p.PacienteNombre).Select(p => new AccesoPacienteDto(p.PacienteId, p.PacienteNombre)).ToList());
    }
}
