using NugetPackage_Rest.Configs;
using NugetPackage_Rest.Exceptions;
using NugetPackage_Rest.Extensions;
using NugetPackage_Rest.Interfaces.IFluents;
using NugetPackage_Rest.Interfaces.IRequests;
using Newtonsoft.Json;
using Serilog;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace NugetPackage_Rest.Builders
{
    /// <summary>Builder para requests sin body (GET). Implementa todos los eslabones
    /// de su cadena y devuelve "this" en cada paso.</summary>
    internal class NotContentRequestBuilder : INotContentRequest, IFluentAuth<IFluentContent>, IFluentContent
    {
        private readonly HttpClient _client;
        private readonly HttpRequestMessage _request;
        private readonly RequestSettings _settings;
        private readonly Dictionary<string, string> _queryParameters = new();

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
            var uriCompleta = $"{uri}{endpoint ?? String.Empty}";

            if (_queryParameters.Count > 0)
            {
                var separador = uriCompleta.Contains('?') ? "&" : "?";
                var queryString = string.Join("&", _queryParameters.Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value)}"));
                uriCompleta = $"{uriCompleta}{separador}{queryString}";
            }

            _request.RequestUri = new Uri(uriCompleta);
            return this;
        }

        public IFluentAuth<IFluentContent> WithHeaders([NotNull] Dictionary<string, string> keyValues)
        {
            _request.AddHeaders(keyValues);
            return this;
        }

        public IFluentAuth<IFluentContent> WithQuery([NotNull] IDictionary<string, string> parametros)
        {
            foreach (var parametro in parametros)
            {
                _queryParameters[parametro.Key] = parametro.Value;
            }
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
}