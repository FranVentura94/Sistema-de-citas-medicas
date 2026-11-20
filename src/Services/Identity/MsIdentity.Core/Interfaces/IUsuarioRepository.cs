using MsIdentity.Core.Entities;

namespace MsIdentity.Core.Interfaces;

public interface IUsuarioRepository
{
    // Trae el usuario con su Rol y los Permisos del rol
    Task<Usuario?> GetByNombreUsuarioAsync(string nombreUsuario, CancellationToken cancellationToken);

    Task GuardarCambiosAsync(CancellationToken cancellationToken);
}