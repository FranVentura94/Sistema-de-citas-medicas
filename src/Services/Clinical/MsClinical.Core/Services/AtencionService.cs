using MsClinical.Core.Dtos;
using MsClinical.Core.Entities;
using MsClinical.Core.Exceptions;
using MsClinical.Core.Interfaces;

namespace MsClinical.Core.Services;

public interface IAtencionService
{
    Task<AtencionDto> IniciarAsync(long citaId, UsuarioActual usuario, CancellationToken ct);
    Task<AtencionDto> GetAsync(long atencionId, CancellationToken ct);
    Task<AtencionDto> GetPorCitaAsync(long citaId, CancellationToken ct);
    Task<List<AtencionResumenDto>> GetMisAtencionesAsync(UsuarioActual usuario, DateOnly? fecha, CancellationToken ct);
    Task<AtencionDto> RegistrarSignosAsync(long atencionId, SignosVitalesRequest request, UsuarioActual usuario, CancellationToken ct);
    Task<AtencionDto> AgregarNotaAsync(long atencionId, NotaRequest request, UsuarioActual usuario, CancellationToken ct);
    Task<AtencionDto> AgregarDiagnosticoAsync(long atencionId, DiagnosticoRequest request, UsuarioActual usuario, CancellationToken ct);
    Task<AtencionDto> QuitarDiagnosticoAsync(long atencionId, int diagnosticoId, UsuarioActual usuario, CancellationToken ct);
    Task<AtencionDto> GuardarPrescripcionAsync(long atencionId, PrescripcionRequest request, UsuarioActual usuario, CancellationToken ct);
    Task<AtencionDto> FinalizarAsync(long atencionId, FinalizarAtencionRequest request, UsuarioActual usuario, CancellationToken ct);
}

public class AtencionService : IAtencionService
{
    // Estados de la cita (MS_Agenda) desde los que se puede iniciar la atención
    private static readonly string[] EstadosCitaValidos = { "PROGRAMADA", "CONFIRMADA", "EN_ESPERA", "EN_ATENCION" };
    private static readonly string[] TiposNota = { "EVOLUCION", "VALORACION", "HALLAZGO", "OBSERVACION" };
    private static readonly string[] TiposDiagnostico = { "PRINCIPAL", "SECUNDARIO" };

    private readonly IClinicalRepository _repo;
    private readonly IAgendaService _agenda;

    public AtencionService(IClinicalRepository repo, IAgendaService agenda)
    {
        _repo = repo;
        _agenda = agenda;
    }

    // ---------- Iniciar: la cita pasa a "En atención" en MS_Agenda ----------
    public async Task<AtencionDto> IniciarAsync(long citaId, UsuarioActual usuario, CancellationToken ct)
    {
        // Si ya se inició antes, se devuelve la misma atención
        var existente = await _repo.GetAtencionPorCitaAsync(citaId, ct);
        if (existente is not null)
        {
            ValidarMedico(existente.MedicoId, usuario);
            return existente.ToDto();
        }

        var cita = await _agenda.GetCitaAsync(citaId, ct)
            ?? throw new DomainException(Errores.NO_ENCONTRADO, $"No existe la cita {citaId}.", 404);

        ValidarMedico(cita.MedicoId, usuario);

        if (!EstadosCitaValidos.Contains(cita.EstadoCodigo))
            throw new DomainException(Errores.CITA_NO_VALIDA,
                $"No se puede atender una cita en estado '{cita.EstadoNombre}'.", 409);

        if (cita.EstadoCodigo != "EN_ATENCION")
            await _agenda.CambiarEstadoCitaAsync(citaId, "EN_ATENCION", null, ct);

        // El expediente se crea automáticamente en la primera atención del paciente
        var expediente = await _repo.GetExpedienteAsync(cita.PacienteId, false, ct);
        if (expediente is null)
        {
            expediente = new ExpedienteClinico
            {
                PacienteId = cita.PacienteId,
                PacienteNombre = cita.PacienteNombre,
                NumeroExpediente = $"EXP-{cita.PacienteId:D6}",
                FechaApertura = DateTime.Now
            };
            await _repo.AddExpedienteAsync(expediente, ct);
        }

        var atencion = new Atencion
        {
            ExpedienteClinicoId = expediente.ExpedienteClinicoId,
            CitaId = cita.CitaId,
            PacienteId = cita.PacienteId,
            PacienteNombre = cita.PacienteNombre,
            MedicoId = cita.MedicoId,
            MedicoNombre = cita.MedicoNombre,
            EspecialidadNombre = cita.EspecialidadNombre,
            MotivoConsulta = cita.MotivoConsulta,
            FechaInicio = DateTime.Now,
            Estado = EstadosAtencion.EN_PROCESO
        };

        await _repo.AddAtencionAsync(atencion, ct);
        return await GetAsync(atencion.AtencionId, ct);
    }

