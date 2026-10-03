using BuildingBlocks.Rest.Exceptions;
using BuildingBlocks.Rest.Interfaces.IServices;
using Microsoft.Extensions.Options;
using Notificaciones.Core.Configs;
using Notificaciones.Core.Exceptions;
using Notificaciones.Core.Models;
using Notificaciones.Core.Services;

namespace Notificaciones.Infrastructure.Services;

internal class PacientesService : IPacientesService
{
    private readonly IRest _rest;
    private readonly DownstreamOptions _options;

    public PacientesService(IRest rest, IOptions<DownstreamOptions> options)
    {
        _rest = rest;
        _options = options.Value;
    }

    public async Task<PacienteDto> GetPorDocumentoAsync(string numeroDocumento)
    {
        try
        {
            return await _rest.Get
                .WithoutAuth()
                .WithQuery(new Dictionary<string, string> { ["numeroDocumento"] = numeroDocumento })
                .WithUri(_options.ApiBaseUrl, "/api/Pacientes/uno")
                .DeserializeWithAsync<PacienteDto>();
        }
        catch (ApiException ex) when (ex.Reason == ApiFailureReason.HttpError && ex.StatusCode == 404)
        {
            throw new DomainException(Errores.PACIENTE_NO_ENCONTRADO,
                $"No existe un paciente con el documento {numeroDocumento}.", ex);
        }
        catch (ApiException ex) when (ex.Reason is ApiFailureReason.Network or ApiFailureReason.Timeout)
        {
            throw new DomainException(Errores.SERVICIO_NO_DISPONIBLE,
                "El servicio de Pacientes no está disponible. Intentá de nuevo más tarde.", ex);
        }
        catch (ApiException ex)
        {
            throw new DomainException(Errores.ERROR_SERVICIO_EXTERNO,
                $"El servicio de Pacientes respondió con un error ({ex.StatusCode}).", ex);
        }
    }
}