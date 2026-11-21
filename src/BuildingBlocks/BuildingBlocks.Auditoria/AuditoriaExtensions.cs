using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Auditoria;

public static class AuditoriaExtensions
{
    // ---------- 1) En Program/Extension: services.AddAuditoria<MiDbContext>("Identity"); ----------
    public static IServiceCollection AddAuditoria<TContext>(this IServiceCollection services, string servicio)
        where TContext : DbContext
    {
        services.AddHttpContextAccessor();
        services.AddSingleton(new AuditoriaOpciones { Servicio = servicio });
        services.AddScoped<ContextoAuditoria>();
        services.AddScoped<AuditoriaInterceptor>();
        services.AddScoped<IAuditoria, AuditoriaService<TContext>>();
        services.AddScoped<IAuditoriaLector, AuditoriaLector<TContext>>();
        services.AddHostedService<AuditoriaSoloLectura<TContext>>();

        // Engancha el interceptor al DbContext del servicio (todo SaveChanges queda auditado)
        services.ConfigureDbContext<TContext>((sp, options) =>
            options.AddInterceptors(sp.GetRequiredService<AuditoriaInterceptor>()));

        // Publica GET api/Auditoria de esta librería
        var ensamblado = typeof(AuditoriaController).Assembly;
        services.AddControllers().ConfigureApplicationPartManager(m =>
        {
            if (!m.ApplicationParts.OfType<AssemblyPart>().Any(p => p.Assembly == ensamblado))
                m.ApplicationParts.Add(new AssemblyPart(ensamblado));
        });

        return services;
    }

    // ---------- 2) En OnModelCreating: modelBuilder.AgregarAuditoria(); ----------
    public static ModelBuilder AgregarAuditoria(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RegistroAuditoria>(e =>
        {
            e.ToTable("Auditoria", "dbo");
            e.HasKey(x => x.Id);
            e.Property(x => x.Servicio).HasMaxLength(30).IsRequired();
            e.Property(x => x.Categoria).HasMaxLength(20).IsRequired();
            e.Property(x => x.Accion).HasMaxLength(40).IsRequired();
            e.Property(x => x.Descripcion).HasMaxLength(500).IsRequired();
            e.Property(x => x.Entidad).HasMaxLength(80);
            e.Property(x => x.EntidadId).HasMaxLength(60);
            e.Property(x => x.UsuarioLogin).HasMaxLength(50);
            e.Property(x => x.UsuarioNombre).HasMaxLength(150);
            e.Property(x => x.Rol).HasMaxLength(50);
            e.Property(x => x.Ip).HasMaxLength(60);
            e.Property(x => x.Dispositivo).HasMaxLength(100);
            e.Property(x => x.Ruta).HasMaxLength(250);

            e.HasIndex(x => x.Fecha);
            e.HasIndex(x => new { x.Entidad, x.EntidadId });
            e.HasIndex(x => x.UsuarioId);
        });
        return modelBuilder;
    }

    // ---------- 3) En el pipeline, ANTES de UseAuthentication: app.UseAuditoria(); ----------
    public static IApplicationBuilder UseAuditoria(this IApplicationBuilder app) =>
        app.UseMiddleware<AuditoriaAccesosMiddleware>();
}

// Al arrancar, protege la tabla Auditoria: nadie puede modificarla ni borrarla (ni desde SQL Server)
public class AuditoriaSoloLectura<TContext> : IHostedService where TContext : DbContext
{
    private readonly IServiceScopeFactory _scopes;
    private readonly ILogger<AuditoriaSoloLectura<TContext>> _logger;

    public AuditoriaSoloLectura(IServiceScopeFactory scopes, ILogger<AuditoriaSoloLectura<TContext>> logger)
    {
        _scopes = scopes;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        const string sql = """
            IF OBJECT_ID(N'dbo.Auditoria', N'U') IS NOT NULL AND OBJECT_ID(N'dbo.trg_Auditoria_SoloLectura', N'TR') IS NULL
            EXEC(N'CREATE TRIGGER dbo.trg_Auditoria_SoloLectura ON dbo.Auditoria INSTEAD OF UPDATE, DELETE AS
                   BEGIN
                       THROW 51000, N''La auditoría es de solo lectura: no se puede modificar ni borrar.'', 1;
                   END');
            """;
        try
        {
            using var scope = _scopes.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TContext>();
            await db.Database.ExecuteSqlRawAsync(sql, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo proteger la tabla de auditoría.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}