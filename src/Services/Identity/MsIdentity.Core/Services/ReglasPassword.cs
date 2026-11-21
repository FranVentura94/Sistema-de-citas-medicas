using System.Security.Cryptography;
using MsIdentity.Core.Exceptions;

namespace MsIdentity.Core.Services;

// Reglas de contraseñas: validar las que crea la persona y generar las temporales
public static class ReglasPassword
{
    public const int HORAS_VIGENCIA_TEMPORAL = 72;

    // Sin letras ni números que se confunden al dictarlos (O/0, l/1/I)
    private const string MAYUSCULAS = "ABCDEFGHJKLMNPQRSTUVWXYZ";
    private const string MINUSCULAS = "abcdefghijkmnpqrstuvwxyz";
    private const string NUMEROS = "23456789";

    // Mínimo 8 caracteres, con mayúscula, minúscula y número
    public static void Validar(string? password)
    {
        if (string.IsNullOrEmpty(password) || password.Length < 8 ||
            !password.Any(char.IsUpper) || !password.Any(char.IsLower) || !password.Any(char.IsDigit))
            throw new DomainException(Errores.DATOS_INCOMPLETOS,
                "La contraseña debe tener al menos 8 caracteres, con mayúscula, minúscula y número.");
    }

    // Ejemplo: "Kp7m-Qx4z-R9tW" (fácil de dictar, 12 caracteres aleatorios)
    public static string GenerarTemporal()
    {
        const string todos = MAYUSCULAS + MINUSCULAS + NUMEROS;
        while (true)
        {
            var c = Enumerable.Range(0, 12).Select(_ => todos[RandomNumberGenerator.GetInt32(todos.Length)]).ToArray();
            var texto = new string(c);

            if (texto.Any(char.IsUpper) && texto.Any(char.IsLower) && texto.Any(char.IsDigit))
                return $"{texto[..4]}-{texto[4..8]}-{texto[8..]}";
        }
    }
}