using Identity.Data;
using Identity.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Identity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly IdentityDbContext _context;

        public RolesController(IdentityDbContext context)
        {
            _context = context;
        }

        // GET: api/Roles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Rol>>> GetRoles()
        {
            return await _context.Roles.ToListAsync();
        }

        // GET: api/Roles/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Rol>> GetRolById(int id)
        {
            var rol = await _context.Roles.FindAsync(id);

            if (rol == null)
            {
                return NotFound("Rol no encontrado.");
            }

            return Ok(rol);
        }

        // POST: api/Roles
        [HttpPost]
        public async Task<ActionResult<Rol>> CreateRol([FromBody] CreateRolDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Nombre))
            {
                return BadRequest("El nombre del rol es obligatorio.");
            }

            var nuevoRol = new Rol
            {
                Nombre = request.Nombre,
                Descripcion = request.Descripcion,
                Activo = true,
                FechaCreacion = DateTime.Now
            };

            _context.Roles.Add(nuevoRol);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRolById), new { id = nuevoRol.RolId }, nuevoRol);
        }
    }

    // DTO para recibir los datos desde Swagger / Frontend
    public class CreateRolDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
    }
}