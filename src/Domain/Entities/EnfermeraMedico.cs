namespace Domain.Entities;

// Qué enfermera(o) asiste a qué médico
public class EnfermeraMedico
{
    public int EmpleadoID { get; set; }
    public int MedicoID { get; set; }

    public Empleado? Empleado { get; set; }
    public Medico? Medico { get; set; }
}