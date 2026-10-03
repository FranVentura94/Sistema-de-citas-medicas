using System.Runtime.CompilerServices;

namespace BuildingBlocks.Rest.Interfaces.IFluents;

public interface IFluentContent
{
    Task<string> GetContentAsStringAsync();
    Task<byte[]> GetContentAsByteArrayAsync();
    Task<T> DeserializeWithAsync<T>();
    TaskAwaiter<HttpResponseMessage> GetAwaiter();
}