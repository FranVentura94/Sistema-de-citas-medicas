using BuildingBlocks.Rest.Exceptions;
using BuildingBlocks.Rest.Interfaces.IServices;
using Core.Configs;
using Core.Exceptions;
using Core.Services;
using Domain.Models;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services;

internal class IdentityService : IIdentityService
{
    private readonly IRest _rest;
    private readonly DownstreamOptions _options;

    public IdentityService(IRest rest, IOptions<DownstreamOptions> options)
    {
        _rest = rest;
        _options = options.Value;
    }

    public Task<List<RolDto>> GetRolesAsync()
    {
        return EjecutarAsync(() => _rest.Get
            .WithoutAuth()
            .WithUri(_options.IdentityBaseUrl, "/api/Roles")
            .DeserializeWithAsync<List<RolDto>>());
    }

    public Task<RolDto> GetRolByIdAsync(int id)
    {
        return EjecutarAsync(() => _rest.Get
            .WithoutAuth()
            .WithUri(_options.IdentityBaseUrl, $"/api/Roles/{id}")
            .DeserializeWithAsync<RolDto>());
    }

    public Task<RolDto> CrearRolAsync(string nombre, string? descripcion)
    {
        return EjecutarAsync(() => _rest.Post
            .WithoutAuth()
            .WithUri(_options.IdentityBaseUrl, "/api/Roles")
            .WithBody(new { nombre, descripcion })
            .DeserializeWithAsync<RolDto>());
    }

    private static async Task<T> EjecutarAsync<T>(Func<Task<T>> llamada)
    {
        try
        {
            return await llamada();
        }
        catch (ApiException ex) when (ex.Reason == ApiFailureReason.HttpError && ex.StatusCode == 404)
        {
            throw new DomainException(Errores.RECURSO_NO_ENCONTRADO,
                "El recurso solicitado no existe en el servicio de Identity.", ex);
        }
        catch (ApiException ex) when (ex.EsTransitoria())
        {
            throw new DomainException(Errores.SERVICIO_NO_DISPONIBLE,
                "El servicio de Identity no está disponible. Intentá de nuevo más tarde.", ex);
        }
        catch (ApiException ex)
        {
            throw new DomainException(Errores.ERROR_SERVICIO_EXTERNO,
                $"Identity respondió con un error ({ex.StatusCode}): {ex.ResponseBody ?? ex.Message}", ex);
        }
    }
}