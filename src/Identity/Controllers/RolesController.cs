using Identity.Data;
using Identity.Dtos;
using Identity.Wrappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Identity.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly IdentityDbContext _context;

        public RolesController(IdentityDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObtenerPorId(int id, CancellationToken cancellationToken)
        {
            var rol = await _context.Roles
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.RolId == id, cancellationToken);

            if (rol is null)
            {
                return NotFound(new HttpResponse<object>
                {
                    Succeeded = false,
                    ErrorCode = 404,
                    ErrorMessage = $"No existe un rol con Id {id}."
                });
            }

            var dto = new RolDto
            {
                RolId = rol.RolId,
                Nombre = rol.Nombre,
                Descripcion = rol.Descripcion,
                Activo = rol.Activo
            };

            return Ok(new HttpResponse<RolDto>
            {
                Succeeded = true,
                Result = dto
            });
        }
    }
}