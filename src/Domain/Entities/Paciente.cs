namespace Domain.Entities;

public class Paciente
{
    // Clave primaria
    public long PacienteID { get; set; }

    // Identificación y Nombres
    public string NumeroIdentificacion { get; set; } = null!;
    public string TipoIdentificacion { get; set; } = null!;
    public string Nombres { get; set; } = null!;
    public string Apellidos { get; set; } = null!;

    // Información Personal
    public DateTime FechaNacimiento { get; set; }
    public string Genero { get; set; } = null!;
    public string? EstadoCivil { get; set; }
    public string? GrupoSanguineo { get; set; }

    // Contacto y Ubicación
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    public string? Ciudad { get; set; }
    public string? EstadoProvincia { get; set; }
    public string? Pais { get; set; }

    // Contacto de Emergencia
    public string? ContactoEmergenciaNombre { get; set; }
    public string? ContactoEmergenciaTelefono { get; set; }

    // Control y Auditoría
    public DateTime FechaRegistro { get; set; } = DateTime.Now;
    public bool Activo { get; set; } = true;
}