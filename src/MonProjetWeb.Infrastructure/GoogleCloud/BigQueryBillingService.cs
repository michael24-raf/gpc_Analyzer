#pragma warning disable CS0618
using Google.Apis.Auth.OAuth2;
using Google.Cloud.BigQuery.V2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MonProjetWeb.Application.Common.DTOs.GCP;

namespace MonProjetWeb.Infrastructure.GoogleCloud;

public class BigQueryBillingService
{
    private readonly IConfiguration _config;
    private readonly ILogger<BigQueryBillingService> _logger;
    private readonly string _projectId;
    private readonly string _dataset;
    private readonly string _serviceAccountPath;

    public BigQueryBillingService(
        IConfiguration config,
        ILogger<BigQueryBillingService> logger)
    {
        _config             = config;
        _logger             = logger;
        _projectId          = _config["GoogleCloud:ProjectId"]!;
        _dataset            = _config["GoogleCloud:BigQueryDataset"] ?? "billing_export";
        _serviceAccountPath = _config["GoogleCloud:ServiceAccountPath"]!;
    }

    private async Task<BigQueryClient?> GetClientAsync()
    {
        try
        {
            var fullPath = Path.IsPathRooted(_serviceAccountPath)
                ? _serviceAccountPath
                : Path.Combine(Directory.GetCurrentDirectory(), _serviceAccountPath);

            if (!File.Exists(fullPath))
            {
                _logger.LogWarning("Service account introuvable : {Path}", fullPath);
                return null;
            }

            var credential = await GoogleCredential
                .FromFileAsync(fullPath, CancellationToken.None);

            return await BigQueryClient.CreateAsync(_projectId, credential);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Impossible de créer le client BigQuery");
            return null;
        }
    }

    public async Task<List<GcpCostDto>> GetRealCostsByServiceAsync(
        DateTime startDate, DateTime endDate)
    {
        try
        {
            var client = await GetClientAsync();
            if (client is null) return new List<GcpCostDto>();

            var query = $@"
                SELECT
                    service.description as service_name,
                    service.id as service_id,
                    SUM(cost) as total_cost,
                    currency
                FROM `{_projectId}.{_dataset}.gcp_billing_export_v1_*`
                WHERE DATE(usage_start_time) >= '{startDate:yyyy-MM-dd}'
                  AND DATE(usage_start_time) <= '{endDate:yyyy-MM-dd}'
                  AND cost > 0
                GROUP BY service.description, service.id, currency
                ORDER BY total_cost DESC";

            var results = await client.ExecuteQueryAsync(query, null);
            var costs   = new List<GcpCostDto>();

            foreach (var row in results)
            {
                var amountStr = row["total_cost"]?.ToString() ?? "0";
                if (!decimal.TryParse(amountStr,
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out var amount)) continue;

                costs.Add(new GcpCostDto
                {
                    ServiceName  = row["service_name"]?.ToString() ?? "Unknown",
                    ServiceId    = row["service_id"]?.ToString()   ?? "",
                    Amount       = Math.Round(amount, 4),
                    Currency     = row["currency"]?.ToString()     ?? "USD",
                    PeriodStart  = startDate,
                    PeriodEnd    = endDate,
                    Region       = "global",
                    ResourceName = ""
                });
            }

            _logger.LogInformation("BigQuery : {Count} services récupérés", costs.Count);
            return costs;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erreur BigQuery — fallback sur données simulées");
            return new List<GcpCostDto>();
        }
    }

    public async Task<bool> IsDataAvailableAsync()
    {
        try
        {
            var client = await GetClientAsync();
            if (client is null) return false;

            var query = $@"
                SELECT COUNT(*) as cnt
                FROM `{_projectId}.{_dataset}.gcp_billing_export_v1_*`
                LIMIT 1";

            var result = await client.ExecuteQueryAsync(query, null);
            foreach (var row in result)
            {
                var cnt = long.Parse(row["cnt"]?.ToString() ?? "0");
                return cnt > 0;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }
}
