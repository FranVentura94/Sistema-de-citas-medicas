using Notificaciones.Core.Exceptions;
using Notificaciones.Core.Models;

namespace Notificaciones.Core.Services;

public class NotificacionService : INotificacionService
{
    private readonly IPacientesService _pacientesService;
    private readonly IEmailSender _emailSender;

    public NotificacionService(IPacientesService pacientesService, IEmailSender emailSender)
    {
        _pacientesService = pacientesService;
        _emailSender = emailSender;
    }

    public async Task<NotificacionEnviada> EnviarBienvenidaAsync(string numeroDocumento)
    {
        var paciente = await _pacientesService.GetPorDocumentoAsync(numeroDocumento);

        if (string.IsNullOrWhiteSpace(paciente.Email))
        {
            throw new DomainException(Errores.PACIENTE_SIN_EMAIL,
                $"El paciente {paciente.Nombres} {paciente.Apellidos} no tiene un correo registrado.");
        }

        var asunto = "Bienvenido a la clínica";
        var cuerpo = $"Hola {paciente.Nombres} {paciente.Apellidos}, tu registro (documento {paciente.NumeroDocumento}) fue completado. Te damos la bienvenida.";

        await _emailSender.EnviarAsync(paciente.Email, asunto, cuerpo);

        return new NotificacionEnviada(paciente.Email, asunto);
    }
}