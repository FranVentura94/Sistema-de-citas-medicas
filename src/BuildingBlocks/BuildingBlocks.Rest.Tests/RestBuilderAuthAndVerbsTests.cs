using System.Net;
using System.Text;
using BuildingBlocks.Rest.Builders;
using BuildingBlocks.Rest.Configs;
using BuildingBlocks.Rest.Exceptions;
using BuildingBlocks.Rest.Tests.Fakes;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Rest.Tests;

public class RestBuilderAuthAndVerbsTests
{
    private static RestBuilder CreateRestBuilder(FakeHttpMessageHandler handler)
    {
        return new RestBuilder(new HttpClient(handler), Options.Create(new RequestSettings()));
    }

    private static HttpResponseMessage Ok() =>
        new(HttpStatusCode.OK) { Content = new StringContent("ok") };

    [Fact]
    public async Task Get_WithBearer_SendsAuthorizationBearerHeader()
    {
        HttpRequestMessage? captured = null;
        var rest = CreateRestBuilder(new FakeHttpMessageHandler(req => { captured = req; return Ok(); }));

        await rest.Get.WithBearer("abc123").WithUri("https://api.example.com", "/orders").GetContentAsStringAsync();

        Assert.Equal("Bearer", captured!.Headers.Authorization!.Scheme);
        Assert.Equal("abc123", captured.Headers.Authorization.Parameter);
    }

    [Fact]
    public async Task Get_WithBasic_SendsBase64EncodedCredentials()
    {
        HttpRequestMessage? captured = null;
        var rest = CreateRestBuilder(new FakeHttpMessageHandler(req => { captured = req; return Ok(); }));

        await rest.Get.WithBasic("usuario01", "secret").WithUri("https://api.example.com", "/orders").GetContentAsStringAsync();

        var esperado = Convert.ToBase64String(Encoding.UTF8.GetBytes("usuario01:secret"));
        Assert.Equal("Basic", captured!.Headers.Authorization!.Scheme);
        Assert.Equal(esperado, captured.Headers.Authorization.Parameter);
    }

    [Fact]
    public async Task Get_WithoutAuth_DoesNotSendAuthorizationHeader()
    {
        HttpRequestMessage? captured = null;
        var rest = CreateRestBuilder(new FakeHttpMessageHandler(req => { captured = req; return Ok(); }));

        await rest.Get.WithoutAuth().WithUri("https://api.example.com", "/orders").GetContentAsStringAsync();

        Assert.Null(captured!.Headers.Authorization);
    }

    [Fact]
    public async Task Get_WithHeaders_SendsCustomHeader()
    {
        HttpRequestMessage? captured = null;
        var rest = CreateRestBuilder(new FakeHttpMessageHandler(req => { captured = req; return Ok(); }));

        await rest.Get.WithoutAuth()
            .WithHeaders(new Dictionary<string, string> { ["X-Correlation-Id"] = "corr-1" })
            .WithUri("https://api.example.com", "/orders")
            .GetContentAsStringAsync();

        Assert.Equal("corr-1", Assert.Single(captured!.Headers.GetValues("X-Correlation-Id")));
    }

    [Fact]
    public async Task Put_SendsPutMethodWithJsonBody()
    {
        HttpRequestMessage? captured = null;
        var rest = CreateRestBuilder(new FakeHttpMessageHandler(req => { captured = req; return Ok(); }));

        await rest.Put.WithoutAuth().WithUri("https://api.example.com", "/orders/1")
            .WithBody(new { Name = "widget" })
            .GetContentAsStringAsync();

        Assert.Equal(HttpMethod.Put, captured!.Method);
        Assert.Equal("application/json", captured.Content!.Headers.ContentType!.MediaType);
    }

    [Fact]
    public async Task Patch_SendsPatchMethodWithJsonBody()
    {
        HttpRequestMessage? captured = null;
        var rest = CreateRestBuilder(new FakeHttpMessageHandler(req => { captured = req; return Ok(); }));

        await rest.Patch.WithoutAuth().WithUri("https://api.example.com", "/orders/1")
            .WithBody(new { activo = false })
            .GetContentAsStringAsync();

        Assert.Equal(HttpMethod.Patch, captured!.Method);
        Assert.Contains("\"activo\":false", await captured.Content!.ReadAsStringAsync());
    }

    [Fact]
    public async Task Delete_WithoutBody_SendsDeleteMethodWithoutContent()
    {
        HttpRequestMessage? captured = null;
        var rest = CreateRestBuilder(new FakeHttpMessageHandler(req => { captured = req; return Ok(); }));

        await rest.Delete.WithoutAuth().WithUri("https://api.example.com", "/orders/1")
            .WithoutBody()
            .GetContentAsStringAsync();

        Assert.Equal(HttpMethod.Delete, captured!.Method);
        Assert.Null(captured.Content);
    }

    [Fact]
    public async Task Post_WithFormData_SendsMultipartContentWithTheFile()
    {
        HttpRequestMessage? captured = null;
        var rest = CreateRestBuilder(new FakeHttpMessageHandler(req => { captured = req; return Ok(); }));
        using var form = new MultipartFormDataContent();
        form.Add(new ByteArrayContent(Encoding.UTF8.GetBytes("contenido-del-archivo")), "archivo", "producto.png");
        form.Add(new StringContent("42"), "productoId");

        await rest.Post.WithoutAuth().WithUri("https://api.example.com", "/productos/42/imagen")
            .WithFormData(form)
            .GetContentAsStringAsync();

        Assert.Equal("multipart/form-data", captured!.Content!.Headers.ContentType!.MediaType);
        var body = await captured.Content.ReadAsStringAsync();
        Assert.Contains("producto.png", body);
        Assert.Contains("contenido-del-archivo", body);
    }

    [Fact]
    public async Task Get_AwaitDirecto_DevuelveLaRespuestaCrudaSinLanzarExcepcionAunqueSea404()
    {
        var rest = CreateRestBuilder(new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound)));

        var response = await rest.Get.WithoutAuth().WithUri("https://api.example.com", "/orders/42");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Get_EjecutarLaMismaCadenaDosVeces_LanzaApiExceptionUnknown()
    {
        var rest = CreateRestBuilder(new FakeHttpMessageHandler(_ => Ok()));
        var cadena = rest.Get.WithoutAuth().WithUri("https://api.example.com", "/orders");

        await cadena.GetContentAsStringAsync();
        var exception = await Assert.ThrowsAsync<ApiException>(() => cadena.GetContentAsStringAsync());

        Assert.Equal(ApiFailureReason.Unknown, exception.Reason);
    }
}