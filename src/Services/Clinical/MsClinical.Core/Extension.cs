using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MsClinical.Core.Configs;
using MsClinical.Core.Services;

namespace MsClinical.Core;

public static class Extension
{
    public static IServiceCollection AddCore(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DownstreamOptions>(configuration.GetSection("Downstream"));

        services.AddScoped<IAtencionService, AtencionService>();
        services.AddScoped<IExpedienteService, ExpedienteService>();
        services.AddScoped<ITriajeService, TriajeService>();

        return services;
    }
}