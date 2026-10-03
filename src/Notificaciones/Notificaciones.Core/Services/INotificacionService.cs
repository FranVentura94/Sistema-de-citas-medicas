using Notificaciones.Core.Models;

namespace Notificaciones.Core.Services;

public interface INotificacionService
{
    Task<NotificacionEnviada> EnviarBienvenidaAsync(string numeroDocumento);
}