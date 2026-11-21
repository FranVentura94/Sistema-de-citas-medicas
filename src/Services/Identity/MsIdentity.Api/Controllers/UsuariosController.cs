using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MsIdentity.Core.Dtos;
using MsIdentity.Core.Services;
using BuildingBlocks.Auditoria;

namespace MsIdentity.Api.Controllers;

// Accesos al portal: SOLO el Administrador
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _service;
    private readonly IAuditoria _auditoria;

    public UsuariosController(IUsuarioService service, IAuditoria auditoria)
    {
        _service = service;
        _auditoria = auditoria;
    }

    private int UsuarioActualId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    // GET api/Usuarios
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct) => Ok(await _service.GetAllAsync(ct));

    // GET api/Usuarios/roles  -> niveles de acceso
    [HttpGet("roles")]
    public async Task<IActionResult> Roles(CancellationToken ct) => Ok(await _service.GetRolesAsync(ct));

    // POST api/Usuarios  -> crea el acceso (sin contraseña)
    [HttpPost]
    public async Task<IActionResult> Crear(CrearUsuarioRequest request, CancellationToken ct) =>
        Ok(await _service.CrearAsync(request, ct));

    // PUT api/Usuarios/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, ActualizarUsuarioRequest request, CancellationToken ct) =>
        Ok(await _service.ActualizarAsync(id, request, UsuarioActualId, ct));

    // POST api/Usuarios/5/password-temporal  -> se muestra una sola vez
    [HttpPost("{id:int}/password-temporal")]
    public async Task<IActionResult> GenerarPasswordTemporal(int id, CancellationToken ct)
    {
        var temporal = await _service.GenerarPasswordTemporalAsync(id, UsuarioActualId, ct);

        // La contraseña NUNCA se guarda en la auditoría: solo que se generó y quién lo hizo
        await _auditoria.RegistrarAsync(CategoriasAuditoria.Seguridad, AccionesAuditoria.ContrasenaTemporal,
            $"Generó una contraseña temporal para el usuario #{id}", "Usuario", id.ToString(), ct: ct);

        return Ok(temporal);
    }
}