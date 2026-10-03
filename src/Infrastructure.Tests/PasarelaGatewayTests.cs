using System.Net;
using BuildingBlocks.Rest.Builders;
using BuildingBlocks.Rest.Configs;
using BuildingBlocks.Rest.Interfaces.IServices;
using Core.Configs;
using Core.Exceptions;
using Core.Services;
using Domain.Models;
using Infrastructure.Services;
using Infrastructure.Tests.Fakes;
using Microsoft.Extensions.Options;

namespace Infrastructure.Tests;

public class PasarelaGatewayTests
{
    private sealed class FakeTokenProvider : ITokenProvider
    {
        public Task<string> GetTokenAsync() => Task.FromResult("token-fake");
    }

    private static PasarelaGateway CreateGateway(Func<HttpRequestMessage, HttpResponseMessage> handler, int maxReintentos = 2)
    {
        IRest rest = new RestBuilder(new HttpClient(new FakeHttpMessageHandler(handler)), Options.Create(new RequestSettings()));
        var options = Options.Create(new PasarelaOptions { BaseUrl = "http://pasarela", MaxReintentos = maxReintentos });
        return new PasarelaGateway(rest, new FakeTokenProvider(), options);
    }

    private static HttpResponseMessage Json(HttpStatusCode status, string body) =>
        new(status) { Content = new StringContent(body) };

    private static CobroRequest Cobro() => new() { Referencia = "R1", Monto = 25.5m, Moneda = "USD" };

    [Fact]
    public async Task CobrarAsync_ConExito_EnviaBearerYDevuelveElCobro()
    {
        HttpRequestMessage? enviado = null;
        var gateway = CreateGateway(req =>
        {
            enviado = req;
            return Json(HttpStatusCode.OK, "{\"estado\":\"APROBADO\",\"transaccionId\":\"TX-1\"}");
        });

        var resultado = await gateway.CobrarAsync(Cobro());

        Assert.Equal("APROBADO", resultado.Estado);
        Assert.Equal("TX-1", resultado.TransaccionId);
        Assert.Equal("http://pasarela/v1/cobros", enviado!.RequestUri!.ToString());
        Assert.Equal("Bearer", enviado.Headers.Authorization!.Scheme);
        Assert.Equal("token-fake", enviado.Headers.Authorization.Parameter);
        Assert.Contains("\"referencia\":\"R1\"", await enviado.Content!.ReadAsStringAsync());
    }

    [Fact]
    public async Task CobrarAsync_CuandoFallaLaRedYElCobroNoSeRegistro_ReintentaYDevuelveElCobro()
    {
        int posts = 0;
        int gets = 0;
        var gateway = CreateGateway(req =>
        {
            if (req.Method == HttpMethod.Post)
            {
                posts++;
                if (posts == 1) throw new HttpRequestException("Connection reset");
                return Json(HttpStatusCode.OK, "{\"estado\":\"APROBADO\",\"transaccionId\":\"TX-2\"}");
            }

            gets++;
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        var resultado = await gateway.CobrarAsync(Cobro());

        Assert.Equal("TX-2", resultado.TransaccionId);
        Assert.Equal(2, posts);
        Assert.Equal(1, gets);
    }

    [Fact]
    public async Task CobrarAsync_CuandoFallaLaRedPeroElCobroYaSeAplico_NoReenvia()
    {
        int posts = 0;
        var gateway = CreateGateway(req =>
        {
            if (req.Method == HttpMethod.Post)
            {
                posts++;
                throw new HttpRequestException("Connection reset");
            }

            return Json(HttpStatusCode.OK, "{\"estado\":\"APROBADO\",\"transaccionId\":\"TX-1\"}");
        });

        var resultado = await gateway.CobrarAsync(Cobro());

        Assert.Equal("TX-1", resultado.TransaccionId);
        Assert.Equal(1, posts);
    }

    [Fact]
    public async Task CobrarAsync_CuandoSeAgotanLosReintentos_LanzaPasarelaNoDisponible()
    {
        int posts = 0;
        var gateway = CreateGateway(req =>
        {
            if (req.Method == HttpMethod.Post)
            {
                posts++;
                throw new HttpRequestException("Connection refused");
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        }, maxReintentos: 2);

        var exception = await Assert.ThrowsAsync<DomainException>(() => gateway.CobrarAsync(Cobro()));

        Assert.Equal(Errores.PASARELA_NO_DISPONIBLE, exception.Codigo);
        Assert.Equal(3, posts);
    }

    [Fact]
    public async Task CobrarAsync_CuandoLaPasarelaRechaza_NoReintentaYLeeElMensaje()
    {
        int posts = 0;
        var gateway = CreateGateway(_ =>
        {
            posts++;
            return Json((HttpStatusCode)402,
                "{\"estado\":\"RECHAZADO\",\"mensajes\":[\"Fondos insuficientes\",\"Tarjeta vencida\"]}");
        });

        var exception = await Assert.ThrowsAsync<DomainException>(() => gateway.CobrarAsync(Cobro()));

        Assert.Equal(Errores.COBRO_RECHAZADO, exception.Codigo);
        Assert.Contains("Fondos insuficientes", exception.Message);
        Assert.Contains("Tarjeta vencida", exception.Message);
        Assert.Equal(1, posts);
    }

    [Fact]
    public async Task CobrarAsync_ConRespuestaInvalida_LanzaErrorServicioExterno()
    {
        var gateway = CreateGateway(_ => Json(HttpStatusCode.OK, "not-json"));

        var exception = await Assert.ThrowsAsync<DomainException>(() => gateway.CobrarAsync(Cobro()));

        Assert.Equal(Errores.ERROR_SERVICIO_EXTERNO, exception.Codigo);
    }

    [Fact]
    public async Task ConsultarCobroAsync_CuandoLaPasarelaNoConoceLaReferencia_DevuelveNull()
    {
        var gateway = CreateGateway(_ => new HttpResponseMessage(HttpStatusCode.NotFound));

        var resultado = await gateway.ConsultarCobroAsync("R-inexistente");

        Assert.Null(resultado);
    }
}