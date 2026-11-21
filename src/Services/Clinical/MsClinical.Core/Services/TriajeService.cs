using MsClinical.Core.Dtos;
using MsClinical.Core.Entities;
using MsClinical.Core.Exceptions;
using MsClinical.Core.Interfaces;

namespace MsClinical.Core.Services;

// Triaje: la enfermera toma los signos vitales al llegar el paciente y lo deja "En espera" para el médico
public interface ITriajeService
{
    Task<TriajeDto> RegistrarAsync(TriajeRequest request, UsuarioActual usuario, CancellationToken ct);
    Task<TriajeDto?> GetPorCitaAsync(long citaId, CancellationToken ct);
}

public class TriajeService : ITriajeService
{
    private static readonly string[] EstadosPermitidos = { "PROGRAMADA", "CONFIRMADA", "EN_ESPERA" };

    private readonly IClinicalRepository _repo;
    private readonly IAgendaService _agenda;

    public TriajeService(IClinicalRepository repo, IAgendaService agenda)
    {
        _repo = repo;
        _agenda = agenda;
    }

    public async Task<TriajeDto> RegistrarAsync(TriajeRequest r, UsuarioActual usuario, CancellationToken ct)
    {
        // Agenda valida que quien llama tenga acceso a la cita (enfermera asignada, médico, admin)
        var cita = await _agenda.GetCitaAsync(r.CitaId, ct)
            ?? throw new DomainException(Errores.NO_ENCONTRADO, $"No existe la cita {r.CitaId}.", 404);

        var existente = await _repo.GetTriajePorCitaAsync(r.CitaId, ct);
        if (existente is null && !EstadosPermitidos.Contains(cita.EstadoCodigo))
            throw new DomainException(Errores.CITA_NO_VALIDA, $"No se puede hacer triaje a una cita en estado '{cita.EstadoNombre}'.", 409);

        Validar(r);

        var t = existente ?? new Triaje
        {
            CitaId = cita.CitaId,
            PacienteId = cita.PacienteId,
            PacienteNombre = cita.PacienteNombre,
            MedicoId = cita.MedicoId,
            MedicoNombre = cita.MedicoNombre
        };

        t.PresionSistolica = r.PresionSistolica;
        t.PresionDiastolica = r.PresionDiastolica;
        t.FrecuenciaCardiaca = r.FrecuenciaCardiaca;
        t.FrecuenciaRespiratoria = r.FrecuenciaRespiratoria;
        t.Temperatura = r.Temperatura;
        t.SaturacionOxigeno = r.SaturacionOxigeno;
        t.Peso = r.Peso;
        t.Talla = r.Talla;
        t.Notas = string.IsNullOrWhiteSpace(r.Notas) ? null : r.Notas.Trim();
        t.RegistradoPorUsuarioId = usuario.UsuarioId;
        t.RegistradoPorNombre = NombreVisible(usuario);
        t.RegistradoPorRol = usuario.Rol ?? string.Empty;
        t.FechaRegistro = DateTime.Now;

        if (existente is null) await _repo.AddTriajeAsync(t, ct);
        else await _repo.GuardarCambiosAsync(ct);

        // El paciente queda listo para el médico
        if (cita.EstadoCodigo is "PROGRAMADA" or "CONFIRMADA")
            await _agenda.CambiarEstadoCitaAsync(cita.CitaId, "EN_ESPERA", "Triaje realizado: listo para consulta", ct);

        return ToDto(t);
    }

    public async Task<TriajeDto?> GetPorCitaAsync(long citaId, CancellationToken ct)
    {
        var t = await _repo.GetTriajePorCitaAsync(citaId, ct);
        return t is null ? null : ToDto(t);
    }

    // "Enf. Ana Pérez" / "Dr(a). Carlos Rivas" / nombre tal cual
    public static string NombreVisible(UsuarioActual u)
    {
        var nombre = u.Nombre ?? u.NombreUsuario ?? "—";
        return u.EsEnfermeria ? $"Enf. {nombre}" : nombre;
    }

    public static TriajeDto ToDto(Triaje t) => new(
        t.TriajeId, t.CitaId, t.PacienteId, t.PacienteNombre, t.MedicoId, t.MedicoNombre,
        new SignosVitales
        {
            PresionSistolica = t.PresionSistolica, PresionDiastolica = t.PresionDiastolica,
            FrecuenciaCardiaca = t.FrecuenciaCardiaca, FrecuenciaRespiratoria = t.FrecuenciaRespiratoria,
            Temperatura = t.Temperatura, SaturacionOxigeno = t.SaturacionOxigeno, Peso = t.Peso, Talla = t.Talla,
            FechaRegistro = t.FechaRegistro
        }.ToDto(),
        t.Notas, t.RegistradoPorNombre, t.RegistradoPorRol, t.FechaRegistro);

    private static void Validar(TriajeRequest r)
    {
        Rango(r.PresionSistolica, 50, 260, "La presión sistólica");
        Rango(r.PresionDiastolica, 30, 160, "La presión diastólica");
        Rango(r.FrecuenciaCardiaca, 20, 250, "La frecuencia cardíaca");
        Rango(r.FrecuenciaRespiratoria, 5, 80, "La frecuencia respiratoria");
        Rango(r.Temperatura, 30, 45, "La temperatura");
        Rango(r.SaturacionOxigeno, 50, 100, "La saturación de oxígeno");
        Rango(r.Peso, 0.5m, 400, "El peso");
        Rango(r.Talla, 30, 250, "La talla");

        if (r.PresionSistolica is null && r.FrecuenciaCardiaca is null && r.Temperatura is null && r.Peso is null && r.SaturacionOxigeno is null)
            throw new DomainException(Errores.DATOS_INVALIDOS, "Registra al menos un signo vital.");
    }

    private static void Rango(decimal? valor, decimal min, decimal max, string campo)
    {
        if (valor.HasValue && (valor < min || valor > max))
            throw new DomainException(Errores.DATOS_INVALIDOS, $"{campo} debe estar entre {min} y {max}.");
    }
}
