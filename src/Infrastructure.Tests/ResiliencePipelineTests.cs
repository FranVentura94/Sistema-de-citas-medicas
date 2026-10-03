using System.Net;
using BuildingBlocks.Rest.Builders;
using BuildingBlocks.Rest.Exceptions;
using BuildingBlocks.Rest.Interfaces.IServices;
using Infrastructure.Tests.Fakes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Tests;

public class ResiliencePipelineTests
{
    private static IRest CreateRest(FakeHttpMessageHandler fake)
    {
        var configuration = new ConfigurationBuilder().Build();
        var services = new ServiceCollection();
        services.AddInfrastructure(configuration);
        services.AddHttpClient<IRest, RestBuilder>().ConfigurePrimaryHttpMessageHandler(() => fake);
        return services.BuildServiceProvider().GetRequiredService<IRest>();
    }

    [Fact]
    public async Task Get_CuandoFallaDosVecesYLuegoResponde_SeReintentaAutomaticamente()
    {
        int llamadas = 0;
        var rest = CreateRest(new FakeHttpMessageHandler(_ =>
        {
            llamadas++;
            if (llamadas < 3) throw new HttpRequestException("Connection reset");
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("ok") };
        }));

        var resultado = await rest.Get.WithoutAuth().WithUri("http://identity", "/api/Roles").GetContentAsStringAsync();

        Assert.Equal("ok", resultado);
        Assert.Equal(3, llamadas);
    }

    [Fact]
    public async Task Get_CuandoElServicioSiempreRespondeError_ReintentaYAlFinalLanzaHttpError()
    {
        int llamadas = 0;
        var rest = CreateRest(new FakeHttpMessageHandler(_ =>
        {
            llamadas++;
            return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
        }));

        var exception = await Assert.ThrowsAsync<ApiException>(
            () => rest.Get.WithoutAuth().WithUri("http://identity", "/api/Roles").GetContentAsStringAsync());

        Assert.Equal(ApiFailureReason.HttpError, exception.Reason);
        Assert.Equal(503, exception.StatusCode);
        Assert.Equal(3, llamadas);
    }

    [Fact]
    public async Task Post_CuandoFalla_NoSeReintentaParaNoDuplicarLaOperacion()
    {
        int llamadas = 0;
        var rest = CreateRest(new FakeHttpMessageHandler(_ =>
        {
            llamadas++;
            throw new HttpRequestException("Connection reset");
        }));

        var exception = await Assert.ThrowsAsync<ApiException>(
            () => rest.Post.WithoutAuth().WithUri("http://pasarela", "/v1/cobros")
                .WithBody(new { monto = 10 })
                .GetContentAsStringAsync());

        Assert.Equal(ApiFailureReason.Network, exception.Reason);
        Assert.Equal(1, llamadas);
    }
}