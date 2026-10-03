using System.Net;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using BuildingBlocks.Rest.Builders;
using BuildingBlocks.Rest.Configs;
using BuildingBlocks.Rest.Serialization;
using BuildingBlocks.Rest.Tests.Fakes;
using BuildingBlocks.Rest.Tests.Models;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Rest.Tests;

public class JsonSerializerTests
{
    private class Cliente
    {
        [JsonPropertyName("full_name")]
        public string Nombre { get; set; } = string.Empty;
    }

    public static IEnumerable<object[]> Serializers() =>
    [
        [new NewtonsoftJsonSerializer()],
        [new SystemTextJsonSerializer()]
    ];

    [Theory]
    [MemberData(nameof(Serializers))]
    public void RoundTrip_SerializesAndDeserializesTheSameObject(IJsonSerializer serializer)
    {
        var json = serializer.Serialize(new TestOrder { Id = 7, Name = "widget" });

        var result = serializer.Deserialize<TestOrder>(json);

        Assert.Equal(7, result.Id);
        Assert.Equal("widget", result.Name);
    }

    [Theory]
    [MemberData(nameof(Serializers))]
    public void Deserialize_IsCaseInsensitive(IJsonSerializer serializer)
    {
        var result = serializer.Deserialize<TestOrder>("{\"id\":42,\"name\":\"widget\"}");

        Assert.Equal(42, result.Id);
        Assert.Equal("widget", result.Name);
    }

    [Theory]
    [MemberData(nameof(Serializers))]
    public void Deserialize_WithInvalidJson_ThrowsSerializationException(IJsonSerializer serializer)
    {
        var exception = Assert.Throws<SerializationException>(() => serializer.Deserialize<TestOrder>("not-json"));

        Assert.NotNull(exception.InnerException);
    }

    [Fact]
    public void JsonPropertyName_OnlyIsHonoredBySystemTextJson()
    {
        const string json = "{\"full_name\":\"Ana\"}";

        Assert.Equal("Ana", new SystemTextJsonSerializer().Deserialize<Cliente>(json).Nombre);
        Assert.Equal(string.Empty, new NewtonsoftJsonSerializer().Deserialize<Cliente>(json).Nombre);
    }

    [Fact]
    public async Task RestBuilder_WithSystemTextJson_SerializesBodyAndDeserializesResponse()
    {
        HttpRequestMessage? captured = null;
        var client = new HttpClient(new FakeHttpMessageHandler(req =>
        {
            captured = req;
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"id\":9,\"name\":\"stj\"}") };
        }));
        var rest = new RestBuilder(client, Options.Create(new RequestSettings { Serializer = JsonSerializerType.SystemTextJson }));

        var result = await rest.Post.WithoutAuth().WithUri("https://api.example.com", "/orders")
            .WithBody(new { Name = "widget" })
            .DeserializeWithAsync<TestOrder>();

        Assert.Equal(9, result.Id);
        Assert.Equal("stj", result.Name);
        Assert.Contains("\"Name\":\"widget\"", await captured!.Content!.ReadAsStringAsync());
    }
}