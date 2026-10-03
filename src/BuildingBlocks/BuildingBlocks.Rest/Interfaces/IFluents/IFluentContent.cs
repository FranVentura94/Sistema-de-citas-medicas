using System.Runtime.CompilerServices;

namespace BuildingBlocks.Rest.Interfaces.IFluents;

public interface IFluentContent
{
    Task<string> GetContentAsStringAsync(CancellationToken cancellationToken = default);
    Task<byte[]> GetContentAsByteArrayAsync(CancellationToken cancellationToken = default);
    Task<T> DeserializeWithAsync<T>(CancellationToken cancellationToken = default);
    TaskAwaiter<HttpResponseMessage> GetAwaiter();
}