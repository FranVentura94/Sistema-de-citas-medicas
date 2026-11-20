using Core.Exceptions;

namespace Api.Middlewares;

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
            context.Response.StatusCode = ex.Codigo switch
            {
                Errores.RECURSO_NO_ENCONTRADO => StatusCodes.Status404NotFound,
                Errores.DATOS_INVALIDOS => StatusCodes.Status400BadRequest,
                Errores.REGISTRO_DUPLICADO => StatusCodes.Status409Conflict,
                Errores.SERVICIO_NO_DISPONIBLE or Errores.PASARELA_NO_DISPONIBLE => StatusCodes.Status503ServiceUnavailable,
                Errores.COBRO_RECHAZADO => StatusCodes.Status422UnprocessableEntity,
                _ => StatusCodes.Status502BadGateway
            };

            await context.Response.WriteAsJsonAsync(new
            {
                codigo = (int)ex.Codigo,
                error = ex.Codigo.ToString(),
                mensaje = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no controlado");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new
            {
                codigo = 500,
                error = "ERROR_INTERNO",
                mensaje = "Ocurrió un error inesperado. Intenta de nuevo."
            });
        }
    }
}