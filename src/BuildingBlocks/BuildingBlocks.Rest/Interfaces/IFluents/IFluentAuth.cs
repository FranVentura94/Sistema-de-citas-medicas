using System.Diagnostics.CodeAnalysis;

namespace BuildingBlocks.Rest.Interfaces.IFluents;

public interface IFluentAuth<TNext>
{
    TNext WithUri([NotNull] string uri, string endpoint = "");
    IFluentAuth<TNext> WithHeaders([NotNull] Dictionary<string, string> keyValues);
}