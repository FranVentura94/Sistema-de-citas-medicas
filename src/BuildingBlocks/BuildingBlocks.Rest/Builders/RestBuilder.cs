using BuildingBlocks.Rest.Configs;
using BuildingBlocks.Rest.Interfaces.IRequests;
using BuildingBlocks.Rest.Interfaces.IServices;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Rest.Builders;

public class RestBuilder : IRest
{
    private readonly HttpClient _httpClient;
    private readonly RequestSettings _requestSettings;

    public RestBuilder(HttpClient httpClient, IOptions<RequestSettings> options)
    {
        _httpClient = httpClient;
        _requestSettings = options.Value;
    }

    public INotContentRequest Get => new NotContentRequestBuilder(_httpClient, HttpMethod.Get, _requestSettings);

    public IWithContentRequest Post => new WithContentRequestBuilder(_httpClient, HttpMethod.Post, _requestSettings);

    public IWithContentRequest Put => new WithContentRequestBuilder(_httpClient, HttpMethod.Put, _requestSettings);

    public IWithContentRequest Delete => new WithContentRequestBuilder(_httpClient, HttpMethod.Delete, _requestSettings);

    public IWithContentRequest Patch => new WithContentRequestBuilder(_httpClient, HttpMethod.Patch, _requestSettings);
}