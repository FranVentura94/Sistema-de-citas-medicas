using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Context;

public class ClinicaDbContext : DbContext
{
    public ClinicaDbContext(DbContextOptions<ClinicaDbContext> options) : base(options)
    {
    }

    // Mapeo de Tablas
    public DbSet<Paciente> Pacientes { get; set; }
    public DbSet<Medico> Medicos { get; set; }
    public DbSet<Citas> Citas { get; set; }
    public DbSet<Atenciones> Atenciones { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Mapeo explícito a tablas de SQL Server
        modelBuilder.Entity<Paciente>().ToTable("Pacientes", "dbo");
        modelBuilder.Entity<Medico>().ToTable("Medicos", "dbo");
        modelBuilder.Entity<Citas>().ToTable("Citas", "dbo");
        modelBuilder.Entity<Atenciones>().ToTable("Atenciones", "dbo");

        // Claves primarias
        modelBuilder.Entity<Citas>()
            .HasKey(c => c.CitaID);

        modelBuilder.Entity<Atenciones>()
            .HasKey(a => a.AtencionID);
    }
}