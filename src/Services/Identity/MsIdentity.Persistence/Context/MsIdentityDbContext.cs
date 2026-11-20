using Microsoft.EntityFrameworkCore;
using MsIdentity.Core.Entities;

namespace MsIdentity.Persistence.Context;

public class MsIdentityDbContext : DbContext
{
    public MsIdentityDbContext(DbContextOptions<MsIdentityDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Permiso> Permisos => Set<Permiso>();
    public DbSet<RolPermiso> RolPermisos => Set<RolPermiso>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---------- Usuarios ----------
        modelBuilder.Entity<Usuario>(e =>
        {
            e.ToTable("Usuarios", "dbo");
            e.HasKey(u => u.UsuarioId);

            e.Property(u => u.NombreUsuario).HasMaxLength(50).IsRequired();
            e.HasIndex(u => u.NombreUsuario).IsUnique();

            e.Property(u => u.NombreCompleto).HasMaxLength(150).IsRequired();
            e.Property(u => u.Email).HasMaxLength(150).IsRequired();
            e.Property(u => u.PasswordHash).HasMaxLength(200).IsRequired();
            e.Property(u => u.FechaCreacion).HasDefaultValueSql("GETDATE()");

            e.HasOne(u => u.Rol)
             .WithMany(r => r.Usuarios)
             .HasForeignKey(u => u.RolId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- Roles ----------
        modelBuilder.Entity<Rol>(e =>
        {
            e.ToTable("Roles", "dbo");
            e.HasKey(r => r.RolId);

            e.Property(r => r.Nombre).HasMaxLength(50).IsRequired();
            e.HasIndex(r => r.Nombre).IsUnique();

            e.Property(r => r.Descripcion).HasMaxLength(200);
            e.Property(r => r.FechaCreacion).HasDefaultValueSql("GETDATE()");
        });

        // ---------- Permisos ----------
        modelBuilder.Entity<Permiso>(e =>
        {
            e.ToTable("Permisos", "dbo");
            e.HasKey(p => p.PermisoId);

            e.Property(p => p.Codigo).HasMaxLength(60).IsRequired();
            e.HasIndex(p => p.Codigo).IsUnique();

            e.Property(p => p.Nombre).HasMaxLength(100).IsRequired();
            e.Property(p => p.Modulo).HasMaxLength(50).IsRequired();
        });

        // ---------- Rol_Permiso (tabla intermedia) ----------
        modelBuilder.Entity<RolPermiso>(e =>
        {
            e.ToTable("RolPermisos", "dbo");
            e.HasKey(rp => new { rp.RolId, rp.PermisoId });

            e.HasOne(rp => rp.Rol)
             .WithMany(r => r.RolPermisos)
             .HasForeignKey(rp => rp.RolId);

            e.HasOne(rp => rp.Permiso)
             .WithMany(p => p.RolPermisos)
             .HasForeignKey(rp => rp.PermisoId);
        });
    }
}