using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MonProjetWeb.Application.Common.Interfaces;
using MonProjetWeb.Persistence.Context;
using MonProjetWeb.Persistence.Repositories;

namespace MonProjetWeb.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)
            ));

        // Repositories
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUserRepository,           UserRepository>();
        services.AddScoped<ICostRepository,           CostRepository>();
        services.AddScoped<IBudgetRepository,         BudgetRepository>();
        services.AddScoped<IAlertRepository,          AlertRepository>();
        services.AddScoped<IRecommendationRepository, RecommendationRepository>();
        services.AddScoped<IGcpAccountRepository,     GcpAccountRepository>();

        return services;
    }
}