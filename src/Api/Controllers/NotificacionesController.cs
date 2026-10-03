using Microsoft.AspNetCore.Mvc;
using Notificaciones.Core.Services;

namespace Notificaciones.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificacionesController : ControllerBase
{
    private readonly INotificacionService _notificacionService;

    public NotificacionesController(INotificacionService notificacionService)
    {
        _notificacionService = notificacionService;
    }

    [HttpPost("bienvenida")]
    public async Task<IActionResult> EnviarBienvenida(BienvenidaRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.NumeroDocumento))
            return BadRequest("El numero de documento es obligatorio.");

        var resultado = await _notificacionService.EnviarBienvenidaAsync(request.NumeroDocumento);
        return Ok(resultado);
    }
}

public record BienvenidaRequest(string NumeroDocumento);