using BuildingBlocks.Rest.Exceptions;
using BuildingBlocks.Rest.Interfaces.IServices;
using Core.Configs;
using Core.Exceptions;
using Core.Services;
using Domain.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Infrastructure.Services;

internal class PasarelaGateway : IPasarelaGateway
{
    private readonly IRest _rest;
    private readonly ITokenProvider _tokenProvider;
    private readonly PasarelaOptions _options;

    public PasarelaGateway(IRest rest, ITokenProvider tokenProvider, IOptions<PasarelaOptions> options)
    {
        _rest = rest;
        _tokenProvider = tokenProvider;
        _options = options.Value;
    }

    public async Task<CobroResponse> CobrarAsync(CobroRequest request)
    {
        int totalIntentos = _options.MaxReintentos + 1;

        for (int intento = 1; ; intento++)
        {
            try
            {
                return await EnviarCobroAsync(request);
            }
            catch (ApiException ex) when (EsReintentable(ex))
            {
                if (intento >= totalIntentos)
                {
                    throw new DomainException(Errores.PASARELA_NO_DISPONIBLE,
                        $"No fue posible cobrar la referencia {request.Referencia} tras {_options.MaxReintentos} reintentos.", ex);
                }

                var yaAplicado = await ConsultarCobroAsync(request.Referencia);
                if (yaAplicado?.Estado == "APROBADO")
                {
                    return yaAplicado;
                }
            }
            catch (ApiException ex) when (ex.Reason == ApiFailureReason.HttpError)
            {
                throw ConstruirRechazo(ex);
            }
            catch (ApiException ex)
            {
                throw new DomainException(Errores.ERROR_SERVICIO_EXTERNO,
                    $"La pasarela devolvió una respuesta inesperada ({ex.Reason}).", ex);
            }
        }
    }

    public async Task<CobroResponse?> ConsultarCobroAsync(string referencia)
    {
        var token = await _tokenProvider.GetTokenAsync();

        try
        {
            return await _rest.Get
                .WithBearer(token)
                .WithUri(_options.BaseUrl, $"/v1/cobros/{referencia}")
                .DeserializeWithAsync<CobroResponse>();
        }
        catch (ApiException ex) when (ex.Reason == ApiFailureReason.HttpError && ex.StatusCode == 404)
        {
            return null;
        }
        catch (ApiException ex) when (EsReintentable(ex))
        {
            throw new DomainException(Errores.PASARELA_NO_DISPONIBLE,
                "La pasarela no está disponible para consultar el cobro.", ex);
        }
        catch (ApiException ex)
        {
            throw new DomainException(Errores.ERROR_SERVICIO_EXTERNO,
                $"La pasarela respondió con un error ({ex.StatusCode}) al consultar el cobro.", ex);
        }
    }

    private async Task<CobroResponse> EnviarCobroAsync(CobroRequest request)
    {
        var token = await _tokenProvider.GetTokenAsync();

        return await _rest.Post
            .WithBearer(token)
            .WithUri(_options.BaseUrl, "/v1/cobros")
            .WithBody(request)
            .DeserializeWithAsync<CobroResponse>();
    }

    private static bool EsReintentable(ApiException ex) =>
        ex.Reason is ApiFailureReason.Timeout or ApiFailureReason.Network;

    private static DomainException ConstruirRechazo(ApiException ex)
    {
        CobroResponse? rechazo = null;
        try
        {
            rechazo = JsonConvert.DeserializeObject<CobroResponse>(ex.ResponseBody ?? "{}");
        }
        catch (JsonException)
        {
        }

        var detalle = rechazo?.Mensajes is { Count: > 0 } mensajes
            ? string.Join("; ", mensajes)
            : ex.ResponseBody ?? "sin detalle";

        return new DomainException(Errores.COBRO_RECHAZADO,
            $"La pasarela rechazó el cobro ({ex.StatusCode}): {detalle}", ex);
    }
}