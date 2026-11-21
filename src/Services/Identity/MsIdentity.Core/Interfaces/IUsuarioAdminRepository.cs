using MsIdentity.Core.Entities;

namespace MsIdentity.Core.Interfaces;

// Operaciones de administración de usuarios (acceso al portal)
public interface IUsuarioAdminRepository
{
    Task<List<Usuario>> GetAllAsync(CancellationToken ct);
    Task<Usuario?> GetByIdAsync(int usuarioId, CancellationToken ct);
    Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario, CancellationToken ct);
    Task<bool> MedicoYaTieneUsuarioAsync(int medicoId, int? excluirUsuarioId, CancellationToken ct);
    Task<int> ContarAdministradoresActivosAsync(CancellationToken ct);
    Task AddAsync(Usuario usuario, CancellationToken ct);

    Task<List<Rol>> GetRolesAsync(CancellationToken ct);
    Task<Rol?> GetRolAsync(int rolId, CancellationToken ct);

    Task GuardarCambiosAsync(CancellationToken ct);
}