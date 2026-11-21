using Microsoft.EntityFrameworkCore;
using MsIdentity.Core.Entities;
using MsIdentity.Core.Interfaces;
using MsIdentity.Persistence.Context;

namespace MsIdentity.Persistence.Repositories;

public class UsuarioAdminRepository : IUsuarioAdminRepository
{
    private readonly MsIdentityDbContext _db;

    public UsuarioAdminRepository(MsIdentityDbContext db)
    {
        _db = db;
    }

    public Task<List<Usuario>> GetAllAsync(CancellationToken ct) =>
        _db.Usuarios.AsNoTracking()
            .Include(u => u.Rol)
            .OrderBy(u => u.NombreCompleto)
            .ToListAsync(ct);

    public Task<Usuario?> GetByIdAsync(int usuarioId, CancellationToken ct) =>
        _db.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.UsuarioId == usuarioId, ct);

    public Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario, CancellationToken ct) =>
        _db.Usuarios.AnyAsync(u => u.NombreUsuario == nombreUsuario, ct);

    public Task<bool> MedicoYaTieneUsuarioAsync(int medicoId, int? excluirUsuarioId, CancellationToken ct) =>
        _db.Usuarios.AnyAsync(u => u.MedicoId == medicoId && (excluirUsuarioId == null || u.UsuarioId != excluirUsuarioId), ct);

    public Task<int> ContarAdministradoresActivosAsync(CancellationToken ct) =>
        _db.Usuarios.CountAsync(u => u.Activo && u.Rol!.Nombre == "Administrador", ct);

    public async Task AddAsync(Usuario usuario, CancellationToken ct)
    {
        await _db.Usuarios.AddAsync(usuario, ct);
        await _db.SaveChangesAsync(ct);
    }

    public Task<List<Rol>> GetRolesAsync(CancellationToken ct) =>
        _db.Roles.AsNoTracking().Where(r => r.Activo).OrderBy(r => r.Nombre).ToListAsync(ct);

    public Task<Rol?> GetRolAsync(int rolId, CancellationToken ct) =>
        _db.Roles.FirstOrDefaultAsync(r => r.RolId == rolId && r.Activo, ct);

    public Task GuardarCambiosAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}