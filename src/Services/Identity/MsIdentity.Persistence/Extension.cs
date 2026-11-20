using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MsIdentity.Core.Interfaces;
using MsIdentity.Persistence.Context;
using MsIdentity.Persistence.Repositories;

namespace MsIdentity.Persistence;

public static class Extension
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<MsIdentityDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("IdentityConnection")));

        services.AddScoped<IUsuarioRepository, UsuarioRepository>();

        return services;
    }
}