    public async Task<AtencionDto> GetAsync(long atencionId, CancellationToken ct)
    {
        var atencion = await _repo.GetAtencionAsync(atencionId, ct)
            ?? throw new DomainException(Errores.NO_ENCONTRADO, $"No existe la atención {atencionId}.", 404);
        return atencion.ToDto();
    }

    public async Task<AtencionDto> GetPorCitaAsync(long citaId, CancellationToken ct)
    {
        var atencion = await _repo.GetAtencionPorCitaAsync(citaId, ct)
            ?? throw new DomainException(Errores.NO_ENCONTRADO, $"La cita {citaId} todavía no tiene atención.", 404);
        return atencion.ToDto();
    }

    public async Task<List<AtencionResumenDto>> GetMisAtencionesAsync(UsuarioActual usuario, DateOnly? fecha, CancellationToken ct)
    {
        if (usuario.MedicoId is null)
            throw new DomainException(Errores.SIN_PERMISO, "Tu usuario no está enlazado a un médico.", 403);

        DateTime? desde = fecha?.ToDateTime(TimeOnly.MinValue);
        var atenciones = await _repo.GetAtencionesPorMedicoAsync(usuario.MedicoId.Value, desde, desde?.AddDays(1), ct);
        return atenciones.Select(a => a.ToResumen()).ToList();
    }

    // ---------- Signos vitales (se crean o se actualizan) ----------
    public async Task<AtencionDto> RegistrarSignosAsync(long atencionId, SignosVitalesRequest r, UsuarioActual usuario, CancellationToken ct)
    {
        var atencion = await GetEditableAsync(atencionId, usuario, ct);

        Rango(r.PresionSistolica, 50, 260, "La presión sistólica");
        Rango(r.PresionDiastolica, 30, 160, "La presión diastólica");
        Rango(r.FrecuenciaCardiaca, 20, 250, "La frecuencia cardíaca");
        Rango(r.FrecuenciaRespiratoria, 5, 80, "La frecuencia respiratoria");
        Rango(r.Temperatura, 30, 45, "La temperatura");
        Rango(r.SaturacionOxigeno, 50, 100, "La saturación de oxígeno");
        Rango(r.Peso, 0.5m, 400, "El peso");
        Rango(r.Talla, 30, 250, "La talla");

        atencion.SignosVitales ??= new SignosVitales { AtencionId = atencion.AtencionId };
        var s = atencion.SignosVitales;
        s.PresionSistolica = r.PresionSistolica;
        s.PresionDiastolica = r.PresionDiastolica;
        s.FrecuenciaCardiaca = r.FrecuenciaCardiaca;
        s.FrecuenciaRespiratoria = r.FrecuenciaRespiratoria;
        s.Temperatura = r.Temperatura;
        s.SaturacionOxigeno = r.SaturacionOxigeno;
        s.Peso = r.Peso;
        s.Talla = r.Talla;
        s.FechaRegistro = DateTime.Now;

        await _repo.GuardarCambiosAsync(ct);
        return atencion.ToDto();
    }

