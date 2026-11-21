using BuildingBlocks.Rest.Exceptions;
using BuildingBlocks.Rest.Interfaces.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using MsClinical.Core.Configs;
using MsClinical.Core.Dtos;
using MsClinical.Core.Exceptions;
using MsClinical.Core.Interfaces;

namespace MsClinical.Infrastructure.Services;

// Consulta datos del paciente en MS_Directory (para abrir su expediente aunque aún no tenga atenciones)
internal class PacientesService : IPacientesService
{
    private readonly IRest _rest;
    private readonly DownstreamOptions _options;
    private readonly IHttpContextAccessor _httpContext;

    public PacientesService(IRest rest, IOptions<DownstreamOptions> options, IHttpContextAccessor httpContext)
    {
        _rest = rest;
        _options = options.Value;
        _httpContext = httpContext;
    }

    public async Task<PacienteRemotoDto?> GetPacienteAsync(long pacienteId, CancellationToken ct)
    {
        try
        {
            return await _rest.Get
                .WithBearer(ObtenerToken())
                .WithUri(_options.DirectoryBaseUrl, $"/api/Pacientes/{pacienteId}")
                .DeserializeWithAsync<PacienteRemotoDto>(ct);
        }
        catch (ApiException ex) when (ex.Reason == ApiFailureReason.HttpError && ex.StatusCode == 404)
        {
            return null;
        }
        catch (ApiException ex) when (ex.Reason is ApiFailureReason.Network or ApiFailureReason.Timeout)
        {
            throw new DomainException(Errores.SERVICIO_NO_DISPONIBLE,
                "El servicio de Pacientes no está disponible. Intenta de nuevo más tarde.", 503, ex);
        }
        catch (ApiException ex)
        {
            throw new DomainException(Errores.ERROR_SERVICIO_EXTERNO,
                $"El servicio de Pacientes respondió con un error ({ex.StatusCode}).", 502, ex);
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
