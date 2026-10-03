namespace Notificaciones.Core.Services;

public interface IEmailSender
{
    Task EnviarAsync(string destinatario, string asunto, string cuerpo);
}