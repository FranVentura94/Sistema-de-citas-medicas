using System.Security.Claims;
using MsAgenda.Core.Dtos;

namespace MsAgenda.Api.Controllers;

// Arma el usuario de Agenda con los datos del token JWT
public static class UsuarioDelToken
{
    public static UsuarioAgenda Agenda(this ClaimsPrincipal user) => new(
        int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null,
        user.Identity?.Name,
        user.FindFirstValue("nombre"),
        user.FindFirstValue(ClaimTypes.Role) ?? string.Empty,
        int.TryParse(user.FindFirstValue("medicoId"), out var medicoId) ? medicoId : null);
}
