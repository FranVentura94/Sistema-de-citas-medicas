namespace Core.Exceptions
{
    public enum Errores
    {
        ROL_NO_ENCONTRADO = 601,
        IDENTITY_NO_DISPONIBLE = 701
    }

    /// <summary>Error de negocio con código propio.</summary>
    public class DomainException : Exception
    {
        public Errores Codigo { get; }

        public DomainException(Errores codigo, string message) : base(message)
        {
            Codigo = codigo;
        }
    }
}