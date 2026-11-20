using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MsIdentity.Core.Configs;
using MsIdentity.Core.Services;

namespace MsIdentity.Core;

public static class Extension
{
    public static IServiceCollection AddCore(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));

        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}