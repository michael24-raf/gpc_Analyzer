#pragma warning disable CS0618
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Billing.Budgets.V1;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MonProjetWeb.Application.Common.DTOs.GCP;
using MonProjetWeb.Application.Common.Interfaces;
using Grpc.Auth;

namespace MonProjetWeb.Infrastructure.GoogleCloud;

public class GoogleCloudService : IGoogleCloudService
{
    private readonly IConfiguration _config;
    private readonly ILogger<GoogleCloudService> _logger;
    private readonly string _serviceAccountPath;
    private readonly BigQueryBillingService _bigQuery;
    private readonly string _projectId;

    public GoogleCloudService(
        IConfiguration config,
        ILogger<GoogleCloudService> logger,
        BigQueryBillingService bigQuery)
    {
        _config             = config;
        _logger             = logger;
        _serviceAccountPath = _config["GoogleCloud:ServiceAccountPath"]!;
        _projectId          = _config["GoogleCloud:ProjectId"]!;
        _bigQuery           = bigQuery;
    }

    public async Task<List<GcpCostDto>> GetCostsByServiceAsync(
        string billingAccountId, DateTime startDate, DateTime endDate)
    {
        try
        {
            // Essayer BigQuery directement
            var realCosts = await _bigQuery.GetRealCostsByServiceAsync(startDate, endDate);
            if (realCosts.Any())
            {
                _logger.LogInformation(
                    "BigQuery : {Count} services récupérés", realCosts.Count);
                return realCosts;
            }

            // Fallback sur données simulées
            _logger.LogInformation("BigQuery vide — données simulées");
            return await SimulateCostsByServiceAsync(startDate, endDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur — fallback simulé");
            return await SimulateCostsByServiceAsync(startDate, endDate);
        }
    }

    public async Task<List<GcpCostDto>> GetMonthlyCostsAsync(
        string billingAccountId, int months = 6)
    {
        var costs = new List<GcpCostDto>();
        var now   = DateTime.UtcNow;

        for (int i = months - 1; i >= 0; i--)
        {
            var start = new DateTime(now.Year, now.Month, 1).AddMonths(-i);
            var end   = start.AddMonths(1).AddDays(-1);
            var monthlyCosts = await GetCostsByServiceAsync(billingAccountId, start, end);
            costs.AddRange(monthlyCosts);
        }

        return costs;
    }

    public async Task<List<GcpBudgetDto>> GetBudgetsAsync(string billingAccountId)
    {
        try
        {
            var credential = await GetCredentialAsync();
            var clientBuilder = new BudgetServiceClientBuilder
            {
                ChannelCredentials = credential.ToChannelCredentials()
            };
            var client  = await clientBuilder.BuildAsync();
            var parent  = $"billingAccounts/{billingAccountId}";
            var budgets = new List<GcpBudgetDto>();

            await foreach (var budget in client.ListBudgetsAsync(parent))
            {
                var amount = budget.Amount?.SpecifiedAmount?.Units ?? 0;
                budgets.Add(new GcpBudgetDto
                {
                    BudgetId     = budget.BudgetName.BudgetId,
                    DisplayName  = budget.DisplayName,
                    BudgetAmount = (decimal)amount,
                    CurrentSpend = amount * 0.85m,
                    Currency     = budget.Amount?.SpecifiedAmount?.CurrencyCode ?? "USD"
                });
            }

            return budgets.Any() ? budgets : GetSimulatedBudgets();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Impossible de récupérer les budgets GCP, utilisation des données simulées");
            return GetSimulatedBudgets();
        }
    }

    public async Task<GcpSummaryDto> GetSummaryAsync(string billingAccountId)
    {
        var now        = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1);

        var costs      = await GetCostsByServiceAsync(billingAccountId, monthStart, now);
        var budgets    = await GetBudgetsAsync(billingAccountId);
        var totalCost  = costs.Sum(c => c.Amount);
        var totalBudget = budgets.Sum(b => b.BudgetAmount);
        var usedPercent = totalBudget > 0
            ? Math.Round(totalCost / totalBudget * 100, 2) : 0;

        return new GcpSummaryDto
        {
            TotalCost         = totalCost,
            Currency          = "USD",
            BudgetAmount      = totalBudget,
            BudgetUsedPercent = usedPercent,
            AnomaliesCount    = costs.Count(c => c.Amount > costs.Average(x => x.Amount) * 2),
            PotentialSavings  = totalCost * 0.15m,
            TopServices       = costs.OrderByDescending(c => c.Amount).Take(5).ToList(),
            Budgets           = budgets,
            GeneratedAt       = DateTime.UtcNow
        };
    }

    private async Task<GoogleCredential> GetCredentialAsync()
    {
        var fullPath = Path.IsPathRooted(_serviceAccountPath)
            ? _serviceAccountPath
            : Path.Combine(Directory.GetCurrentDirectory(), _serviceAccountPath);

        if (!File.Exists(fullPath))
        {
            _logger.LogWarning("Service account JSON introuvable : {Path}", fullPath);
            return GoogleCredential.GetApplicationDefault()
                .CreateScoped("https://www.googleapis.com/auth/cloud-platform");
        }

        using var stream = File.OpenRead(fullPath);
        return await GoogleCredential
            .FromStreamAsync(stream, CancellationToken.None)
            .ContinueWith(t => t.Result.CreateScoped(
                "https://www.googleapis.com/auth/cloud-platform"));
    }

    private static Task<List<GcpCostDto>> SimulateCostsByServiceAsync(
        DateTime start, DateTime end)
    {
        var rng      = new Random(start.Month + start.Day);
        var services = new[]
        {
            ("Compute Engine",    "compute.googleapis.com"),
            ("Cloud Storage",     "storage.googleapis.com"),
            ("BigQuery",          "bigquery.googleapis.com"),
            ("Cloud SQL",         "sqladmin.googleapis.com"),
            ("Kubernetes Engine", "container.googleapis.com"),
            ("Cloud Functions",   "cloudfunctions.googleapis.com"),
            ("Cloud Run",         "run.googleapis.com"),
            ("Pub/Sub",           "pubsub.googleapis.com"),
        };

        var costs = services.Select(s => new GcpCostDto
        {
            ServiceName  = s.Item1,
            ServiceId    = s.Item2,
            Amount       = Math.Round((decimal)(rng.NextDouble() * 500 + 20), 2),
            Currency     = "USD",
            PeriodStart  = start,
            PeriodEnd    = end,
            Region       = "europe-west1",
            ResourceName = $"projects/{s.Item2.Split('.')[0]}"
        }).ToList();

        return Task.FromResult(costs);
    }

    private static List<GcpBudgetDto> GetSimulatedBudgets() => new()
    {
        new GcpBudgetDto
        {
            BudgetId     = "budget-prod-001",
            DisplayName  = "Budget Production",
            BudgetAmount = 5000,
            CurrentSpend = 3240,
            Currency     = "USD"
        },
        new GcpBudgetDto
        {
            BudgetId     = "budget-dev-001",
            DisplayName  = "Budget Développement",
            BudgetAmount = 1000,
            CurrentSpend = 420,
            Currency     = "USD"
        }
    };
}
