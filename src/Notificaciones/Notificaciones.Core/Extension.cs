using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notificaciones.Core.Configs;
using Notificaciones.Core.Services;

namespace Notificaciones.Core;

public static class Extension
{
    public static IServiceCollection AddCore(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DownstreamOptions>(configuration.GetSection("Downstream"));
        services.AddScoped<INotificacionService, NotificacionService>();
        return services;
    }
}