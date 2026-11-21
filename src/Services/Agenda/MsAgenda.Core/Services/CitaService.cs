using MsAgenda.Core.Dtos;
using MsAgenda.Core.Entities;
using MsAgenda.Core.Exceptions;
using MsAgenda.Core.Interfaces;

namespace MsAgenda.Core.Services;

public interface ICitaService
{
    Task<List<CitaDto>> GetCitasAsync(CitaFiltro filtro, UsuarioAgenda usuario, CancellationToken ct);
    Task<CitaDto> GetCitaAsync(long id, UsuarioAgenda usuario, CancellationToken ct);
    Task<CitaDto> CrearCitaAsync(CrearCitaRequest request, UsuarioAgenda usuario, CancellationToken ct);
    Task<CitaDto> CambiarEstadoAsync(long id, CambiarEstadoRequest request, UsuarioAgenda usuario, CancellationToken ct);
    Task<List<EstadoCitaDto>> GetEstadosAsync(CancellationToken ct);
    Task<ResumenCitasDto> GetResumenAsync(DateOnly fecha, int? medicoId, UsuarioAgenda usuario, CancellationToken ct);
}

public class CitaService : ICitaService
{
    // Lo que un médico que NO es el dueño de la cita (agenda compartida) puede hacerle
    private static readonly int[] EstadosParaAutorizado = { EstadosCita.CONFIRMADA, EstadosCita.EN_ESPERA, EstadosCita.CANCELADA, EstadosCita.NO_ASISTIO };
    // Lo que Enfermería puede hacerle a una cita de los médicos que asiste
    private static readonly int[] EstadosParaEnfermeria = { EstadosCita.CONFIRMADA, EstadosCita.EN_ESPERA, EstadosCita.NO_ASISTIO };

    private readonly IAgendaRepository _repo;
    private readonly IDirectoryService _directory;
    private readonly IPermisosAgenda _permisos;

    public CitaService(IAgendaRepository repo, IDirectoryService directory, IPermisosAgenda permisos)
    {
        _repo = repo;
        _directory = directory;
        _permisos = permisos;
    }

    // Cada quien ve solo las citas que le corresponden (ver PermisosAgenda)
    public async Task<List<CitaDto>> GetCitasAsync(CitaFiltro filtro, UsuarioAgenda usuario, CancellationToken ct)
    {
        var permiso = await _permisos.ObtenerAsync(usuario, ct);
        var citas = await _repo.GetCitasAsync(filtro, ct);
        return citas.Where(c => permiso.PuedeVer(c.MedicoId, c.PacienteId)).Select(ToDto).ToList();
    }

    public async Task<CitaDto> GetCitaAsync(long id, UsuarioAgenda usuario, CancellationToken ct)
    {
        var cita = await _repo.GetCitaAsync(id, ct)
            ?? throw new DomainException(Errores.NO_ENCONTRADO, $"No existe la cita {id}.", 404);

        var permiso = await _permisos.ObtenerAsync(usuario, ct);
        if (!permiso.PuedeVer(cita.MedicoId, cita.PacienteId))
            throw new DomainException(Errores.SIN_PERMISO, "No tienes acceso a esta cita.", 403);

        return ToDto(cita);
    }

