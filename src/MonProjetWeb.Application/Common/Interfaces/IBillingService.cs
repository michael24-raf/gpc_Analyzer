using MonProjetWeb.Application.Common.DTOs.GCP;

namespace MonProjetWeb.Application.Common.Interfaces;

public interface IBillingService
{
    Task<List<GcpCostDto>>    GetCurrentMonthCostsAsync(string billingAccountId);
    Task<decimal>             GetTotalSpendAsync(string billingAccountId, DateTime from, DateTime to);
    Task<List<GcpBudgetDto>>  GetBudgetStatusAsync(string billingAccountId);
    Task SyncCostsToDbAsync(int gcpAccountId, string billingAccountId);
}