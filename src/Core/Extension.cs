using Core.Configs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Core;

public static class Extension
{
    public static IServiceCollection AddCore(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DownstreamOptions>(configuration.GetSection("Downstream"));
        services.Configure<PasarelaOptions>(configuration.GetSection("Pasarela"));
        return services;
    }
}