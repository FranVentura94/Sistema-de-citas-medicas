using System.Text;
using BuildingBlocks.Rest.Serialization;

namespace BuildingBlocks.Rest.Extensions;

public static class RequestExtensions
{
    public static void AddHeaders(this HttpRequestMessage request, IDictionary<string, string> headers)
    {
        if (headers == null) return;
        foreach (var header in headers)
        {
            request.Headers.Add(header.Key, header.Value);
        }
    }

    public static void AddContent(this HttpRequestMessage request, object body)
    {
        request.AddContent(body, new NewtonsoftJsonSerializer());
    }

    public static void AddContent(this HttpRequestMessage request, object body, IJsonSerializer serializer)
    {
        if (body == null) return;
        request.Content = new StringContent(serializer.Serialize(body), Encoding.UTF8, "application/json");
    }

    public static void AddFormDataContent(this HttpRequestMessage request, MultipartFormDataContent content)
    {
        if (content == null) return;
        request.Content = content;
    }

    public static void AddFormUrlEncodedContent(this HttpRequestMessage request, IDictionary<string, string> data)
    {
        if (data == null) return;
        request.Content = new FormUrlEncodedContent(data);
    }

    public static string BuildQueryString(IDictionary<string, string>? parametros)
    {
        if (parametros == null || parametros.Count == 0) return string.Empty;

        return "?" + string.Join("&", parametros.Select(p =>
            $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
    }
}