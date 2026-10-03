using BuildingBlocks.Rest.Configs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Rest.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRequestLogging(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RequestSettings>(configuration.GetSection("RestSettings"));
        return services;
    }
}