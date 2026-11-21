using MsAgenda.Core.Dtos;
using MsAgenda.Core.Entities;
using MsAgenda.Core.Exceptions;
using MsAgenda.Core.Interfaces;

namespace MsAgenda.Core.Services;

// Qué citas puede ver/gestionar cada persona:
//   - Administrador y Recepción: todas
//   - Médico: las suyas + las de agendas que le compartieron (toda la agenda o pacientes específicos)
//   - Enfermería: las de los médicos a los que asiste
public class PermisoVista
{
    public bool Todo { get; init; }
    public int? MedicoPropio { get; init; }
    public HashSet<int> MedicosCompletos { get; } = new();
    public Dictionary<int, HashSet<long>> PacientesPorMedico { get; } = new();

    public bool PuedeVer(int medicoId, long pacienteId) =>
        Todo
        || MedicosCompletos.Contains(medicoId)
        || (PacientesPorMedico.TryGetValue(medicoId, out var pacientes) && pacientes.Contains(pacienteId));

    public bool EsPropia(int medicoId) => MedicoPropio == medicoId;
}

public interface IPermisosAgenda
{
    Task<PermisoVista> ObtenerAsync(UsuarioAgenda usuario, CancellationToken ct);
}

public class PermisosAgenda : IPermisosAgenda
{
    private readonly IAgendaRepository _repo;
    private readonly IDirectoryService _directory;
    private PermisoVista? _cache;

    public PermisosAgenda(IAgendaRepository repo, IDirectoryService directory)
    {
        _repo = repo;
        _directory = directory;
    }

    public async Task<PermisoVista> ObtenerAsync(UsuarioAgenda u, CancellationToken ct)
    {
        if (_cache is not null) return _cache;

        if (u.EsAdmin || u.EsRecepcion)
            return _cache = new PermisoVista { Todo = true };

        if (u.EsMedico)
        {
            if (u.MedicoId is null)
                throw new DomainException(Errores.SIN_PERMISO, "Tu usuario no está enlazado a un médico.", 403);

            var permiso = new PermisoVista { MedicoPropio = u.MedicoId };
            permiso.MedicosCompletos.Add(u.MedicoId.Value);

            var ahora = DateTime.Now;
            foreach (var acceso in (await _repo.GetAccesosComoAutorizadoAsync(u.MedicoId.Value, ct)).Where(a => a.Vigente(ahora)))
            {
                if (acceso.Alcance == AlcancesAcceso.TODA)
                {
                    permiso.MedicosCompletos.Add(acceso.MedicoPropietarioId);
                }
                else
                {
                    if (!permiso.PacientesPorMedico.TryGetValue(acceso.MedicoPropietarioId, out var set))
                        permiso.PacientesPorMedico[acceso.MedicoPropietarioId] = set = new HashSet<long>();
                    foreach (var p in acceso.Pacientes) set.Add(p.PacienteId);
                }
            }
            return _cache = permiso;
        }

        if (u.EsEnfermeria)
        {
            var permiso = new PermisoVista();
            var perfil = await _directory.GetMiPerfilAsync(ct);
            foreach (var m in perfil?.MedicosAsignados ?? new List<MedicoAsignadoRemotoDto>())
                permiso.MedicosCompletos.Add(m.MedicoID);
            return _cache = permiso;
        }

        // Cualquier otro nivel: nada
        return _cache = new PermisoVista();
    }
}
