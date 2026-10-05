using Core.Dtos;
using Core.Exceptions;
using Core.Services;
using MediatR;

namespace Core.Features.Roles.Queries
{
    internal class GetRolQueryHandler : IRequestHandler<GetRolQuery, RolDto>
    {
        private readonly IRolService _rolService;

        public GetRolQueryHandler(IRolService rolService)
        {
            _rolService = rolService;
        }

        public async Task<RolDto> Handle(GetRolQuery request, CancellationToken cancellationToken)
        {
            RolDto? rol;
            try
            {
                rol = await _rolService.ObtenerPorIdAsync(request.RolId, cancellationToken);
            }
            catch (Exception ex) when (ex is not DomainException)
            {
                throw new DomainException(Errores.IDENTITY_NO_DISPONIBLE,
                    $"No fue posible consultar el servicio de Identity: {ex.Message}");
            }

            if (rol is null)
                throw new DomainException(Errores.ROL_NO_ENCONTRADO, $"No existe un rol con Id {request.RolId}.");

            return rol;
        }
    }
}