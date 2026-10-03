using BuildingBlocks.Rest.Interfaces.IRequests;

namespace BuildingBlocks.Rest.Interfaces.IServices;

public interface IRest
{
    INotContentRequest Get { get; }
    IWithContentRequest Post { get; }
    IWithContentRequest Put { get; }
    IWithContentRequest Delete { get; }
    IWithContentRequest Patch { get; }
}