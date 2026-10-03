using System.Net;
using System.Text;
using BuildingBlocks.Rest.Builders;
using BuildingBlocks.Rest.Configs;
using BuildingBlocks.Rest.Exceptions;
using BuildingBlocks.Rest.Tests.Fakes;
using BuildingBlocks.Rest.Tests.Models;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Rest.Tests;

public class RestBuilderGetTests
{
    private static RestBuilder CreateRestBuilder(HttpClient client)
    {
        return new RestBuilder(client, Options.Create(new RequestSettings { EnableRequestLogs = false }));
    }

    [Fact]
    public async Task Get_WithSuccessResponse_ReturnsContentAsString()
    {
        var client = new HttpClient(new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("hello world")
        }));
        var rest = CreateRestBuilder(client);

        var result = await rest.Get.WithoutAuth().WithUri("https://api.example.com", "/orders").GetContentAsStringAsync();

        Assert.Equal("hello world", result);
    }

    [Fact]
    public async Task Get_WithErrorStatusCode_ThrowsApiExceptionWithHttpErrorReason()
    {
        var client = new HttpClient(new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent("{\"error\":\"not found\"}")
        }));
        var rest = CreateRestBuilder(client);

        var exception = await Assert.ThrowsAsync<ApiException>(
            () => rest.Get.WithoutAuth().WithUri("https://api.example.com", "/orders/42").GetContentAsStringAsync());

        Assert.Equal(ApiFailureReason.HttpError, exception.Reason);
        Assert.Equal(404, exception.StatusCode);
        Assert.Equal("{\"error\":\"not found\"}", exception.ResponseBody);
    }

    [Fact]
    public async Task Get_WithValidJson_DeserializesIntoTargetType()
    {
        var client = new HttpClient(new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"id\":42,\"name\":\"widget\"}")
        }));
        var rest = CreateRestBuilder(client);

        var result = await rest.Get.WithoutAuth().WithUri("https://api.example.com", "/orders/42").DeserializeWithAsync<TestOrder>();

        Assert.Equal(42, result.Id);
        Assert.Equal("widget", result.Name);
    }

    [Fact]
    public async Task Get_WithInvalidJson_ThrowsApiExceptionWithDeserializationReason()
    {
        var client = new HttpClient(new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("not-json")
        }));
        var rest = CreateRestBuilder(client);

        var exception = await Assert.ThrowsAsync<ApiException>(
            () => rest.Get.WithoutAuth().WithUri("https://api.example.com", "/orders/42").DeserializeWithAsync<TestOrder>());

        Assert.Equal(ApiFailureReason.Deserialization, exception.Reason);
        Assert.Equal("not-json", exception.ResponseBody);
        Assert.NotNull(exception.InnerException);
    }

    [Fact]
    public async Task Get_WithSuccessResponse_ReturnsContentAsByteArray()
    {
        var expectedBytes = Encoding.UTF8.GetBytes("binary-data");
        var client = new HttpClient(new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(expectedBytes)
        }));
        var rest = CreateRestBuilder(client);

        var result = await rest.Get.WithoutAuth().WithUri("https://api.example.com", "/orders/42/file").GetContentAsByteArrayAsync();

        Assert.Equal(expectedBytes, result);
    }

    [Fact]
    public async Task Get_WithQuery_AppendsEscapedQueryStringToTheUri()
    {
        HttpRequestMessage? captured = null;
        var client = new HttpClient(new FakeHttpMessageHandler(req =>
        {
            captured = req;
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("ok") };
        }));
        var rest = CreateRestBuilder(client);

        await rest.Get.WithoutAuth()
            .WithQuery(new Dictionary<string, string> { ["sku"] = "A&B 1", ["cantidad"] = "3" })
            .WithUri("https://api.example.com", "/inventario/disponibilidad")
            .GetContentAsStringAsync();

        Assert.Equal("https://api.example.com/inventario/disponibilidad?sku=A%26B%201&cantidad=3",
            captured!.RequestUri!.AbsoluteUri);
    }

    [Fact]
    public async Task Get_WithoutQuery_DoesNotAppendAnything()
    {
        HttpRequestMessage? captured = null;
        var client = new HttpClient(new FakeHttpMessageHandler(req =>
        {
            captured = req;
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("ok") };
        }));
        var rest = CreateRestBuilder(client);

        await rest.Get.WithoutAuth().WithUri("https://api.example.com", "/orders").GetContentAsStringAsync();

        Assert.Equal("https://api.example.com/orders", captured!.RequestUri!.AbsoluteUri);
    }

    private sealed class WaitForCancellationHandler : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await Task.Delay(Timeout.Infinite, cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }

    [Fact]
    public async Task Get_WhenTheTokenIsCancelled_ThrowsApiExceptionWithTimeoutReason()
    {
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(50));
        var rest = CreateRestBuilder(new HttpClient(new WaitForCancellationHandler()));

        var exception = await Assert.ThrowsAsync<ApiException>(
            () => rest.Get.WithoutAuth().WithUri("https://api.example.com", "/lento")
                .GetContentAsStringAsync(cts.Token));

        Assert.Equal(ApiFailureReason.Timeout, exception.Reason);
    }
}