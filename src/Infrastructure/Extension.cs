using BuildingBlocks.Rest.Builders;
using BuildingBlocks.Rest.Extensions;
using BuildingBlocks.Rest.Interfaces.IServices;
using Core.Services;
using Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;

namespace Infrastructure;

public static class Extension
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRequestLogging(configuration);

        services.AddHttpClient<IRest, RestBuilder>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        })
            .AddStandardResilienceHandler(options =>
            {
                options.Retry.MaxRetryAttempts = 2;
                options.Retry.Delay = TimeSpan.FromMilliseconds(300);
                options.Retry.DisableForUnsafeHttpMethods();
            });

        services.AddScoped<IIdentityService, IdentityService>();
        services.AddSingleton<ITokenProvider, TokenProvider>();
        services.AddScoped<IPasarelaGateway, PasarelaGateway>();

        return services;
    }
}