using Core.Configs;
using Core.Dtos;
using Core.Services;
using Core.Wrappers;
using Microsoft.Extensions.Options;
using NugetPackage_Rest.Exceptions;
using NugetPackage_Rest.Interfaces.IServices;

namespace Infrastructure.Services
{
    internal class RolService : IRolService
    {
        private readonly IRest _rest;
        private readonly DownstreamOptions _options;

        public RolService(IRest rest, IOptions<DownstreamOptions> options)
        {
            _rest = rest;
            _options = options.Value;
        }

        public async Task<RolDto?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _rest.Get
                    .WithoutAuth()
                    .WithUri(_options.IdentityBaseUrl, $"/api/Roles/{id}")
                    .DeserializeWithAsync<HttpResponse<RolDto>>();

                if (response is null || !response.Succeeded)
                    return null;

                return response.Result;
            }
            catch (ApiException ex) when (ex.Reason == ApiFailureReason.HttpError && ex.StatusCode == 404)
            {
                // El servicio de Identity respondió 404: el rol no existe, no es una falla técnica.
                return null;
            }
        }
    }
}