namespace BuildingBlocks.Rest.Exceptions;

public enum ApiFailureReason
{
    Unknown,
    Network,
    Timeout,
    HttpError,
    Deserialization
}