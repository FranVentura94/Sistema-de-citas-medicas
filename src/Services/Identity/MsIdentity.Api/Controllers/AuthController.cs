using System.Security.Claims;
using BuildingBlocks.Auditoria;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MsIdentity.Core.Dtos;
using MsIdentity.Core.Exceptions;
using MsIdentity.Core.Services;

namespace MsIdentity.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IAuditoria _auditoria;

    public AuthController(IAuthService authService, IAuditoria auditoria)
    {
        _authService = authService;
        _auditoria = auditoria;
    }

    // POST api/Auth/login  -> devuelve el token JWT
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var respuesta = await _authService.LoginAsync(request, cancellationToken);

            if (respuesta.RequiereCambioPassword)
                await RegistrarAsync(AccionesAuditoria.InicioSesion, "Entró con su contraseña temporal; debe crear su contraseña personal",
                    Anonimo(request.NombreUsuario));
            else
                await RegistrarAsync(AccionesAuditoria.InicioSesion, "Inició sesión", Del(respuesta));

            return Ok(respuesta);
        }
        catch (DomainException ex)
        {
            await RegistrarAsync(AccionesAuditoria.InicioSesionFallido,
                $"Intento fallido de inicio de sesión con el usuario '{request.NombreUsuario}': {ex.Message}",
                Anonimo(request.NombreUsuario), exito: false);
            throw;
        }
    }

    // POST api/Auth/primer-ingreso  -> cambia la contraseña temporal por la definitiva y entra
    [HttpPost("primer-ingreso")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> PrimerIngreso(PrimerIngresoRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var respuesta = await _authService.PrimerIngresoAsync(request, cancellationToken);
            await RegistrarAsync(AccionesAuditoria.PrimerIngreso, "Creó su contraseña personal en su primer ingreso", Del(respuesta));
            return Ok(respuesta);
        }
        catch (DomainException ex)
        {
            await RegistrarAsync(AccionesAuditoria.PrimerIngreso,
                $"No pudo completar su primer ingreso con el usuario '{request.NombreUsuario}': {ex.Message}",
                Anonimo(request.NombreUsuario), exito: false);
            throw;
        }
    }

    // POST api/Auth/logout  -> solo deja constancia en la auditoría
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await RegistrarAsync(AccionesAuditoria.CierreSesion, "Cerró sesión");
        return Ok(new { mensaje = "Sesión cerrada." });
    }

    // GET api/Auth/me  -> datos del usuario dueño del token (sirve para probar el JWT)
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        return Ok(new
        {
            usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier),
            nombreUsuario = User.Identity?.Name,
            nombre = User.FindFirstValue("nombre"),
            rol = User.FindFirstValue(ClaimTypes.Role),
            permisos = User.FindAll("permiso").Select(c => c.Value)
        });
    }

    // ---------- Auditoría ----------
    private Task RegistrarAsync(string accion, string descripcion, UsuarioAuditado? usuario = null, bool exito = true) =>
        _auditoria.RegistrarAsync(CategoriasAuditoria.Seguridad, accion, descripcion, "Usuario",
            usuario?.UsuarioId?.ToString(), exito, usuario);

    private static UsuarioAuditado Del(LoginResponse r) =>
        new(r.Usuario?.UsuarioId, r.Usuario?.NombreUsuario, r.Usuario?.NombreCompleto, r.Usuario?.Rol);

    private static UsuarioAuditado Anonimo(string? login) =>
        new(null, login?.Trim().ToLowerInvariant(), null, null);
}