    // ---------- Notas médicas ----------
    public async Task<AtencionDto> AgregarNotaAsync(long atencionId, NotaRequest request, UsuarioActual usuario, CancellationToken ct)
    {
        var atencion = await GetEditableAsync(atencionId, usuario, ct);

        var tipo = request.Tipo?.Trim().ToUpperInvariant() ?? string.Empty;
        if (!TiposNota.Contains(tipo))
            throw new DomainException(Errores.DATOS_INVALIDOS, $"El tipo de nota debe ser: {string.Join(", ", TiposNota)}.");

        if (string.IsNullOrWhiteSpace(request.Contenido))
            throw new DomainException(Errores.DATOS_INVALIDOS, "La nota no puede estar vacía.");

        atencion.Notas.Add(new NotaMedica
        {
            Tipo = tipo,
            Contenido = request.Contenido.Trim(),
            Autor = usuario.NombreUsuario,
            FechaRegistro = DateTime.Now
        });

        await _repo.GuardarCambiosAsync(ct);
        return atencion.ToDto();
    }

    // ---------- Diagnósticos (CIE-10) ----------
    public async Task<AtencionDto> AgregarDiagnosticoAsync(long atencionId, DiagnosticoRequest request, UsuarioActual usuario, CancellationToken ct)
    {
        var atencion = await GetEditableAsync(atencionId, usuario, ct);

        var tipo = request.Tipo?.Trim().ToUpperInvariant() ?? string.Empty;
        if (!TiposDiagnostico.Contains(tipo))
            throw new DomainException(Errores.DATOS_INVALIDOS, "El tipo de diagnóstico debe ser PRINCIPAL o SECUNDARIO.");

        var diagnostico = await _repo.GetDiagnosticoAsync(request.DiagnosticoId, ct)
            ?? throw new DomainException(Errores.NO_ENCONTRADO, "El diagnóstico no existe en el catálogo.", 404);

        // Solo puede haber un diagnóstico principal
        if (tipo == "PRINCIPAL")
            foreach (var d in atencion.Diagnosticos.Where(d => d.Tipo == "PRINCIPAL"))
                d.Tipo = "SECUNDARIO";

        var existente = atencion.Diagnosticos.FirstOrDefault(d => d.DiagnosticoId == diagnostico.DiagnosticoId);
        if (existente is null)
        {
            atencion.Diagnosticos.Add(new AtencionDiagnostico
            {
                DiagnosticoId = diagnostico.DiagnosticoId,
                Diagnostico = diagnostico,
                Tipo = tipo,
                Observacion = request.Observacion
            });
        }
        else
        {
            existente.Tipo = tipo;
            existente.Observacion = request.Observacion;
        }

        await _repo.GuardarCambiosAsync(ct);
        return atencion.ToDto();
    }

    public async Task<AtencionDto> QuitarDiagnosticoAsync(long atencionId, int diagnosticoId, UsuarioActual usuario, CancellationToken ct)
    {
        var atencion = await GetEditableAsync(atencionId, usuario, ct);

        var diagnostico = atencion.Diagnosticos.FirstOrDefault(d => d.DiagnosticoId == diagnosticoId)
            ?? throw new DomainException(Errores.NO_ENCONTRADO, "Ese diagnóstico no está en la atención.", 404);

        atencion.Diagnosticos.Remove(diagnostico);
        _repo.RemoveDiagnostico(diagnostico);

        await _repo.GuardarCambiosAsync(ct);
        return atencion.ToDto();
    }

