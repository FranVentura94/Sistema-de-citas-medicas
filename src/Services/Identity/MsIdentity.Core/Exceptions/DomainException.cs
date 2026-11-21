namespace MsIdentity.Core.Exceptions;

public class DomainException : Exception
{
    public string Codigo { get; }
    public int StatusCode { get; }

    public DomainException(string codigo, string mensaje, int statusCode = 400)
        : base(mensaje)
    {
        Codigo = codigo;
        StatusCode = statusCode;
    }
}

public static class Errores
{
    public const string DATOS_INCOMPLETOS = "DATOS_INCOMPLETOS";
    public const string CREDENCIALES_INVALIDAS = "CREDENCIALES_INVALIDAS";
    public const string USUARIO_INACTIVO = "USUARIO_INACTIVO";
    public const string TEMPORAL_VENCIDA = "TEMPORAL_VENCIDA";
}