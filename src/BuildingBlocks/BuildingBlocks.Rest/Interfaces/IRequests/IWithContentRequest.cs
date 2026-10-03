using BuildingBlocks.Rest.Interfaces.IFluents;

namespace BuildingBlocks.Rest.Interfaces.IRequests;

public interface IWithContentRequest
{
    IFluentAuth<IFluentFormat> WithoutAuth();
    IFluentAuth<IFluentFormat> WithBearer(string token);
    IFluentAuth<IFluentFormat> WithBasic(string user, string password);
}