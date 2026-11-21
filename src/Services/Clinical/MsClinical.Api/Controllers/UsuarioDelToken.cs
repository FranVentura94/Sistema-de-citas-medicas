using System.Security.Claims;
using MsClinical.Core.Services;

namespace MsClinical.Api.Controllers;

// Arma el usuario actual con los datos del token JWT
public static class UsuarioDelToken
{
    public static UsuarioActual Clinico(this ClaimsPrincipal user) => new(
        user.Identity?.Name,
        user.FindFirstValue(ClaimTypes.Role),
        int.TryParse(user.FindFirstValue("medicoId"), out var medicoId) ? medicoId : null,
        int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null,
        user.FindFirstValue("nombre"));
}
