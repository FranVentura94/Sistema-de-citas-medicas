namespace Domain.Entities;

public class MedicoEspecialidad
{
    public int MedicoID { get; set; }
    public int EspecialidadID { get; set; }

    // Indica cuál es la especialidad principal del médico
    public bool Principal { get; set; }

    public Medico? Medico { get; set; }
    public Especialidad? Especialidad { get; set; }
}