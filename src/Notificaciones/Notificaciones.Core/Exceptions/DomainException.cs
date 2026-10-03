namespace Notificaciones.Core.Exceptions;

public enum Errores
{
    PACIENTE_NO_ENCONTRADO = 801,
    SERVICIO_NO_DISPONIBLE = 802,
    ERROR_SERVICIO_EXTERNO = 803,
    PACIENTE_SIN_EMAIL = 804
}

public class DomainException : Exception
{
    public Errores Codigo { get; }

    public DomainException(Errores codigo, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        Codigo = codigo;
    }
}