    public async Task<CitaDto> CrearCitaAsync(CrearCitaRequest request, UsuarioAgenda usuario, CancellationToken ct)
    {
        // El médico agenda en su propia agenda o en las que le compartieron (si incluyen a ese paciente)
        var permiso = await _permisos.ObtenerAsync(usuario, ct);
        if (!(usuario.EsAdmin || usuario.EsRecepcion || usuario.EsMedico) || !permiso.PuedeVer(request.MedicoId, request.PacienteId))
            throw new DomainException(Errores.SIN_PERMISO, "No puedes agendar en la agenda de ese médico.", 403);

        if (request.FechaHoraInicio <= DateTime.Now)
            throw new DomainException(Errores.DATOS_INVALIDOS, "La cita debe agendarse en una fecha y hora futura.");

        // 1. El paciente y el médico deben existir en MS_Directory
        var paciente = await _directory.GetPacienteAsync(request.PacienteId, ct)
            ?? throw new DomainException(Errores.NO_ENCONTRADO, $"No existe el paciente {request.PacienteId}.", 404);
        if (!paciente.Activo)
            throw new DomainException(Errores.DATOS_INVALIDOS, "El paciente está inactivo.");

        var medico = await _directory.GetMedicoAsync(request.MedicoId, ct)
            ?? throw new DomainException(Errores.NO_ENCONTRADO, $"No existe el médico {request.MedicoId}.", 404);
        if (!medico.Activo)
            throw new DomainException(Errores.DATOS_INVALIDOS, "El médico está inactivo.");

        // 2. La especialidad debe ser una del médico (si no la envían, se usa la principal)
        var especialidad = request.EspecialidadId.HasValue
            ? medico.Especialidades.FirstOrDefault(e => e.EspecialidadID == request.EspecialidadId.Value)
              ?? throw new DomainException(Errores.DATOS_INVALIDOS, "El médico no atiende esa especialidad.")
            : medico.Especialidades.OrderByDescending(e => e.Principal).FirstOrDefault();

        // 3. Debe caer dentro de un horario del médico
        var fecha = DateOnly.FromDateTime(request.FechaHoraInicio);
        var hora = TimeOnly.FromDateTime(request.FechaHoraInicio);
        var horarios = await _repo.GetHorariosDelDiaAsync(request.MedicoId, (int)fecha.DayOfWeek, ct);

        var horario = horarios.FirstOrDefault(h =>
            hora >= h.HoraInicio && hora.AddMinutes(h.DuracionCitaMinutos) <= h.HoraFin)
            ?? throw new DomainException(Errores.FUERA_DE_HORARIO,
                $"{medico.NombreCompleto} no atiende en ese día y hora.", 422);

        var inicio = request.FechaHoraInicio;
        var fin = inicio.AddMinutes(horario.DuracionCitaMinutos);

        // 4. No debe cruzarse con otra cita del médico ni del paciente
        if (await _repo.MedicoTieneCruceAsync(medico.MedicoID, inicio, fin, ct))
            throw new DomainException(Errores.ESPACIO_OCUPADO, "El médico ya tiene una cita en ese horario.", 409);

        if (await _repo.PacienteTieneCruceAsync(paciente.PacienteID, inicio, fin, ct))
            throw new DomainException(Errores.ESPACIO_OCUPADO, "El paciente ya tiene otra cita en ese horario.", 409);

        var cita = new Cita
        {
            PacienteId = paciente.PacienteID,
            PacienteNombre = $"{paciente.Nombres} {paciente.Apellidos}",
            PacienteDocumento = paciente.NumeroDocumento,
            MedicoId = medico.MedicoID,
            MedicoNombre = medico.NombreCompleto,
            EspecialidadId = especialidad?.EspecialidadID,
            EspecialidadNombre = especialidad?.Nombre,
            ConsultorioId = horario.ConsultorioId,
            FechaHoraInicio = inicio,
            FechaHoraFin = fin,
            EstadoCitaId = EstadosCita.PROGRAMADA,
            MotivoConsulta = request.MotivoConsulta,
            UsuarioRegistro = usuario.Login,
            FechaCreacion = DateTime.Now
        };

        await _repo.AddCitaAsync(cita, ct);

        // Se relee para traer el estado y el consultorio
        return await GetCitaAsync(cita.CitaId, usuario, ct);
    }

