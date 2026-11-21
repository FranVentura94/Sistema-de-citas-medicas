using System.Globalization;
using MsAgenda.Core.Dtos;
using MsAgenda.Core.Entities;
using MsAgenda.Core.Exceptions;
using MsAgenda.Core.Interfaces;

namespace MsAgenda.Core.Services;

public interface IHorarioService
{
    Task<List<ConsultorioDto>> GetConsultoriosAsync(CancellationToken ct);
    Task<List<HorarioDto>> GetHorariosAsync(int? medicoId, CancellationToken ct);
    Task<HorarioDto> CrearHorarioAsync(CrearHorarioRequest request, CancellationToken ct);
    Task<List<EspacioDisponibleDto>> GetDisponibilidadAsync(int medicoId, DateOnly fecha, CancellationToken ct);
    Task EliminarHorarioAsync(int horarioId, CancellationToken ct);
}

public class HorarioService : IHorarioService
{
    private static readonly CultureInfo Es = new("es-ES");

    private readonly IAgendaRepository _repo;
    private readonly IDirectoryService _directory;

    public HorarioService(IAgendaRepository repo, IDirectoryService directory)
    {
        _repo = repo;
        _directory = directory;
    }

    public async Task<List<ConsultorioDto>> GetConsultoriosAsync(CancellationToken ct)
    {
        var consultorios = await _repo.GetConsultoriosAsync(ct);
        return consultorios.Select(c => new ConsultorioDto(c.ConsultorioId, c.Codigo, c.Nombre, c.Ubicacion)).ToList();
    }

    public async Task<List<HorarioDto>> GetHorariosAsync(int? medicoId, CancellationToken ct)
    {
        var horarios = await _repo.GetHorariosAsync(medicoId, ct);
        return horarios.Select(ToDto).ToList();
    }

    public async Task<HorarioDto> CrearHorarioAsync(CrearHorarioRequest request, CancellationToken ct)
    {
        if (request.DiaSemana is < 0 or > 6)
            throw new DomainException(Errores.DATOS_INVALIDOS, "El día debe estar entre 0 (domingo) y 6 (sábado).");

        if (!TimeOnly.TryParse(request.HoraInicio, out var inicio) || !TimeOnly.TryParse(request.HoraFin, out var fin))
            throw new DomainException(Errores.DATOS_INVALIDOS, "Las horas deben tener el formato HH:mm, por ejemplo 08:00.");

        if (fin <= inicio)
            throw new DomainException(Errores.DATOS_INVALIDOS, "La hora de fin debe ser mayor que la de inicio.");

        if (request.DuracionCitaMinutos is < 10 or > 240)
            throw new DomainException(Errores.DATOS_INVALIDOS, "La duración de cada cita debe estar entre 10 y 240 minutos.");

        var consultorio = await _repo.GetConsultorioAsync(request.ConsultorioId, ct)
            ?? throw new DomainException(Errores.NO_ENCONTRADO, "El consultorio no existe.", 404);

        var medico = await _directory.GetMedicoAsync(request.MedicoId, ct)
            ?? throw new DomainException(Errores.NO_ENCONTRADO, $"No existe el médico {request.MedicoId}.", 404);

        // No puede tener dos horarios que se crucen el mismo día
        var delDia = await _repo.GetHorariosDelDiaAsync(request.MedicoId, request.DiaSemana, ct);
        if (delDia.Any(h => inicio < h.HoraFin && fin > h.HoraInicio))
            throw new DomainException(Errores.ESPACIO_OCUPADO, "El médico ya tiene un horario que se cruza en ese día.", 409);

        var horario = new HorarioMedico
        {
            MedicoId = medico.MedicoID,
            MedicoNombre = medico.NombreCompleto,
            ConsultorioId = consultorio.ConsultorioId,
            DiaSemana = request.DiaSemana,
            HoraInicio = inicio,
            HoraFin = fin,
            DuracionCitaMinutos = request.DuracionCitaMinutos,
            Activo = true,
            Consultorio = consultorio
        };

        await _repo.AddHorarioAsync(horario, ct);
        return ToDto(horario);
    }

    // Devuelve los espacios del día (cada X minutos) indicando si están libres u ocupados
    public async Task<List<EspacioDisponibleDto>> GetDisponibilidadAsync(int medicoId, DateOnly fecha, CancellationToken ct)
    {
        var horarios = await _repo.GetHorariosDelDiaAsync(medicoId, (int)fecha.DayOfWeek, ct);
        if (horarios.Count == 0)
            return new List<EspacioDisponibleDto>();

        var desde = fecha.ToDateTime(TimeOnly.MinValue);
        var citas = await _repo.GetCitasAsync(new CitaFiltro(desde, desde.AddDays(1), medicoId, null, null), ct);
        var ocupadas = citas.Where(c => !EstadosCita.Liberan.Contains(c.EstadoCitaId)).ToList();

        var ahora = DateTime.Now;
        var espacios = new List<EspacioDisponibleDto>();

        foreach (var h in horarios.OrderBy(h => h.HoraInicio))
        {
            var inicio = fecha.ToDateTime(h.HoraInicio);
            var finHorario = fecha.ToDateTime(h.HoraFin);

            while (inicio.AddMinutes(h.DuracionCitaMinutos) <= finHorario)
            {
                var fin = inicio.AddMinutes(h.DuracionCitaMinutos);
                var ocupado = ocupadas.Any(c => inicio < c.FechaHoraFin && fin > c.FechaHoraInicio);

                espacios.Add(new EspacioDisponibleDto(
                    inicio, fin,
                    Disponible: !ocupado && inicio > ahora,
                    h.ConsultorioId,
                    h.Consultorio?.Nombre ?? string.Empty));

                inicio = fin;
            }
        }

        return espacios;
    }
    // Quita un bloque de horario (las citas ya agendadas se conservan)
    public async Task EliminarHorarioAsync(int horarioId, CancellationToken ct)
    {
        var horario = await _repo.GetHorarioAsync(horarioId, ct)
            ?? throw new DomainException(Errores.NO_ENCONTRADO, "El horario no existe.", 404);

        horario.Activo = false;
        await _repo.GuardarCambiosAsync(ct);
    }

    private static HorarioDto ToDto(HorarioMedico h) => new(
        h.HorarioMedicoId,
        h.MedicoId,
        h.MedicoNombre,
        h.ConsultorioId,
        h.Consultorio?.Nombre ?? string.Empty,
        h.DiaSemana,
        Es.TextInfo.ToTitleCase(Es.DateTimeFormat.GetDayName((DayOfWeek)h.DiaSemana)),
        h.HoraInicio.ToString("HH:mm"),
        h.HoraFin.ToString("HH:mm"),
        h.DuracionCitaMinutos);
}