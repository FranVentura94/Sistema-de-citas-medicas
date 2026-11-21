using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MsClinical.Core.Interfaces;
using MsClinical.Persistence.Context;
using MsClinical.Persistence.Repositories;
using BuildingBlocks.Auditoria;

namespace MsClinical.Persistence;

public static class Extension
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ClinicalDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("ClinicalConnection")));

        // Auditoría de todo lo que se hace en este microservicio
        services.AddAuditoria<ClinicalDbContext>("Clinical");

        services.AddScoped<IClinicalRepository, ClinicalRepository>();

        return services;
    }
}