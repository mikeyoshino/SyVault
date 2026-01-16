using DigitalVault.Application.Interfaces;
using DigitalVault.Infrastructure.Data;
using DigitalVault.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DigitalVault.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Register DbContext as IApplicationDbContext
        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        // Register services
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IDeadManSwitchJobService, DeadManSwitchJobService>();
        services.AddScoped<IMfaService, MfaService>();

        return services;
    }
}
