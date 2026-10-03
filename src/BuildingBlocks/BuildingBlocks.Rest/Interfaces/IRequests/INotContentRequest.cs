using BuildingBlocks.Rest.Interfaces.IFluents;

namespace BuildingBlocks.Rest.Interfaces.IRequests;

public interface INotContentRequest
{
    IFluentAuth<IFluentContent> WithoutAuth();
    IFluentAuth<IFluentContent> WithBearer(string token);
    IFluentAuth<IFluentContent> WithBasic(string user, string password);
}