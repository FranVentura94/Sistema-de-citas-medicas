using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Auditoria;

// ===================== GET api/Auditoria (solo Administrador) =====================
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrador")]
public class AuditoriaController : ControllerBase
{
    private readonly IAuditoriaLector _lector;

    public AuditoriaController(IAuditoriaLector lector)
    {
        _lector = lector;
    }

    // GET api/Auditoria?desde=2026-11-20&hasta=2026-11-21&categoria=Seguridad&entidad=Paciente&entidadId=3&usuarioId=1&max=500
    [HttpGet]
    public async Task<IActionResult> Buscar([FromQuery] FiltroAuditoria filtro, CancellationToken ct) =>
        Ok(await _lector.BuscarAsync(filtro, ct));
}

// ===================== [Auditar] para registrar quién CONSULTÓ información sensible =====================
// Uso: [Auditar("Consultó el expediente clínico", "Paciente", "pacienteId")]
[AttributeUsage(AttributeTargets.Method)]
public class AuditarAttribute : Attribute, IAsyncActionFilter
{
    private readonly string _descripcion;
    private readonly string _entidad;
    private readonly string _parametroId;

    public AuditarAttribute(string descripcion, string entidad, string parametroId)
    {
        _descripcion = descripcion;
        _entidad = entidad;
        _parametroId = parametroId;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var resultado = await next();

        // Solo si la consulta salió bien
        if (resultado.Exception is not null && !resultado.ExceptionHandled) return;
        if (resultado.Result is ObjectResult { StatusCode: >= 400 } or StatusCodeResult { StatusCode: >= 400 }) return;

        var id = context.ActionArguments.TryGetValue(_parametroId, out var valor) ? valor?.ToString() : null;
        var nombre = NombreEnRespuesta((resultado.Result as ObjectResult)?.Value);
        var descripcion = $"{_descripcion} — {_entidad} #{id}" + (nombre is null ? "" : $" ({nombre})");

        var auditoria = context.HttpContext.RequestServices.GetRequiredService<IAuditoria>();
        await auditoria.RegistrarAsync(CategoriasAuditoria.Consulta, AccionesAuditoria.Consultar, descripcion, _entidad, id);
    }

    // Busca el nombre del paciente en lo que se devolvió (PacienteNombre, NombreCompleto o Nombres + Apellidos)
    private static string? NombreEnRespuesta(object? valor)
    {
        if (valor is null) return null;
        var tipo = valor.GetType();
        string? Leer(string p) => tipo.GetProperty(p, BindingFlags.Public | BindingFlags.Instance)?.GetValue(valor) as string;

        return Leer("PacienteNombre")
            ?? Leer("NombreCompleto")
            ?? (Leer("Nombres") is string n ? $"{n} {Leer("Apellidos")}".Trim() : null);
    }
}

// ===================== Registra los intentos de entrar sin permiso =====================
public class AuditoriaAccesosMiddleware
{
    private readonly RequestDelegate _next;

    public AuditoriaAccesosMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IAuditoria auditoria)
    {
        await _next(context);

        var codigo = context.Response.StatusCode;
        var ruta = $"{context.Request.Method} {context.Request.Path}";

        if (codigo == StatusCodes.Status403Forbidden)
        {
            await auditoria.RegistrarAsync(CategoriasAuditoria.Seguridad, AccionesAuditoria.AccesoDenegado,
                $"Intentó una acción sin permiso: {ruta}", exito: false);
        }
        else if (codigo == StatusCodes.Status401Unauthorized && context.Request.Headers.Authorization.Count > 0)
        {
            await auditoria.RegistrarAsync(CategoriasAuditoria.Seguridad, AccionesAuditoria.SesionInvalida,
                $"Usó una sesión vencida o no válida: {ruta}", exito: false);
        }
    }
}