    // ---------- Receta (se reemplaza completa cada vez que se guarda) ----------
    public async Task<AtencionDto> GuardarPrescripcionAsync(long atencionId, PrescripcionRequest request, UsuarioActual usuario, CancellationToken ct)
    {
        var atencion = await GetEditableAsync(atencionId, usuario, ct);

        if (request.Detalles is null || request.Detalles.Count == 0)
            throw new DomainException(Errores.DATOS_INVALIDOS, "La receta debe tener al menos un medicamento.");

        if (request.Detalles.Any(d => string.IsNullOrWhiteSpace(d.Dosis) || string.IsNullOrWhiteSpace(d.Frecuencia) || string.IsNullOrWhiteSpace(d.Duracion)))
            throw new DomainException(Errores.DATOS_INVALIDOS, "Cada medicamento debe tener dosis, frecuencia y duración.");

        var ids = request.Detalles.Select(d => d.MedicamentoId).Distinct().ToList();
        var medicamentos = await _repo.GetMedicamentosAsync(ids, ct);
        if (medicamentos.Count != ids.Count)
            throw new DomainException(Errores.NO_ENCONTRADO, "Uno o más medicamentos no existen en el catálogo.", 404);

        atencion.Prescripcion ??= new Prescripcion { AtencionId = atencion.AtencionId };
        var receta = atencion.Prescripcion;

        _repo.RemoveDetalles(receta.Detalles.ToList());
        receta.Detalles.Clear();

        receta.Fecha = DateTime.Now;
        receta.IndicacionesGenerales = request.IndicacionesGenerales;

        foreach (var d in request.Detalles)
        {
            receta.Detalles.Add(new PrescripcionDetalle
            {
                MedicamentoId = d.MedicamentoId,
                Medicamento = medicamentos.First(m => m.MedicamentoId == d.MedicamentoId),
                Dosis = d.Dosis.Trim(),
                Frecuencia = d.Frecuencia.Trim(),
                Duracion = d.Duracion.Trim(),
                Via = string.IsNullOrWhiteSpace(d.Via) ? "Oral" : d.Via.Trim(),
                Indicaciones = d.Indicaciones
            });
        }

        await _repo.GuardarCambiosAsync(ct);
        return atencion.ToDto();
    }

    // ---------- Finalizar: la cita pasa a "Atendida" en MS_Agenda ----------
    public async Task<AtencionDto> FinalizarAsync(long atencionId, FinalizarAtencionRequest request, UsuarioActual usuario, CancellationToken ct)
    {
        var atencion = await GetEditableAsync(atencionId, usuario, ct);

        if (atencion.Diagnosticos.Count == 0)
            throw new DomainException(Errores.DATOS_INVALIDOS, "Registra al menos un diagnóstico antes de finalizar la atención.");

        // Primero se actualiza la cita; si Agenda falla, la atención sigue abierta
        await _agenda.CambiarEstadoCitaAsync(atencion.CitaId, "ATENDIDA", "Atención finalizada", ct);

        atencion.Estado = EstadosAtencion.FINALIZADA;
        atencion.FechaFin = DateTime.Now;
        if (!string.IsNullOrWhiteSpace(request.Observaciones))
            atencion.Observaciones = request.Observaciones;

        await _repo.GuardarCambiosAsync(ct);
        return atencion.ToDto();
    }

    // ---------- Ayudantes ----------
    private async Task<Atencion> GetEditableAsync(long atencionId, UsuarioActual usuario, CancellationToken ct)
    {
        var atencion = await _repo.GetAtencionAsync(atencionId, ct)
            ?? throw new DomainException(Errores.NO_ENCONTRADO, $"No existe la atención {atencionId}.", 404);

        ValidarMedico(atencion.MedicoId, usuario);

        if (atencion.Estado == EstadosAtencion.FINALIZADA)
            throw new DomainException(Errores.ATENCION_FINALIZADA, "La atención ya fue finalizada y no se puede modificar.", 409);

        return atencion;
    }

    // Un médico solo puede atender sus propias citas (el administrador puede todo)
    private static void ValidarMedico(int medicoId, UsuarioActual usuario)
    {
        if (!usuario.EsAdministrador && usuario.MedicoId != medicoId)
            throw new DomainException(Errores.SIN_PERMISO, "Esta cita pertenece a otro médico.", 403);
    }

    private static void Rango(decimal? valor, decimal min, decimal max, string campo)
    {
        if (valor.HasValue && (valor < min || valor > max))
            throw new DomainException(Errores.DATOS_INVALIDOS, $"{campo} debe estar entre {min} y {max}.");
    }

    private static void Rango(int? valor, int min, int max, string campo) =>
        Rango((decimal?)valor, min, max, campo);
}