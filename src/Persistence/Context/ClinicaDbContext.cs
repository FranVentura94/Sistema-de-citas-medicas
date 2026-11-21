using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using BuildingBlocks.Auditoria;

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
    public DbSet<Empleado> Empleados { get; set; }
    public DbSet<EnfermeraMedico> EnfermeraMedicos { get; set; }
    public DbSet<MedicoEspecialidad> MedicoEspecialidades { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Tabla de auditoría (quién hizo qué y cuándo)
        modelBuilder.AgregarAuditoria();
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

        // ---------- Empleados (personal de la clínica) ----------
        modelBuilder.Entity<Empleado>(e =>
        {
            e.ToTable("Empleados", "dbo");
            e.HasKey(x => x.EmpleadoID);
            e.Property(x => x.Nombres).HasMaxLength(100).IsRequired();
            e.Property(x => x.Apellidos).HasMaxLength(100);
            e.Property(x => x.Identificacion).HasMaxLength(30);
            e.Property(x => x.Genero).HasMaxLength(1);
            e.Property(x => x.Telefono).HasMaxLength(30);
            e.Property(x => x.Email).HasMaxLength(150);
            e.Property(x => x.Direccion).HasMaxLength(250);
            e.Property(x => x.Cargo).HasMaxLength(30).IsRequired();
            e.Property(x => x.Notas).HasMaxLength(500);
            e.Property(x => x.EmergenciaNombre).HasMaxLength(100);
            e.Property(x => x.EmergenciaParentesco).HasMaxLength(50);
            e.Property(x => x.EmergenciaTelefono).HasMaxLength(30);
            e.Property(x => x.FechaRegistro).HasDefaultValueSql("GETDATE()");

            // Un usuario y un médico pertenecen a un solo empleado
            e.HasIndex(x => x.UsuarioID).IsUnique().HasFilter("[UsuarioID] IS NOT NULL");
            e.HasIndex(x => x.MedicoID).IsUnique().HasFilter("[MedicoID] IS NOT NULL");

            e.HasOne(x => x.Medico)
             .WithMany()
             .HasForeignKey(x => x.MedicoID)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- Enfermera_Medico (a quién asiste cada enfermera) ----------
        modelBuilder.Entity<EnfermeraMedico>(e =>
        {
            e.ToTable("EnfermeraMedicos", "dbo");
            e.HasKey(x => new { x.EmpleadoID, x.MedicoID });

            e.HasOne(x => x.Empleado)
             .WithMany(emp => emp.MedicosAsignados)
             .HasForeignKey(x => x.EmpleadoID);

            e.HasOne(x => x.Medico)
             .WithMany()
             .HasForeignKey(x => x.MedicoID)
             .OnDelete(DeleteBehavior.Restrict);
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

        // Fichas del personal que ya existía (enlazadas con sus usuarios de MsIdentity y sus médicos)
        var registro = new DateTime(2026, 11, 20);
        modelBuilder.Entity<Empleado>().HasData(
            new Empleado { EmpleadoID = 1, Nombres = "Administrador", Apellidos = "del Sistema", Cargo = Cargos.Administrativo, Email = "admin@clinica.test", UsuarioID = 1, Activo = true, FechaRegistro = registro },
            new Empleado { EmpleadoID = 2, Nombres = "Laura", Apellidos = "Martínez", Genero = "F", Cargo = Cargos.Recepcion, Email = "recepcion@clinica.test", UsuarioID = 2, Activo = true, FechaRegistro = registro },
            new Empleado { EmpleadoID = 3, Nombres = "Carlos", Apellidos = "Rivas", Genero = "M", Cargo = Cargos.Medico, Identificacion = "04512378-9", Telefono = "7012-3456", Email = "medico@clinica.test", FechaIngreso = new DateTime(2026, 1, 15), MedicoID = 1, UsuarioID = 3, Activo = true, FechaRegistro = registro },
            new Empleado { EmpleadoID = 4, Nombres = "Andrea", Apellidos = "López", Genero = "F", Cargo = Cargos.Medico, Identificacion = "03456789-1", Telefono = "7123-9876", Email = "andrea.lopez@clinica.test", FechaIngreso = new DateTime(2026, 2, 1), MedicoID = 2, Activo = true, FechaRegistro = registro },
            new Empleado { EmpleadoID = 5, Nombres = "Roberto", Apellidos = "Méndez", Genero = "M", Cargo = Cargos.Medico, Identificacion = "02345678-5", Telefono = "7234-5678", Email = "roberto.mendez@clinica.test", FechaIngreso = new DateTime(2026, 3, 10), MedicoID = 3, Activo = true, FechaRegistro = registro },
            new Empleado { EmpleadoID = 6, Nombres = "Ana", Apellidos = "Pérez", Genero = "F", Cargo = Cargos.Enfermeria, Telefono = "7345-1122", FechaIngreso = new DateTime(2026, 4, 1), Activo = true, FechaRegistro = registro });

        // La enfermera Ana Pérez asiste al Dr. Carlos Rivas
        modelBuilder.Entity<EnfermeraMedico>().HasData(new EnfermeraMedico { EmpleadoID = 6, MedicoID = 1 });
    }
}