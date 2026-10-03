using Core.Exceptions;

namespace Api.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
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
                Errores.SERVICIO_NO_DISPONIBLE => StatusCodes.Status503ServiceUnavailable,
                _ => StatusCodes.Status502BadGateway
            };

            await context.Response.WriteAsJsonAsync(new
            {
                codigo = (int)ex.Codigo,
                error = ex.Codigo.ToString(),
                mensaje = ex.Message
            });
        }
    }
}