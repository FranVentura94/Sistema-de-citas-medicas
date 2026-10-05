using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Core.Configs;
using Infrastructure.Services;
using Microsoft.Extensions.Options;
using NugetPackage_Rest.Builders;
using NugetPackage_Rest.Configs;
using NugetPackage_Rest.Interfaces.IServices;
using Tests.Fakes;
using Xunit;

namespace Tests
{
    public class RolServiceTests
    {
        [Fact]
        public async Task ObtenerPorIdAsync_DevuelveRol()
        {
            HttpRequestMessage? enviado = null;
            var handler = new FakeHttpMessageHandler(req =>
            {
                enviado = req;
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        "{\"succeeded\":true,\"result\":{\"rolId\":1,\"nombre\":\"Administrador\",\"descripcion\":\"Acceso total al sistema\",\"activo\":true}}")
                };
            });

            IRest rest = new RestBuilder(new HttpClient(handler), Options.Create(new RequestSettings()));
            var options = Options.Create(new DownstreamOptions { IdentityBaseUrl = "http://identity" });
            var service = new RolService(rest, options);

            var rol = await service.ObtenerPorIdAsync(1);

            Assert.NotNull(rol);
            Assert.Equal("Administrador", rol!.Nombre);
            Assert.Equal("http://identity/api/Roles/1", enviado!.RequestUri!.ToString());
        }

        [Fact]
        public async Task ObtenerPorIdAsync_Devuelve404_RetornaNull()
        {
            var handler = new FakeHttpMessageHandler(req =>
                new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent(
                        "{\"succeeded\":false,\"result\":null,\"errorCode\":404,\"errorMessage\":\"No existe un rol con Id 100.\"}")
                });

            IRest rest = new RestBuilder(new HttpClient(handler), Options.Create(new RequestSettings()));
            var options = Options.Create(new DownstreamOptions { IdentityBaseUrl = "http://identity" });
            var service = new RolService(rest, options);

            var rol = await service.ObtenerPorIdAsync(100);

            Assert.Null(rol);
        }
    }
}