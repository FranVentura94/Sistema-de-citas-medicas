namespace MsClinical.Core.Exceptions;

public class DomainException : Exception
{
    public string Codigo { get; }
    public int StatusCode { get; }

    public DomainException(string codigo, string mensaje, int statusCode = 400, Exception? inner = null)
        : base(mensaje, inner)
    {
        Codigo = codigo;
        StatusCode = statusCode;
    }
}

public static class Errores
{
    public const string DATOS_INVALIDOS = "DATOS_INVALIDOS";
    public const string NO_ENCONTRADO = "NO_ENCONTRADO";
    public const string SIN_PERMISO = "SIN_PERMISO";
    public const string ATENCION_FINALIZADA = "ATENCION_FINALIZADA";
    public const string CITA_NO_VALIDA = "CITA_NO_VALIDA";
    public const string SERVICIO_NO_DISPONIBLE = "SERVICIO_NO_DISPONIBLE";
    public const string ERROR_SERVICIO_EXTERNO = "ERROR_SERVICIO_EXTERNO";
}