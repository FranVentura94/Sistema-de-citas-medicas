using Identity.Entities;
using Microsoft.EntityFrameworkCore;

namespace Identity.Data;

public class IdentityDbContext : DbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
        : base(options)
    {
    }

    public DbSet<Rol> Roles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Rol>().ToTable("Roles", "dbo");

        modelBuilder.Entity<Rol>()
            .HasKey(r => r.RolId);

        modelBuilder.Entity<Rol>()
            .Property(r => r.Nombre)
            .HasMaxLength(50)
            .IsRequired();

        modelBuilder.Entity<Rol>()
            .HasIndex(r => r.Nombre)
            .IsUnique();

        modelBuilder.Entity<Rol>()
            .Property(r => r.Descripcion)
            .HasMaxLength(200);

        modelBuilder.Entity<Rol>()
            .Property(r => r.Activo)
            .HasDefaultValue(true);

        modelBuilder.Entity<Rol>()
            .Property(r => r.FechaCreacion)
            .HasDefaultValueSql("GETDATE()");
    }
}