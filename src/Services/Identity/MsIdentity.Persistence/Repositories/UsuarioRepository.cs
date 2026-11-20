using Microsoft.EntityFrameworkCore;
using MsIdentity.Core.Entities;
using MsIdentity.Core.Interfaces;
using MsIdentity.Persistence.Context;

namespace MsIdentity.Persistence.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly MsIdentityDbContext _db;

    public UsuarioRepository(MsIdentityDbContext db)
    {
        _db = db;
    }

    public async Task<Usuario?> GetByNombreUsuarioAsync(string nombreUsuario, CancellationToken cancellationToken)
    {
        return await _db.Usuarios
            .Include(u => u.Rol!)
                .ThenInclude(r => r.RolPermisos)
                    .ThenInclude(rp => rp.Permiso)
            .FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario, cancellationToken);
    }

    public Task GuardarCambiosAsync(CancellationToken cancellationToken)
    {
        return _db.SaveChangesAsync(cancellationToken);
    }
}