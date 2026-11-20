using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Context;

// DbContext del microservicio Directory (base MsDirectoryDB)
public class ClinicaDbContext : DbContext
{
    public ClinicaDbContext(DbContextOptions<ClinicaDbContext> options) : base(options)
    {
    }

    public DbSet<Paciente> Pacientes { get; set; }
    public DbSet<Medico> Medicos { get; set; }
    public DbSet<Especialidad> Especialidades { get; set; }
    public DbSet<MedicoEspecialidad> MedicoEspecialidades { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Paciente>().ToTable("Pacientes", "dbo");
        modelBuilder.Entity<Medico>().ToTable("Medicos", "dbo");

        // ---------- Especialidades ----------
        modelBuilder.Entity<Especialidad>(e =>
        {
            e.ToTable("Especialidades", "dbo");
            e.HasKey(x => x.EspecialidadID);
            e.Property(x => x.Codigo).HasMaxLength(20).IsRequired();
            e.HasIndex(x => x.Codigo).IsUnique();
            e.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
            e.Property(x => x.Descripcion).HasMaxLength(250);
        });

        // ---------- Medico_Especialidad (muchos a muchos) ----------
        modelBuilder.Entity<MedicoEspecialidad>(e =>
        {
            e.ToTable("MedicoEspecialidades", "dbo");
            e.HasKey(x => new { x.MedicoID, x.EspecialidadID });

            e.HasOne(x => x.Medico)
             .WithMany(m => m.Especialidades)
             .HasForeignKey(x => x.MedicoID);

            e.HasOne(x => x.Especialidad)
             .WithMany(es => es.Medicos)
             .HasForeignKey(x => x.EspecialidadID);
        });

        // ---------- Datos iniciales ----------
        modelBuilder.Entity<Especialidad>().HasData(
            new Especialidad { EspecialidadID = 1, Codigo = "MG", Nombre = "Medicina General", Descripcion = "Atención primaria y consulta general" },
            new Especialidad { EspecialidadID = 2, Codigo = "PED", Nombre = "Pediatría", Descripcion = "Atención de niños y adolescentes" },
            new Especialidad { EspecialidadID = 3, Codigo = "CAR", Nombre = "Cardiología", Descripcion = "Enfermedades del corazón" },
            new Especialidad { EspecialidadID = 4, Codigo = "DER", Nombre = "Dermatología", Descripcion = "Enfermedades de la piel" },
            new Especialidad { EspecialidadID = 5, Codigo = "GIN", Nombre = "Ginecología", Descripcion = "Salud de la mujer" },
            new Especialidad { EspecialidadID = 6, Codigo = "TRA", Nombre = "Traumatología", Descripcion = "Lesiones de huesos y articulaciones" });

        // El médico 1 está enlazado con el usuario "medico" (UsuarioId = 3) de MsIdentity
        modelBuilder.Entity<Medico>().HasData(
            new Medico { MedicoID = 1, UsuarioID = 3, Nombres = "Carlos", Apellidos = "Rivas", NumeroColegiatura = "JVPM-10234", Identification = "04512378-9", Telefono = "7012-3456", Email = "medico@clinica.test", FechaIngreso = new DateTime(2026, 1, 15), Activo = true },
            new Medico { MedicoID = 2, Nombres = "Andrea", Apellidos = "López", NumeroColegiatura = "JVPM-11872", Identification = "03456789-1", Telefono = "7123-9876", Email = "andrea.lopez@clinica.test", FechaIngreso = new DateTime(2026, 2, 1), Activo = true },
            new Medico { MedicoID = 3, Nombres = "Roberto", Apellidos = "Méndez", NumeroColegiatura = "JVPM-09561", Identification = "02345678-5", Telefono = "7234-5678", Email = "roberto.mendez@clinica.test", FechaIngreso = new DateTime(2026, 3, 10), Activo = true });

        modelBuilder.Entity<MedicoEspecialidad>().HasData(
            new MedicoEspecialidad { MedicoID = 1, EspecialidadID = 1, Principal = true },
            new MedicoEspecialidad { MedicoID = 2, EspecialidadID = 2, Principal = true },
            new MedicoEspecialidad { MedicoID = 3, EspecialidadID = 3, Principal = true },
            new MedicoEspecialidad { MedicoID = 3, EspecialidadID = 1, Principal = false });
    }
}