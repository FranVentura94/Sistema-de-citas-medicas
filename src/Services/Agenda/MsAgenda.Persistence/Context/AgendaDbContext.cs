using Microsoft.EntityFrameworkCore;
using MsAgenda.Core.Entities;

namespace MsAgenda.Persistence.Context;

public class AgendaDbContext : DbContext
{
    public AgendaDbContext(DbContextOptions<AgendaDbContext> options) : base(options)
    {
    }

    public DbSet<Consultorio> Consultorios => Set<Consultorio>();
    public DbSet<HorarioMedico> HorariosMedicos => Set<HorarioMedico>();
    public DbSet<EstadoCita> Estados => Set<EstadoCita>();
    public DbSet<Cita> Citas => Set<Cita>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---------- Consultorios ----------
        modelBuilder.Entity<Consultorio>(e =>
        {
            e.ToTable("Consultorios", "dbo");
            e.HasKey(x => x.ConsultorioId);
            e.Property(x => x.Codigo).HasMaxLength(20).IsRequired();
            e.HasIndex(x => x.Codigo).IsUnique();
            e.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
            e.Property(x => x.Ubicacion).HasMaxLength(150);
        });

        // ---------- Horarios de médicos ----------
        modelBuilder.Entity<HorarioMedico>(e =>
        {
            e.ToTable("HorariosMedicos", "dbo");
            e.HasKey(x => x.HorarioMedicoId);
            e.Property(x => x.MedicoNombre).HasMaxLength(150).IsRequired();
            e.HasIndex(x => new { x.MedicoId, x.DiaSemana });

            e.HasOne(x => x.Consultorio)
             .WithMany()
             .HasForeignKey(x => x.ConsultorioId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- Estados de cita ----------
        modelBuilder.Entity<EstadoCita>(e =>
        {
            e.ToTable("EstadosCita", "dbo");
            e.HasKey(x => x.EstadoCitaId);
            e.Property(x => x.EstadoCitaId).ValueGeneratedNever();
            e.Property(x => x.Codigo).HasMaxLength(30).IsRequired();
            e.HasIndex(x => x.Codigo).IsUnique();
            e.Property(x => x.Nombre).HasMaxLength(50).IsRequired();
            e.Property(x => x.Color).HasMaxLength(10).IsRequired();
        });

        // ---------- Citas ----------
        modelBuilder.Entity<Cita>(e =>
        {
            e.ToTable("Citas", "dbo");
            e.HasKey(x => x.CitaId);
            e.Property(x => x.PacienteNombre).HasMaxLength(200).IsRequired();
            e.Property(x => x.PacienteDocumento).HasMaxLength(30).IsRequired();
            e.Property(x => x.MedicoNombre).HasMaxLength(150).IsRequired();
            e.Property(x => x.EspecialidadNombre).HasMaxLength(100);
            e.Property(x => x.MotivoConsulta).HasMaxLength(500);
            e.Property(x => x.Notas).HasMaxLength(500);
            e.Property(x => x.UsuarioRegistro).HasMaxLength(50);

            e.HasIndex(x => new { x.MedicoId, x.FechaHoraInicio });
            e.HasIndex(x => x.PacienteId);

            e.HasOne(x => x.Estado)
             .WithMany()
             .HasForeignKey(x => x.EstadoCitaId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Consultorio)
             .WithMany()
             .HasForeignKey(x => x.ConsultorioId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- Datos iniciales ----------
        modelBuilder.Entity<EstadoCita>().HasData(
            new EstadoCita { EstadoCitaId = EstadosCita.PROGRAMADA, Codigo = "PROGRAMADA", Nombre = "Programada", Color = "#3B82F6", Orden = 1 },
            new EstadoCita { EstadoCitaId = EstadosCita.CONFIRMADA, Codigo = "CONFIRMADA", Nombre = "Confirmada", Color = "#6366F1", Orden = 2 },
            new EstadoCita { EstadoCitaId = EstadosCita.EN_ESPERA, Codigo = "EN_ESPERA", Nombre = "En espera", Color = "#F59E0B", Orden = 3 },
            new EstadoCita { EstadoCitaId = EstadosCita.EN_ATENCION, Codigo = "EN_ATENCION", Nombre = "En atención", Color = "#8B5CF6", Orden = 4 },
            new EstadoCita { EstadoCitaId = EstadosCita.ATENDIDA, Codigo = "ATENDIDA", Nombre = "Atendida", Color = "#10B981", Orden = 5 },
            new EstadoCita { EstadoCitaId = EstadosCita.CANCELADA, Codigo = "CANCELADA", Nombre = "Cancelada", Color = "#EF4444", Orden = 6 },
            new EstadoCita { EstadoCitaId = EstadosCita.NO_ASISTIO, Codigo = "NO_ASISTIO", Nombre = "No asistió", Color = "#6B7280", Orden = 7 });

        modelBuilder.Entity<Consultorio>().HasData(
            new Consultorio { ConsultorioId = 1, Codigo = "C-101", Nombre = "Consultorio 101", Ubicacion = "Primer nivel" },
            new Consultorio { ConsultorioId = 2, Codigo = "C-102", Nombre = "Consultorio 102", Ubicacion = "Primer nivel" },
            new Consultorio { ConsultorioId = 3, Codigo = "C-201", Nombre = "Consultorio 201", Ubicacion = "Segundo nivel" });

        // Horarios de los 3 médicos sembrados en MS_Directory
        // (0 = domingo, 1 = lunes ... 6 = sábado)
        var horarios = new List<HorarioMedico>();
        var id = 1;

        // Dr. Carlos Rivas (Medicina General): lunes a viernes 08-12 y 14-17, sábado 08-12
        for (var dia = 1; dia <= 5; dia++)
        {
            horarios.Add(Horario(id++, 1, "Dr(a). Carlos Rivas", 1, dia, 8, 12));
            horarios.Add(Horario(id++, 1, "Dr(a). Carlos Rivas", 1, dia, 14, 17));
        }
        horarios.Add(Horario(id++, 1, "Dr(a). Carlos Rivas", 1, 6, 8, 12));

        // Dra. Andrea López (Pediatría): lunes, miércoles y viernes 08-13
        foreach (var dia in new[] { 1, 3, 5 })
            horarios.Add(Horario(id++, 2, "Dr(a). Andrea López", 2, dia, 8, 13));

        // Dr. Roberto Méndez (Cardiología): martes y jueves 09-15
        foreach (var dia in new[] { 2, 4 })
            horarios.Add(Horario(id++, 3, "Dr(a). Roberto Méndez", 3, dia, 9, 15));

        modelBuilder.Entity<HorarioMedico>().HasData(horarios);
    }

    private static HorarioMedico Horario(int id, int medicoId, string medico, int consultorioId, int dia, int desde, int hasta) => new()
    {
        HorarioMedicoId = id,
        MedicoId = medicoId,
        MedicoNombre = medico,
        ConsultorioId = consultorioId,
        DiaSemana = dia,
        HoraInicio = new TimeOnly(desde, 0),
        HoraFin = new TimeOnly(hasta, 0),
        DuracionCitaMinutos = 30,
        Activo = true
    };
}