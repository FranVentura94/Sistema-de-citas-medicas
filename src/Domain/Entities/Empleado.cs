namespace Domain.Entities;

// Ficha de cualquier persona que trabaja en la clínica (tenga o no acceso al portal)
public class Empleado
{
    public int EmpleadoID { get; set; }

    // Datos personales (solo el nombre es obligatorio)
    public string Nombres { get; set; } = null!;
    public string? Apellidos { get; set; }
    public string? Identificacion { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    public string? Genero { get; set; }            // "F", "M" o null

    // Contacto
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }

    // Trabajo
    public string Cargo { get; set; } = Cargos.Otro;
    public DateTime? FechaIngreso { get; set; }
    public string? Notas { get; set; }

    // Contacto de emergencia
    public string? EmergenciaNombre { get; set; }
    public string? EmergenciaParentesco { get; set; }
    public string? EmergenciaTelefono { get; set; }

    // Enlaces: con su ficha de médico (si es médico) y con su usuario de MsIdentity (si tiene acceso)
    public int? MedicoID { get; set; }
    public int? UsuarioID { get; set; }

    public bool Activo { get; set; } = true;
    public DateTime FechaRegistro { get; set; } = DateTime.Now;

    public Medico? Medico { get; set; }

    // Si es enfermera(o): médicos a los que asiste
    public ICollection<EnfermeraMedico> MedicosAsignados { get; set; } = new List<EnfermeraMedico>();
}

public static class Cargos
{
    public const string Medico = "Médico";
    public const string Enfermeria = "Enfermería";
    public const string Recepcion = "Recepción";
    public const string Administrativo = "Administrativo";
    public const string Otro = "Otro";

    public static readonly string[] Todos = { Medico, Enfermeria, Recepcion, Administrativo, Otro };
}