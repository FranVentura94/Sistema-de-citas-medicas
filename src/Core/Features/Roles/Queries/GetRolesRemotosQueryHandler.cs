using Core.Services;
using Domain.Models;
using MediatR;

namespace Core.Features.Roles.Queries;

public class GetRolesRemotosQueryHandler : IRequestHandler<GetRolesRemotosQuery, List<RolDto>>
{
    private readonly IIdentityService _identityService;

    public GetRolesRemotosQueryHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<List<RolDto>> Handle(
        GetRolesRemotosQuery request,
        CancellationToken cancellationToken)
    {
        return await _identityService.GetRolesAsync();
    }
}