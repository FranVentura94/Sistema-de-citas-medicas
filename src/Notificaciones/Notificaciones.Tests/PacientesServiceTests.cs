using System.Net;
using BuildingBlocks.Rest.Builders;
using BuildingBlocks.Rest.Configs;
using BuildingBlocks.Rest.Interfaces.IServices;
using Microsoft.Extensions.Options;
using Notificaciones.Core.Configs;
using Notificaciones.Core.Exceptions;
using Notificaciones.Infrastructure.Services;
using Notificaciones.Tests.Fakes;

namespace Notificaciones.Tests;

public class PacientesServiceTests
{
    private static PacientesService CreateService(FakeHttpMessageHandler handler)
    {
        IRest rest = new RestBuilder(new HttpClient(handler), Options.Create(new RequestSettings()));
        var options = Options.Create(new DownstreamOptions { ApiBaseUrl = "http://api" });
        return new PacientesService(rest, options);
    }

    [Fact]
    public async Task GetPorDocumentoAsync_EscapaLaQueryYDevuelveElPaciente()
    {
        HttpRequestMessage? enviado = null;
        var service = CreateService(new FakeHttpMessageHandler(req =>
        {
            enviado = req;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "{\"pacienteID\":5,\"nombres\":\"Ana\",\"apellidos\":\"Martinez\",\"numeroDocumento\":\"A&B 1\",\"email\":\"ana@correo.com\"}")
            };
        }));

        var paciente = await service.GetPorDocumentoAsync("A&B 1");

        Assert.Equal(HttpMethod.Get, enviado!.Method);
        Assert.Equal("http://api/api/Pacientes/uno?numeroDocumento=A%26B%201", enviado.RequestUri!.AbsoluteUri);
        Assert.Equal(5, paciente.PacienteID);
        Assert.Equal("Ana", paciente.Nombres);
        Assert.Equal("ana@correo.com", paciente.Email);
    }

    [Fact]
    public async Task GetPorDocumentoAsync_CuandoLaApiResponde404_LanzaPacienteNoEncontrado()
    {
        var service = CreateService(new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound)));

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.GetPorDocumentoAsync("no-existe"));

        Assert.Equal(Errores.PACIENTE_NO_ENCONTRADO, exception.Codigo);
    }

    [Fact]
    public async Task GetPorDocumentoAsync_CuandoLaApiNoResponde_LanzaServicioNoDisponible()
    {
        var service = CreateService(new FakeHttpMessageHandler(new HttpRequestException("Connection refused")));

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.GetPorDocumentoAsync("123"));

        Assert.Equal(Errores.SERVICIO_NO_DISPONIBLE, exception.Codigo);
    }

    [Fact]
    public async Task GetPorDocumentoAsync_CuandoLaApiResponde500_LanzaErrorServicioExterno()
    {
        var service = CreateService(new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError)));

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.GetPorDocumentoAsync("123"));

        Assert.Equal(Errores.ERROR_SERVICIO_EXTERNO, exception.Codigo);
    }
}