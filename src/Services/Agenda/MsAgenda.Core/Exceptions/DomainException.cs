namespace MsAgenda.Core.Exceptions;

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
    public const string FUERA_DE_HORARIO = "FUERA_DE_HORARIO";
    public const string ESPACIO_OCUPADO = "ESPACIO_OCUPADO";
    public const string TRANSICION_INVALIDA = "TRANSICION_INVALIDA";
    public const string SERVICIO_NO_DISPONIBLE = "SERVICIO_NO_DISPONIBLE";
    public const string ERROR_SERVICIO_EXTERNO = "ERROR_SERVICIO_EXTERNO";
    public const string SIN_PERMISO = "SIN_PERMISO";
}