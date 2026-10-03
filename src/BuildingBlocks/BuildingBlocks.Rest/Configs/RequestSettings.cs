namespace BuildingBlocks.Rest.Configs;

public enum JsonSerializerType
{
    Newtonsoft,
    SystemTextJson
}

public class RequestSettings
{
    public bool EnableRequestLogs { get; set; }
    public JsonSerializerType Serializer { get; set; } = JsonSerializerType.Newtonsoft;
}