using Microsoft.Extensions.DependencyInjection;
using MonProjetWeb.Application.Common.Interfaces.Services;
using MonProjetWeb.Application.Services;

namespace MonProjetWeb.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICostAnalysisService,    CostAnalysisService>();
        services.AddScoped<IBudgetService,          BudgetService>();
        services.AddScoped<IAlertService,           AlertService>();
        services.AddScoped<IRecommendationService,  RecommendationService>();
        services.AddScoped<IAnomalyDetectionService, AnomalyDetectionService>();

        return services;
    }
}