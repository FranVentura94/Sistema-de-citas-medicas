using Core.Dtos;
using MediatR;

namespace Core.Features.Roles.Queries
{
    public class GetRolQuery : IRequest<RolDto>
    {
        public int RolId { get; set; }
    }
}