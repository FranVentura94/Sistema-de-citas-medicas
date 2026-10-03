using BuildingBlocks.Rest.Configs;
using BuildingBlocks.Rest.Exceptions;
using BuildingBlocks.Rest.Extensions;
using BuildingBlocks.Rest.Interfaces.IFluents;
using BuildingBlocks.Rest.Interfaces.IRequests;
using Newtonsoft.Json;
using Serilog;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;

namespace BuildingBlocks.Rest.Builders;

internal class NotContentRequestBuilder : INotContentRequest, IFluentAuth<IFluentContent>, IFluentContent
{
    private readonly HttpClient _client;
    private readonly HttpRequestMessage _request;
    private readonly RequestSettings _settings;
    private string _query = string.Empty;

    public NotContentRequestBuilder(HttpClient client, HttpMethod method, RequestSettings settings)
    {
        _client = client;
        _request = new HttpRequestMessage { Method = method };
        _settings = settings;
    }

    public IFluentAuth<IFluentContent> WithBasic(string user, string password)
    {
        var basicAuth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{user}:{password}"));
        _request.Headers.Authorization = new AuthenticationHeaderValue("Basic", basicAuth);
        return this;
    }

    public IFluentAuth<IFluentContent> WithBearer(string token)
    {
        _request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return this;
    }

    public IFluentAuth<IFluentContent> WithoutAuth()
    {
        return this;
    }

    public IFluentContent WithUri([NotNull] string uri, string endpoint = "")
    {
        _request.RequestUri = new Uri($"{uri}{endpoint ?? String.Empty}{_query}");
        return this;
    }

    public IFluentAuth<IFluentContent> WithHeaders([NotNull] Dictionary<string, string> keyValues)
    {
        _request.AddHeaders(keyValues);
        return this;
    }

    public IFluentAuth<IFluentContent> WithQuery([NotNull] IDictionary<string, string> parametros)
    {
        _query = RequestExtensions.BuildQueryString(parametros);
        return this;
    }

    public async Task<string> GetContentAsStringAsync(CancellationToken cancellationToken = default)
    {
        WriteRequestLog();
        var response = await HttpRequestExecutor.SendAsync(_client, _request, cancellationToken);
        await HttpRequestExecutor.EnsureSuccessAsync(response, _request);
        return await HttpRequestExecutor.ReadContentAsStringAsync(response, _request);
    }

    public async Task<byte[]> GetContentAsByteArrayAsync(CancellationToken cancellationToken = default)
    {
        WriteRequestLog();
        var response = await HttpRequestExecutor.SendAsync(_client, _request, cancellationToken);
        await HttpRequestExecutor.EnsureSuccessAsync(response, _request);
        return await HttpRequestExecutor.ReadContentAsByteArrayAsync(response, _request);
    }

    public async Task<T> DeserializeWithAsync<T>(CancellationToken cancellationToken = default)
    {
        WriteRequestLog();
        var response = await HttpRequestExecutor.SendAsync(_client, _request, cancellationToken);
        await HttpRequestExecutor.EnsureSuccessAsync(response, _request);
        var content = await HttpRequestExecutor.ReadContentAsStringAsync(response, _request);
        try
        {
            return JsonConvert.DeserializeObject<T>(content)!;
        }
        catch (JsonException ex)
        {
            throw HttpRequestExecutor.LogAndBuild(
                $"Response from {_request.Method} {_request.RequestUri} could not be deserialized into {typeof(T).Name}: {ex.Message}",
                ApiFailureReason.Deserialization, _request, ex, responseBody: content);
        }
    }

    public TaskAwaiter<HttpResponseMessage> GetAwaiter()
    {
        return _client.SendAsync(_request).GetAwaiter();
    }

    private void WriteRequestLog()
    {
        if (_settings.EnableRequestLogs)
            Log.ForContext("Method", _request.Method)
                .ForContext("url", _request.RequestUri)
                .Information("Sending request");
    }
}