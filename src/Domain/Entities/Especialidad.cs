namespace Domain.Entities;

public class Especialidad
{
    public int EspecialidadID { get; set; }
    public string Codigo { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string? Descripcion { get; set; }
    public bool Activo { get; set; } = true;

    public ICollection<MedicoEspecialidad> Medicos { get; set; } = new List<MedicoEspecialidad>();
}