using BuildingBlocks.Rest.Configs;
using BuildingBlocks.Rest.Interfaces.IRequests;
using BuildingBlocks.Rest.Interfaces.IServices;
using BuildingBlocks.Rest.Serialization;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Rest.Builders;

public class RestBuilder : IRest
{
    private readonly HttpClient _httpClient;
    private readonly RequestSettings _requestSettings;
    private readonly IJsonSerializer _serializer;

    public RestBuilder(HttpClient httpClient, IOptions<RequestSettings> options)
    {
        _httpClient = httpClient;
        _requestSettings = options.Value;
        _serializer = _requestSettings.Serializer switch
        {
            JsonSerializerType.SystemTextJson => new SystemTextJsonSerializer(),
            _ => new NewtonsoftJsonSerializer()
        };
    }

    public INotContentRequest Get => new NotContentRequestBuilder(_httpClient, HttpMethod.Get, _requestSettings, _serializer);

    public IWithContentRequest Post => new WithContentRequestBuilder(_httpClient, HttpMethod.Post, _requestSettings, _serializer);

    public IWithContentRequest Put => new WithContentRequestBuilder(_httpClient, HttpMethod.Put, _requestSettings, _serializer);

    public IWithContentRequest Delete => new WithContentRequestBuilder(_httpClient, HttpMethod.Delete, _requestSettings, _serializer);

    public IWithContentRequest Patch => new WithContentRequestBuilder(_httpClient, HttpMethod.Patch, _requestSettings, _serializer);
}