namespace Domain.Entities;

public class Paciente
{
    public long PacienteID { get; set; }

    public string CodigoPaciente { get; set; } = null!;
    public string TipoDocumento { get; set; } = null!;
    public string NumeroDocumento { get; set; } = null!;

    public string Nombres { get; set; } = null!;
    public string Apellidos { get; set; } = null!;

    public DateTime FechaNacimiento { get; set; }

    public string Sexo { get; set; } = null!;
    public string? EstadoCivil { get; set; }

    public string? Telefono { get; set; }
    public string? TelefonoSecundario { get; set; }
    public string? Email { get; set; }

    public string? Direccion { get; set; }
    public string? Ciudad { get; set; }
    public string? Pais { get; set; }
    public string? Ocupacion { get; set; }

    public string? TipoSangre { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaRegistro { get; set; }
}