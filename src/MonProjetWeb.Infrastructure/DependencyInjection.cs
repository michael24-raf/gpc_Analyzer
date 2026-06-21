using Microsoft.Extensions.DependencyInjection;
using MonProjetWeb.Application.Common.Interfaces;
using MonProjetWeb.Infrastructure.GoogleCloud;
using MonProjetWeb.Infrastructure.Services;

namespace MonProjetWeb.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Auth
        services.AddScoped<IJwtService,  JwtService>();
        services.AddScoped<IAuthService, AuthService>();

        // Google Cloud
        services.AddScoped<BigQueryBillingService>();
        services.AddScoped<IGoogleCloudService, GoogleCloudService>();
        services.AddScoped<IBillingService,     BillingService>();

        return services;
    }
}