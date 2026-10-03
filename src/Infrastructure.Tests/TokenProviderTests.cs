using System.Net;
using BuildingBlocks.Rest.Builders;
using BuildingBlocks.Rest.Configs;
using BuildingBlocks.Rest.Interfaces.IServices;
using Core.Configs;
using Core.Exceptions;
using Infrastructure.Services;
using Infrastructure.Tests.Fakes;
using Microsoft.Extensions.Options;

namespace Infrastructure.Tests;

public class TokenProviderTests
{
    private static TokenProvider CreateProvider(Func<HttpRequestMessage, HttpResponseMessage> handler)
    {
        IRest rest = new RestBuilder(new HttpClient(new FakeHttpMessageHandler(handler)), Options.Create(new RequestSettings()));
        var options = Options.Create(new PasarelaOptions
        {
            BaseUrl = "http://pasarela",
            ClientId = "citas-demo",
            ClientSecret = "s3cret"
        });
        return new TokenProvider(rest, options);
    }

    private static HttpResponseMessage Json(HttpStatusCode status, string body) =>
        new(status) { Content = new StringContent(body) };

    [Fact]
    public async Task GetTokenAsync_PideElTokenConFormUrlEncodedYLoDevuelve()
    {
        HttpRequestMessage? enviado = null;
        var provider = CreateProvider(req =>
        {
            enviado = req;
            return Json(HttpStatusCode.OK, "{\"access_token\":\"tok-1\",\"expires_in\":3600}");
        });

        var token = await provider.GetTokenAsync();

        Assert.Equal("tok-1", token);
        Assert.Equal("http://pasarela/oauth/token", enviado!.RequestUri!.ToString());
        Assert.Equal("application/x-www-form-urlencoded", enviado.Content!.Headers.ContentType!.MediaType);
        Assert.Equal("grant_type=client_credentials&client_id=citas-demo&client_secret=s3cret",
            await enviado.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task GetTokenAsync_LlamadasSeguidas_ReutilizanElTokenEnCache()
    {
        int llamadas = 0;
        var provider = CreateProvider(_ =>
        {
            llamadas++;
            return Json(HttpStatusCode.OK, "{\"access_token\":\"tok-1\",\"expires_in\":3600}");
        });

        var primero = await provider.GetTokenAsync();
        var segundo = await provider.GetTokenAsync();

        Assert.Equal(primero, segundo);
        Assert.Equal(1, llamadas);
    }

    [Fact]
    public async Task GetTokenAsync_CuandoElTokenYaExpiro_PideUnoNuevo()
    {
        int llamadas = 0;
        var provider = CreateProvider(_ =>
        {
            llamadas++;
            return Json(HttpStatusCode.OK, "{\"access_token\":\"tok-" + llamadas + "\",\"expires_in\":30}");
        });

        var primero = await provider.GetTokenAsync();
        var segundo = await provider.GetTokenAsync();

        Assert.Equal("tok-1", primero);
        Assert.Equal("tok-2", segundo);
        Assert.Equal(2, llamadas);
    }

    [Fact]
    public async Task GetTokenAsync_SinAccessToken_LanzaAutenticacionFallida()
    {
        var provider = CreateProvider(_ => Json(HttpStatusCode.OK, "{}"));

        var exception = await Assert.ThrowsAsync<DomainException>(() => provider.GetTokenAsync());

        Assert.Equal(Errores.AUTENTICACION_FALLIDA, exception.Codigo);
    }

    [Fact]
    public async Task GetTokenAsync_CuandoLaPasarelaRespondeError_LanzaAutenticacionFallida()
    {
        var provider = CreateProvider(_ => Json(HttpStatusCode.Unauthorized, "invalid_client"));

        var exception = await Assert.ThrowsAsync<DomainException>(() => provider.GetTokenAsync());

        Assert.Equal(Errores.AUTENTICACION_FALLIDA, exception.Codigo);
    }
}