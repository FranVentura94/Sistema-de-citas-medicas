using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Auditoria;

// Arma cada registro con los datos de la petición actual: usuario del token, IP, dispositivo y ruta
public class ContextoAuditoria
{
    private readonly IHttpContextAccessor _http;
    private readonly AuditoriaOpciones _opciones;

    public ContextoAuditoria(IHttpContextAccessor http, AuditoriaOpciones opciones)
    {
        _http = http;
        _opciones = opciones;
    }

    public AuditoriaOpciones Opciones => _opciones;

    public RegistroAuditoria Nuevo(string categoria, string accion, string descripcion, UsuarioAuditado? usuario = null)
    {
        var u = usuario ?? UsuarioDelToken();
        var ctx = _http.HttpContext;

        return new RegistroAuditoria
        {
            Fecha = DateTime.Now,
            Servicio = _opciones.Servicio,
            Categoria = categoria,
            Accion = accion,
            Descripcion = Recortar(descripcion, 500)!,
            UsuarioId = u.UsuarioId,
            UsuarioLogin = Recortar(u.Login, 50),
            UsuarioNombre = Recortar(u.Nombre, 150),
            Rol = Recortar(u.Rol, 50),
            Ip = Recortar(Ip(ctx), 60),
            Dispositivo = Recortar(Dispositivo(ctx), 100),
            Ruta = ctx is null ? null : Recortar($"{ctx.Request.Method} {ctx.Request.Path}", 250)
        };
    }

    public UsuarioAuditado UsuarioDelToken()
    {
        var user = _http.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
            return new UsuarioAuditado(null, null, null, null);

        var id = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
        return new UsuarioAuditado(
            int.TryParse(id, out var n) ? n : null,
            user.Identity.Name ?? user.FindFirstValue("unique_name"),
            user.FindFirstValue("nombre"),
            user.FindFirstValue(ClaimTypes.Role));
    }

    // Detrás del Gateway, la IP real del navegador viene en X-Forwarded-For
    private static string? Ip(HttpContext? ctx)
    {
        if (ctx is null) return null;
        var reenviada = ctx.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(reenviada)) return reenviada.Split(',')[0].Trim();
        return ctx.Connection.RemoteIpAddress?.ToString();
    }

    // "Mozilla/5.0 (Windows NT 10.0...) Chrome/131..." -> "Chrome · Windows"
    private static string? Dispositivo(HttpContext? ctx)
    {
        var ua = ctx?.Request.Headers.UserAgent.ToString();
        if (string.IsNullOrWhiteSpace(ua)) return null;

        var navegador = ua.Contains("Edg/") ? "Edge"
            : ua.Contains("OPR/") ? "Opera"
            : ua.Contains("Chrome/") ? "Chrome"
            : ua.Contains("Firefox/") ? "Firefox"
            : ua.Contains("Safari/") ? "Safari"
            : ua.Contains("PostmanRuntime") ? "Postman"
            : "Otro";

        var sistema = ua.Contains("Android") ? "Android"
            : ua.Contains("iPhone") || ua.Contains("iPad") ? "iPhone/iPad"
            : ua.Contains("Windows") ? "Windows"
            : ua.Contains("Mac OS") ? "Mac"
            : ua.Contains("Linux") ? "Linux"
            : "Otro";

        return $"{navegador} · {sistema}";
    }

    private static string? Recortar(string? texto, int max) =>
        texto is null ? null : texto.Length <= max ? texto : texto[..max];
}