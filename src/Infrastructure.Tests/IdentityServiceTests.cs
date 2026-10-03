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

public class IdentityServiceTests
{
    private static IdentityService CreateService(FakeHttpMessageHandler handler)
    {
        IRest rest = new RestBuilder(new HttpClient(handler), Options.Create(new RequestSettings()));
        var options = Options.Create(new DownstreamOptions { IdentityBaseUrl = "http://identity" });
        return new IdentityService(rest, options);
    }

    [Fact]
    public async Task GetRolesAsync_DevuelveLosRolesDeIdentity()
    {
        HttpRequestMessage? enviado = null;
        var service = CreateService(new FakeHttpMessageHandler(req =>
        {
            enviado = req;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    "[{\"rolId\":1,\"nombre\":\"Administrador\",\"descripcion\":\"Acceso total\",\"activo\":true,\"fechaCreacion\":\"2026-08-17T18:42:27\"}]")
            };
        }));

        var roles = await service.GetRolesAsync();

        var rol = Assert.Single(roles);
        Assert.Equal(1, rol.RolId);
        Assert.Equal("Administrador", rol.Nombre);
        Assert.Equal(HttpMethod.Get, enviado!.Method);
        Assert.Equal("http://identity/api/Roles", enviado.RequestUri!.ToString());
    }

    [Fact]
    public async Task GetRolByIdAsync_CuandoIdentityResponde404_LanzaRecursoNoEncontrado()
    {
        var service = CreateService(new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent("Rol no encontrado.")
        }));

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.GetRolByIdAsync(999));

        Assert.Equal(Errores.RECURSO_NO_ENCONTRADO, exception.Codigo);
    }

    [Fact]
    public async Task GetRolesAsync_CuandoIdentityNoResponde_LanzaServicioNoDisponible()
    {
        var service = CreateService(new FakeHttpMessageHandler(new HttpRequestException("Connection refused")));

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.GetRolesAsync());

        Assert.Equal(Errores.SERVICIO_NO_DISPONIBLE, exception.Codigo);
    }

    [Fact]
    public async Task GetRolesAsync_CuandoSeAgotaElTiempo_LanzaServicioNoDisponible()
    {
        var service = CreateService(new FakeHttpMessageHandler(new TaskCanceledException("Timed out")));

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.GetRolesAsync());

        Assert.Equal(Errores.SERVICIO_NO_DISPONIBLE, exception.Codigo);
    }

    [Fact]
    public async Task GetRolesAsync_CuandoIdentityResponde500_LanzaErrorServicioExterno()
    {
        var service = CreateService(new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent("boom")
        }));

        var exception = await Assert.ThrowsAsync<DomainException>(() => service.GetRolesAsync());

        Assert.Equal(Errores.ERROR_SERVICIO_EXTERNO, exception.Codigo);
        Assert.Contains("500", exception.Message);
    }

    [Fact]
    public async Task CrearRolAsync_EnviaElBodyComoJsonYDevuelveElRolCreado()
    {
        HttpRequestMessage? enviado = null;
        var service = CreateService(new FakeHttpMessageHandler(req =>
        {
            enviado = req;
            return new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(
                    "{\"rolId\":8,\"nombre\":\"Enfermería\",\"descripcion\":\"Notas\",\"activo\":true,\"fechaCreacion\":\"2026-10-03T00:00:32\"}")
            };
        }));

        var rol = await service.CrearRolAsync("Enfermería", "Notas");

        Assert.Equal(8, rol.RolId);
        Assert.Equal(HttpMethod.Post, enviado!.Method);
        Assert.Equal("http://identity/api/Roles", enviado.RequestUri!.ToString());
        Assert.Equal("application/json", enviado.Content!.Headers.ContentType!.MediaType);
        var cuerpo = await enviado.Content.ReadAsStringAsync();
        Assert.Contains("\"nombre\":\"Enfermería\"", cuerpo);
    }
}