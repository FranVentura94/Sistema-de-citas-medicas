namespace MsIdentity.Core.Entities;

public class Permiso
{
    public int PermisoId { get; set; }

    // Ej: PACIENTES_VER, CITAS_CREAR, ATENCION_REGISTRAR
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Modulo { get; set; } = string.Empty;

    public ICollection<RolPermiso> RolPermisos { get; set; } = new List<RolPermiso>();
}