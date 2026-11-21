using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MsIdentity.Core.Interfaces;
using MsIdentity.Persistence.Context;
using MsIdentity.Persistence.Repositories;
using BuildingBlocks.Auditoria;

namespace MsIdentity.Persistence;

public static class Extension
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<MsIdentityDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("IdentityConnection")));

        // Auditoría de todo lo que se hace en este microservicio
        services.AddAuditoria<MsIdentityDbContext>("Identity");

        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IUsuarioAdminRepository, UsuarioAdminRepository>();

        return services;
    }
}