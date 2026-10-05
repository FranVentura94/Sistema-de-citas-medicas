using System.Runtime.CompilerServices;
using System.Threading;

namespace NugetPackage_Rest.Interfaces.IFluents
{
    /// <summary>Último eslabón de la cadena: ejecuta el request y lee la respuesta.</summary>
    public interface IFluentContent
    {
        Task<string> GetContentAsStringAsync(CancellationToken cancellationToken = default);
        Task<byte[]> GetContentAsByteArrayAsync(CancellationToken cancellationToken = default);
        Task<T> DeserializeWithAsync<T>(CancellationToken cancellationToken = default);
        TaskAwaiter<HttpResponseMessage> GetAwaiter();
    }
}