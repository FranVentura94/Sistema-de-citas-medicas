using Microsoft.EntityFrameworkCore;
using MsClinical.Core.Entities;

namespace MsClinical.Persistence.Context;

public class ClinicalDbContext : DbContext
{
    public ClinicalDbContext(DbContextOptions<ClinicalDbContext> options) : base(options)
    {
    }

    public DbSet<ExpedienteClinico> Expedientes => Set<ExpedienteClinico>();
    public DbSet<Atencion> Atenciones => Set<Atencion>();
    public DbSet<SignosVitales> SignosVitales => Set<SignosVitales>();
    public DbSet<NotaMedica> NotasMedicas => Set<NotaMedica>();
    public DbSet<Diagnostico> Diagnosticos => Set<Diagnostico>();
    public DbSet<AtencionDiagnostico> AtencionDiagnosticos => Set<AtencionDiagnostico>();
    public DbSet<Medicamento> Medicamentos => Set<Medicamento>();
    public DbSet<Prescripcion> Prescripciones => Set<Prescripcion>();
    public DbSet<PrescripcionDetalle> PrescripcionDetalles => Set<PrescripcionDetalle>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---------- Expedientes ----------
        modelBuilder.Entity<ExpedienteClinico>(e =>
        {
            e.ToTable("ExpedientesClinicos", "dbo");
            e.HasKey(x => x.ExpedienteClinicoId);
            e.HasIndex(x => x.PacienteId).IsUnique();
            e.Property(x => x.NumeroExpediente).HasMaxLength(20).IsRequired();
            e.HasIndex(x => x.NumeroExpediente).IsUnique();
            e.Property(x => x.PacienteNombre).HasMaxLength(200).IsRequired();
            e.Property(x => x.Alergias).HasMaxLength(500);
            e.Property(x => x.AntecedentesPersonales).HasMaxLength(1000);
            e.Property(x => x.AntecedentesFamiliares).HasMaxLength(1000);
            e.Property(x => x.Observaciones).HasMaxLength(1000);
        });