    public async Task<CitaDto> CambiarEstadoAsync(long id, CambiarEstadoRequest request, UsuarioAgenda usuario, CancellationToken ct)
    {
        var cita = await _repo.GetCitaAsync(id, ct)
            ?? throw new DomainException(Errores.NO_ENCONTRADO, $"No existe la cita {id}.", 404);

        var permiso = await _permisos.ObtenerAsync(usuario, ct);
        if (!permiso.PuedeVer(cita.MedicoId, cita.PacienteId))
            throw new DomainException(Errores.SIN_PERMISO, "No tienes acceso a esta cita.", 403);

        var estados = await _repo.GetEstadosAsync(ct);
        var nuevo = estados.FirstOrDefault(e => e.Codigo.Equals(request.Estado?.Trim(), StringComparison.OrdinalIgnoreCase))
            ?? throw new DomainException(Errores.DATOS_INVALIDOS, $"El estado '{request.Estado}' no existe.");

        if (!EstadosCita.Transiciones[cita.EstadoCitaId].Contains(nuevo.EstadoCitaId))
            throw new DomainException(Errores.TRANSICION_INVALIDA,
                $"Una cita en estado '{cita.Estado?.Nombre}' no puede pasar a '{nuevo.Nombre}'.", 409);

        // Atender (En atención / Atendida) es solo del médico dueño de la cita
        if (usuario.EsMedico && !permiso.EsPropia(cita.MedicoId) && !EstadosParaAutorizado.Contains(nuevo.EstadoCitaId))
            throw new DomainException(Errores.SIN_PERMISO, "Solo el médico de la cita puede atenderla.", 403);

        if (usuario.EsEnfermeria && !EstadosParaEnfermeria.Contains(nuevo.EstadoCitaId))
            throw new DomainException(Errores.SIN_PERMISO, $"Enfermería no puede pasar una cita a '{nuevo.Nombre}'.", 403);

        cita.EstadoCitaId = nuevo.EstadoCitaId;
        cita.Estado = nuevo;
        if (!string.IsNullOrWhiteSpace(request.Notas))
            cita.Notas = request.Notas;
        cita.FechaActualizacion = DateTime.Now;

        await _repo.GuardarCambiosAsync(ct);
        return ToDto(cita);
    }

    public async Task<List<EstadoCitaDto>> GetEstadosAsync(CancellationToken ct)
    {
        var estados = await _repo.GetEstadosAsync(ct);
        return estados.Select(e => new EstadoCitaDto(e.Codigo, e.Nombre, e.Color)).ToList();
    }

    // Conteo de citas del día por estado (para el dashboard)
    public async Task<ResumenCitasDto> GetResumenAsync(DateOnly fecha, int? medicoId, UsuarioAgenda usuario, CancellationToken ct)
    {
        var desde = fecha.ToDateTime(TimeOnly.MinValue);
        var permiso = await _permisos.ObtenerAsync(usuario, ct);
        var citas = (await _repo.GetCitasAsync(new CitaFiltro(desde, desde.AddDays(1), medicoId, null, null), ct))
            .Where(c => permiso.PuedeVer(c.MedicoId, c.PacienteId)).ToList();
        var estados = await _repo.GetEstadosAsync(ct);

        var porEstado = estados
            .Select(e => new ResumenEstadoDto(e.Codigo, e.Nombre, e.Color, citas.Count(c => c.EstadoCitaId == e.EstadoCitaId)))
            .ToList();

        return new ResumenCitasDto(fecha, citas.Count, porEstado);
    }

    private static CitaDto ToDto(Cita c) => new(
        c.CitaId,
        c.PacienteId,
        c.PacienteNombre,
        c.PacienteDocumento,
        c.MedicoId,
        c.MedicoNombre,
        c.EspecialidadId,
        c.EspecialidadNombre,
        c.ConsultorioId,
        c.Consultorio?.Nombre ?? string.Empty,
        c.FechaHoraInicio,
        c.FechaHoraFin,
        c.Estado?.Codigo ?? string.Empty,
        c.Estado?.Nombre ?? string.Empty,
        c.Estado?.Color ?? string.Empty,
        c.MotivoConsulta,
        c.Notas,
        c.FechaCreacion);
}