using System.Diagnostics.CodeAnalysis;

namespace BuildingBlocks.Rest.Interfaces.IFluents;

public interface IFluentFormat
{
    IFluentContent WithoutBody();
    IFluentContent WithBody([NotNull] object body);
    IFluentContent WithFormData([NotNull] MultipartFormDataContent content);
    IFluentContent WithFormUrlEncoded([NotNull] IDictionary<string, string> data);
}