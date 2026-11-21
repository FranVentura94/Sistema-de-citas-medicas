using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MsAgenda.Core.Interfaces;
using MsAgenda.Persistence.Context;
using MsAgenda.Persistence.Repositories;
using BuildingBlocks.Auditoria;

namespace MsAgenda.Persistence;

public static class Extension
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AgendaDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("AgendaConnection")));

        // Auditoría de todo lo que se hace en este microservicio
        services.AddAuditoria<AgendaDbContext>("Agenda");

        services.AddScoped<IAgendaRepository, AgendaRepository>();

        return services;
    }
}