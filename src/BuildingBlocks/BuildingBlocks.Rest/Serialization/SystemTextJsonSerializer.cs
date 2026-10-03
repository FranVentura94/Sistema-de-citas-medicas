using System.Runtime.Serialization;
using System.Text.Json;

namespace BuildingBlocks.Rest.Serialization;

public class SystemTextJsonSerializer : IJsonSerializer
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    public string Serialize(object value) => JsonSerializer.Serialize(value, value.GetType(), Options);

    public T Deserialize<T>(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json, Options)!;
        }
        catch (JsonException ex)
        {
            throw new SerializationException(ex.Message, ex);
        }
    }
}