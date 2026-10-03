using Domain.Models;
using MediatR;

namespace Core.Features.Roles.Queries;

public record GetRolesRemotosQuery : IRequest<List<RolDto>>;