using System.Text.Json;
using BuildingBlocks.Rest.Exceptions;
using BuildingBlocks.Rest.Interfaces.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using MsClinical.Core.Configs;
using MsClinical.Core.Dtos;
using MsClinical.Core.Exceptions;
using MsClinical.Core.Interfaces;

namespace MsClinical.Infrastructure.Services;

// Consulta y actualiza citas en MS_Agenda usando la librería BuildingBlocks.Rest
internal class AgendaService : IAgendaService
{
    private readonly IRest _rest;
    private readonly DownstreamOptions _options;
    private readonly IHttpContextAccessor _httpContext;

    public AgendaService(IRest rest, IOptions<DownstreamOptions> options, IHttpContextAccessor httpContext)
    {
        _rest = rest;
        _options = options.Value;
        _httpContext = httpContext;
    }

    public async Task<CitaRemotaDto?> GetCitaAsync(long citaId, CancellationToken ct)
    {
        try
        {
            return await _rest.Get
                .WithBearer(ObtenerToken())
                .WithUri(_options.AgendaBaseUrl, $"/api/Citas/{citaId}")
                .DeserializeWithAsync<CitaRemotaDto>(ct);
        }
        catch (ApiException ex) when (ex.Reason == ApiFailureReason.HttpError && ex.StatusCode == 404)
        {
            return null;
        }
        catch (ApiException ex)
        {
            throw Traducir(ex);
        }
    }

    public async Task<CitaRemotaDto> CambiarEstadoCitaAsync(long citaId, string estado, string? notas, CancellationToken ct)
    {
        try
        {
            return await _rest.Patch
                .WithBearer(ObtenerToken())
                .WithUri(_options.AgendaBaseUrl, $"/api/Citas/{citaId}/estado")
                .WithBody(new { estado, notas })
                .DeserializeWithAsync<CitaRemotaDto>(ct);
        }
        catch (ApiException ex)
        {
            throw Traducir(ex);
        }
    }

    public async Task<bool> PuedeVerPacienteAsync(long pacienteId, CancellationToken ct)
    {
        try
        {
            var r = await _rest.Get
                .WithBearer(ObtenerToken())
                .WithUri(_options.AgendaBaseUrl, $"/api/Accesos/puede-ver/{pacienteId}")
                .DeserializeWithAsync<PuedeVerRemotoDto>(ct);
            return r?.PuedeVer == true;
        }
        catch (ApiException ex)
        {
            throw Traducir(ex);
        }
    }

    // Convierte el error de Agenda en un DomainException con el mismo mensaje que dio Agenda
    private static DomainException Traducir(ApiException ex)
    {
        if (ex.Reason is ApiFailureReason.Network or ApiFailureReason.Timeout)
            return new DomainException(Errores.SERVICIO_NO_DISPONIBLE,
                "El servicio de Agenda no está disponible. Intenta de nuevo más tarde.", 503, ex);

        if (ex.Reason == ApiFailureReason.HttpError && ex.StatusCode is >= 400 and < 500)
        {
            var mensaje = LeerMensaje(ex.ResponseBody) ?? "Agenda rechazó la operación.";
            return new DomainException(Errores.CITA_NO_VALIDA, mensaje, ex.StatusCode.Value, ex);
        }

        return new DomainException(Errores.ERROR_SERVICIO_EXTERNO,
            $"El servicio de Agenda respondió con un error ({ex.StatusCode}).", 502, ex);
    }

    private static string? LeerMensaje(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.TryGetProperty("mensaje", out var m) ? m.GetString() : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private string ObtenerToken()
    {
        var header = _httpContext.HttpContext?.Request.Headers.Authorization.ToString() ?? string.Empty;
        return header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? header["Bearer ".Length..].Trim()
            : header;
    }
}