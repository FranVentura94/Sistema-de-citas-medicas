using BuildingBlocks.Rest.Builders;
using BuildingBlocks.Rest.Extensions;
using BuildingBlocks.Rest.Interfaces.IServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MsClinical.Core.Interfaces;
using MsClinical.Infrastructure.Services;

namespace MsClinical.Infrastructure;

public static class Extension
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRequestLogging(configuration);

        services.AddHttpClient<IRest, RestBuilder>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddHttpContextAccessor();
        services.AddScoped<IAgendaService, AgendaService>();

        return services;
    }
}