using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MsIdentity.Core.Dtos;
using MsIdentity.Core.Services;

namespace MsIdentity.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // POST api/Auth/login  -> devuelve el token JWT
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var respuesta = await _authService.LoginAsync(request, cancellationToken);
        return Ok(respuesta);
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
}