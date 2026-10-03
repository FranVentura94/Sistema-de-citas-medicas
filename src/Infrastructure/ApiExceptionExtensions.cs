using BuildingBlocks.Rest.Exceptions;
using Polly;

namespace Infrastructure;

internal static class ApiExceptionExtensions
{
    public static bool EsTransitoria(this ApiException ex) =>
        ex.Reason is ApiFailureReason.Network or ApiFailureReason.Timeout
        || ex.InnerException is ExecutionRejectedException;
}