using Microsoft.Extensions.Logging;
using Notificaciones.Core.Services;

namespace Notificaciones.Infrastructure.Services;

internal class ConsoleEmailSender : IEmailSender
{
    private readonly ILogger<ConsoleEmailSender> _logger;

    public ConsoleEmailSender(ILogger<ConsoleEmailSender> logger)
    {
        _logger = logger;
    }

    public Task EnviarAsync(string destinatario, string asunto, string cuerpo)
    {
        _logger.LogInformation(
            "CORREO SIMULADO -> Para: {Destinatario} | Asunto: {Asunto} | Cuerpo: {Cuerpo}",
            destinatario, asunto, cuerpo);

        return Task.CompletedTask;
    }
}