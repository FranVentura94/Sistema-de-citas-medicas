namespace Core.Exceptions;

public enum Errores
{
    SERVICIO_NO_DISPONIBLE = 701,
    RECURSO_NO_ENCONTRADO = 702,
    ERROR_SERVICIO_EXTERNO = 703
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