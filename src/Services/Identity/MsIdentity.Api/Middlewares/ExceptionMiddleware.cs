using MsIdentity.Core.Exceptions;

namespace MsIdentity.Api.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning("Error de dominio {Codigo}: {Mensaje}", ex.Codigo, ex.Message);
            await EscribirErrorAsync(context, ex.StatusCode, ex.Codigo, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no controlado");
            await EscribirErrorAsync(context, StatusCodes.Status500InternalServerError,
                "ERROR_INTERNO", "Ocurrió un error inesperado. Intenta de nuevo.");
        }
    }

    private static Task EscribirErrorAsync(HttpContext context, int statusCode, string codigo, string mensaje)
    {
        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsJsonAsync(new { codigo, mensaje });
    }
}