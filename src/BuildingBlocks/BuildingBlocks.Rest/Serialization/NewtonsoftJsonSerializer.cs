using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace BuildingBlocks.Rest.Serialization;

public class NewtonsoftJsonSerializer : IJsonSerializer
{
    public string Serialize(object value) => JsonConvert.SerializeObject(value);

    public T Deserialize<T>(string json)
    {
        try
        {
            return JsonConvert.DeserializeObject<T>(json)!;
        }
        catch (JsonException ex)
        {
            throw new SerializationException(ex.Message, ex);
        }
    }
}