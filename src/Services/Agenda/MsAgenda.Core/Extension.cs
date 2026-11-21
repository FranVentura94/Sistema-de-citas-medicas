using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MsAgenda.Core.Configs;
using MsAgenda.Core.Services;

namespace MsAgenda.Core;

public static class Extension
{
    public static IServiceCollection AddCore(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DownstreamOptions>(configuration.GetSection("Downstream"));

        services.AddScoped<IHorarioService, HorarioService>();
        services.AddScoped<ICitaService, CitaService>();
        services.AddScoped<IPermisosAgenda, PermisosAgenda>();
        services.AddScoped<IAccesoService, AccesoService>();

        return services;
    }
}