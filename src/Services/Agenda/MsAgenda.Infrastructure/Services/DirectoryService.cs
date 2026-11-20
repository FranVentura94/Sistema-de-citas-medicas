using BuildingBlocks.Rest.Exceptions;
using BuildingBlocks.Rest.Interfaces.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using MsAgenda.Core.Configs;
using MsAgenda.Core.Dtos;
using MsAgenda.Core.Exceptions;
using MsAgenda.Core.Interfaces;

namespace MsAgenda.Infrastructure.Services;

// Consulta pacientes y médicos en MS_Directory usando la librería BuildingBlocks.Rest
internal class DirectoryService : IDirectoryService
{
    private readonly IRest _rest;
    private readonly DownstreamOptions _options;
    private readonly IHttpContextAccessor _httpContext;

    public DirectoryService(IRest rest, IOptions<DownstreamOptions> options, IHttpContextAccessor httpContext)
    {
        _rest = rest;
        _options = options.Value;
        _httpContext = httpContext;
    }

    public Task<PacienteRemotoDto?> GetPacienteAsync(long pacienteId, CancellationToken ct) =>
        GetAsync<PacienteRemotoDto>($"/api/Pacientes/{pacienteId}", "Pacientes", ct);

    public Task<MedicoRemotoDto?> GetMedicoAsync(int medicoId, CancellationToken ct) =>
        GetAsync<MedicoRemotoDto>($"/api/Medicos/{medicoId}", "Médicos", ct);

    private async Task<T?> GetAsync<T>(string endpoint, string servicio, CancellationToken ct) where T : class
    {
        try
        {
            // Reenvía el mismo token JWT con el que el usuario llamó a Agenda
            return await _rest.Get
                .WithBearer(ObtenerToken())
                .WithUri(_options.DirectoryBaseUrl, endpoint)
                .DeserializeWithAsync<T>(ct);
        }
        catch (ApiException ex) when (ex.Reason == ApiFailureReason.HttpError && ex.StatusCode == 404)
        {
            return null;
        }
        catch (ApiException ex) when (ex.Reason is ApiFailureReason.Network or ApiFailureReason.Timeout)
        {
            throw new DomainException(Errores.SERVICIO_NO_DISPONIBLE,
                $"El servicio de {servicio} no está disponible. Intenta de nuevo más tarde.", 503, ex);
        }
        catch (ApiException ex)
        {
            throw new DomainException(Errores.ERROR_SERVICIO_EXTERNO,
                $"El servicio de {servicio} respondió con un error ({ex.StatusCode}).", 502, ex);
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