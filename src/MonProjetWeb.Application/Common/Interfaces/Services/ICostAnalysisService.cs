using MonProjetWeb.Application.Common.DTOs.GCP;

namespace MonProjetWeb.Application.Common.Interfaces.Services;

public interface ICostAnalysisService
{
    Task<GcpSummaryDto> GetDashboardSummaryAsync(int gcpAccountId);
    Task<IEnumerable<GcpCostDto>> GetCostsByPeriodAsync(int gcpAccountId, DateTime start, DateTime end);
    Task<Dictionary<string, decimal>> GetCostsByServiceAsync(int gcpAccountId, DateTime start, DateTime end);
    Task<Dictionary<string, decimal>> GetMonthlyCostsAsync(int gcpAccountId, int months = 6);
    Task<decimal> GetTotalSpendAsync(int gcpAccountId, DateTime start, DateTime end);
    Task SyncFromGcpAsync(int gcpAccountId, string billingAccountId);
}