        // ---------- Atenciones ----------
        modelBuilder.Entity<Atencion>(e =>
        {
            e.ToTable("Atenciones", "dbo");
            e.HasKey(x => x.AtencionId);
            e.HasIndex(x => x.CitaId).IsUnique(); // una atención por cita
            e.HasIndex(x => new { x.MedicoId, x.FechaInicio });
            e.Property(x => x.PacienteNombre).HasMaxLength(200).IsRequired();
            e.Property(x => x.MedicoNombre).HasMaxLength(150).IsRequired();
            e.Property(x => x.EspecialidadNombre).HasMaxLength(100);
            e.Property(x => x.Estado).HasMaxLength(20).IsRequired();
            e.Property(x => x.MotivoConsulta).HasMaxLength(500);
            e.Property(x => x.Observaciones).HasMaxLength(1000);

            e.HasOne(x => x.Expediente)
             .WithMany(x => x.Atenciones)
             .HasForeignKey(x => x.ExpedienteClinicoId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.SignosVitales)
             .WithOne()
             .HasForeignKey<SignosVitales>(x => x.AtencionId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Prescripcion)
             .WithOne()
             .HasForeignKey<Prescripcion>(x => x.AtencionId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(x => x.Notas)
             .WithOne()
             .HasForeignKey(x => x.AtencionId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(x => x.Diagnosticos)
             .WithOne()
             .HasForeignKey(x => x.AtencionId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ---------- Signos vitales ----------
        modelBuilder.Entity<SignosVitales>(e =>
        {
            e.ToTable("SignosVitales", "dbo");
            e.HasKey(x => x.SignosVitalesId);
            e.Property(x => x.Temperatura).HasPrecision(4, 1);
            e.Property(x => x.Peso).HasPrecision(5, 2);
            e.Property(x => x.Talla).HasPrecision(5, 1);
        });

        // ---------- Notas médicas ----------
        modelBuilder.Entity<NotaMedica>(e =>
        {
            e.ToTable("NotasMedicas", "dbo");
            e.HasKey(x => x.NotaMedicaId);
            e.Property(x => x.Tipo).HasMaxLength(20).IsRequired();
            e.Property(x => x.Contenido).HasMaxLength(4000).IsRequired();
            e.Property(x => x.Autor).HasMaxLength(50);
        });

        // ---------- Diagnósticos ----------
        modelBuilder.Entity<Diagnostico>(e =>
        {
            e.ToTable("Diagnosticos", "dbo");
            e.HasKey(x => x.DiagnosticoId);
            e.Property(x => x.Codigo).HasMaxLength(10).IsRequired();
            e.HasIndex(x => x.Codigo).IsUnique();
            e.Property(x => x.Descripcion).HasMaxLength(250).IsRequired();
        });

        modelBuilder.Entity<AtencionDiagnostico>(e =>
        {
            e.ToTable("AtencionDiagnosticos", "dbo");
            e.HasKey(x => new { x.AtencionId, x.DiagnosticoId });
            e.Property(x => x.Tipo).HasMaxLength(20).IsRequired();
            e.Property(x => x.Observacion).HasMaxLength(500);

            e.HasOne(x => x.Diagnostico)
             .WithMany()
             .HasForeignKey(x => x.DiagnosticoId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- Medicamentos y recetas ----------
        modelBuilder.Entity<Medicamento>(e =>
        {
            e.ToTable("Medicamentos", "dbo");
            e.HasKey(x => x.MedicamentoId);
            e.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
            e.Property(x => x.PrincipioActivo).HasMaxLength(100).IsRequired();
            e.Property(x => x.Presentacion).HasMaxLength(50).IsRequired();
            e.Property(x => x.Concentracion).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<Prescripcion>(e =>
        {
            e.ToTable("Prescripciones", "dbo");
            e.HasKey(x => x.PrescripcionId);
            e.Property(x => x.IndicacionesGenerales).HasMaxLength(1000);

            e.HasMany(x => x.Detalles)
             .WithOne()
             .HasForeignKey(x => x.PrescripcionId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PrescripcionDetalle>(e =>
        {
            e.ToTable("PrescripcionDetalles", "dbo");
            e.HasKey(x => x.PrescripcionDetalleId);
            e.Property(x => x.Dosis).HasMaxLength(100).IsRequired();
            e.Property(x => x.Frecuencia).HasMaxLength(100).IsRequired();
            e.Property(x => x.Duracion).HasMaxLength(100).IsRequired();
            e.Property(x => x.Via).HasMaxLength(50).IsRequired();
            e.Property(x => x.Indicaciones).HasMaxLength(500);

            e.HasOne(x => x.Medicamento)
             .WithMany()
             .HasForeignKey(x => x.MedicamentoId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ---------- Catálogo CIE-10 (diagnósticos frecuentes en consulta) ----------
        modelBuilder.Entity<Diagnostico>().HasData(
            new Diagnostico { DiagnosticoId = 1, Codigo = "J00", Descripcion = "Rinofaringitis aguda (resfriado común)" },
            new Diagnostico { DiagnosticoId = 2, Codigo = "J02.9", Descripcion = "Faringitis aguda, no especificada" },
            new Diagnostico { DiagnosticoId = 3, Codigo = "J03.9", Descripcion = "Amigdalitis aguda, no especificada" },
            new Diagnostico { DiagnosticoId = 4, Codigo = "J06.9", Descripcion = "Infección aguda de las vías respiratorias superiores" },
            new Diagnostico { DiagnosticoId = 5, Codigo = "J20.9", Descripcion = "Bronquitis aguda, no especificada" },
            new Diagnostico { DiagnosticoId = 6, Codigo = "A09", Descripcion = "Diarrea y gastroenteritis de presunto origen infeccioso" },
            new Diagnostico { DiagnosticoId = 7, Codigo = "K29.7", Descripcion = "Gastritis, no especificada" },
            new Diagnostico { DiagnosticoId = 8, Codigo = "K21.9", Descripcion = "Enfermedad del reflujo gastroesofágico" },
            new Diagnostico { DiagnosticoId = 9, Codigo = "N39.0", Descripcion = "Infección de vías urinarias, sitio no especificado" },
            new Diagnostico { DiagnosticoId = 10, Codigo = "I10", Descripcion = "Hipertensión esencial (primaria)" },
            new Diagnostico { DiagnosticoId = 11, Codigo = "E11.9", Descripcion = "Diabetes mellitus tipo 2 sin complicaciones" },
            new Diagnostico { DiagnosticoId = 12, Codigo = "E78.5", Descripcion = "Hiperlipidemia, no especificada" },
            new Diagnostico { DiagnosticoId = 13, Codigo = "E66.9", Descripcion = "Obesidad, no especificada" },
            new Diagnostico { DiagnosticoId = 14, Codigo = "R51", Descripcion = "Cefalea" },
            new Diagnostico { DiagnosticoId = 15, Codigo = "G43.9", Descripcion = "Migraña, no especificada" },
            new Diagnostico { DiagnosticoId = 16, Codigo = "M54.5", Descripcion = "Lumbago no especificado" },
            new Diagnostico { DiagnosticoId = 17, Codigo = "L20.9", Descripcion = "Dermatitis atópica, no especificada" },
            new Diagnostico { DiagnosticoId = 18, Codigo = "J45.9", Descripcion = "Asma, no especificada" },
            new Diagnostico { DiagnosticoId = 19, Codigo = "F41.1", Descripcion = "Trastorno de ansiedad generalizada" },
            new Diagnostico { DiagnosticoId = 20, Codigo = "Z00.0", Descripcion = "Examen médico general" });

        // ---------- Catálogo de medicamentos ----------
        modelBuilder.Entity<Medicamento>().HasData(
            new Medicamento { MedicamentoId = 1, Nombre = "Acetaminofén", PrincipioActivo = "Paracetamol", Presentacion = "Tableta", Concentracion = "500 mg" },
            new Medicamento { MedicamentoId = 2, Nombre = "Ibuprofeno", PrincipioActivo = "Ibuprofeno", Presentacion = "Tableta", Concentracion = "400 mg" },
            new Medicamento { MedicamentoId = 3, Nombre = "Amoxicilina", PrincipioActivo = "Amoxicilina", Presentacion = "Cápsula", Concentracion = "500 mg" },
            new Medicamento { MedicamentoId = 4, Nombre = "Amoxicilina suspensión", PrincipioActivo = "Amoxicilina", Presentacion = "Suspensión", Concentracion = "250 mg/5 ml" },
            new Medicamento { MedicamentoId = 5, Nombre = "Azitromicina", PrincipioActivo = "Azitromicina", Presentacion = "Tableta", Concentracion = "500 mg" },
            new Medicamento { MedicamentoId = 6, Nombre = "Loratadina", PrincipioActivo = "Loratadina", Presentacion = "Tableta", Concentracion = "10 mg" },
            new Medicamento { MedicamentoId = 7, Nombre = "Omeprazol", PrincipioActivo = "Omeprazol", Presentacion = "Cápsula", Concentracion = "20 mg" },
            new Medicamento { MedicamentoId = 8, Nombre = "Metformina", PrincipioActivo = "Metformina", Presentacion = "Tableta", Concentracion = "850 mg" },
            new Medicamento { MedicamentoId = 9, Nombre = "Losartán", PrincipioActivo = "Losartán potásico", Presentacion = "Tableta", Concentracion = "50 mg" },
            new Medicamento { MedicamentoId = 10, Nombre = "Enalapril", PrincipioActivo = "Enalapril", Presentacion = "Tableta", Concentracion = "10 mg" },
            new Medicamento { MedicamentoId = 11, Nombre = "Atorvastatina", PrincipioActivo = "Atorvastatina", Presentacion = "Tableta", Concentracion = "20 mg" },
            new Medicamento { MedicamentoId = 12, Nombre = "Salbutamol inhalador", PrincipioActivo = "Salbutamol", Presentacion = "Inhalador", Concentracion = "100 mcg/dosis" },
            new Medicamento { MedicamentoId = 13, Nombre = "Suero oral", PrincipioActivo = "Sales de rehidratación oral", Presentacion = "Sobre", Concentracion = "27.9 g" },
            new Medicamento { MedicamentoId = 14, Nombre = "Nitrofurantoína", PrincipioActivo = "Nitrofurantoína", Presentacion = "Cápsula", Concentracion = "100 mg" },
            new Medicamento { MedicamentoId = 15, Nombre = "Diclofenaco", PrincipioActivo = "Diclofenaco sódico", Presentacion = "Tableta", Concentracion = "50 mg" });
    }
}