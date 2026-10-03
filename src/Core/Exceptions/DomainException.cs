namespace Core.Exceptions;

public enum Errores
{
    SERVICIO_NO_DISPONIBLE = 701,
    RECURSO_NO_ENCONTRADO = 702,
    ERROR_SERVICIO_EXTERNO = 703,
    PASARELA_NO_DISPONIBLE = 704,
    AUTENTICACION_FALLIDA = 705,
    COBRO_RECHAZADO = 706
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