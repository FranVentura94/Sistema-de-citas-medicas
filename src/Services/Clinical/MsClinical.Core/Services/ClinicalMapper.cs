using MsClinical.Core.Dtos;
using MsClinical.Core.Entities;

namespace MsClinical.Core.Services;

public static class ClinicalMapper
{
    public static AtencionDto ToDto(this Atencion a) => new(
        a.AtencionId,
        a.CitaId,
        a.PacienteId,
        a.PacienteNombre,
        a.Expediente?.NumeroExpediente ?? string.Empty,
        a.MedicoId,
        a.MedicoNombre,
        a.EspecialidadNombre,
        a.FechaInicio,
        a.FechaFin,
        a.Estado,
        a.MotivoConsulta,
        a.Observaciones,
        a.SignosVitales?.ToDto(),
        a.Notas.OrderBy(n => n.FechaRegistro)
            .Select(n => new NotaDto(n.NotaMedicaId, n.Tipo, n.Contenido, n.Autor, n.FechaRegistro))
            .ToList(),
        a.Diagnosticos.OrderBy(d => d.Tipo == "PRINCIPAL" ? 0 : 1)
            .Select(d => new AtencionDiagnosticoDto(
                d.DiagnosticoId,
                d.Diagnostico?.Codigo ?? string.Empty,
                d.Diagnostico?.Descripcion ?? string.Empty,
                d.Tipo,
                d.Observacion))
            .ToList(),
        a.Prescripcion?.ToDto());

    public static SignosVitalesDto ToDto(this SignosVitales s)
    {
        // IMC = peso (kg) / talla (m)²
        decimal? imc = s.Peso.HasValue && s.Talla is > 0
            ? Math.Round(s.Peso.Value / ((s.Talla.Value / 100) * (s.Talla.Value / 100)), 1)
            : null;

        return new SignosVitalesDto(
            s.PresionSistolica,
            s.PresionDiastolica,
            s.FrecuenciaCardiaca,
            s.FrecuenciaRespiratoria,
            s.Temperatura,
            s.SaturacionOxigeno,
            s.Peso,
            s.Talla,
            imc,
            s.FechaRegistro);
    }

    public static PrescripcionDto ToDto(this Prescripcion p) => new(
        p.PrescripcionId,
        p.Fecha,
        p.IndicacionesGenerales,
        p.Detalles.Select(d => new PrescripcionDetalleDto(
                d.MedicamentoId,
                d.Medicamento?.Nombre ?? string.Empty,
                d.Medicamento?.Presentacion ?? string.Empty,
                d.Medicamento?.Concentracion ?? string.Empty,
                d.Dosis,
                d.Frecuencia,
                d.Duracion,
                d.Via,
                d.Indicaciones))
            .ToList());

    public static AtencionResumenDto ToResumen(this Atencion a) => new(
        a.AtencionId,
        a.CitaId,
        a.FechaInicio,
        a.MedicoNombre,
        a.EspecialidadNombre,
        a.Estado,
        a.MotivoConsulta,
        a.Diagnosticos
            .Select(d => $"{d.Diagnostico?.Codigo} - {d.Diagnostico?.Descripcion}")
            .ToList());
}