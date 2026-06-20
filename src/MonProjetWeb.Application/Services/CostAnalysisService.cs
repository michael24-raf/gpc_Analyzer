using MonProjetWeb.Application.Common.DTOs.GCP;
using MonProjetWeb.Application.Common.Interfaces;
using MonProjetWeb.Application.Common.Interfaces.Services;
using MonProjetWeb.Domain.Entities;

namespace MonProjetWeb.Application.Services;

public class CostAnalysisService : ICostAnalysisService
{
    private readonly ICostRepository    _costRepo;
    private readonly IGcpAccountRepository _accountRepo;
    private readonly IBillingService    _billing;

    public CostAnalysisService(
        ICostRepository costRepo,
        IGcpAccountRepository accountRepo,
        IBillingService billing)
    {
        _costRepo    = costRepo;
        _accountRepo = accountRepo;
        _billing     = billing;
    }

    public async Task<GcpSummaryDto> GetDashboardSummaryAsync(int gcpAccountId)
    {
        var account = await _accountRepo.GetByIdAsync(gcpAccountId)
            ?? throw new Exception($"Compte GCP {gcpAccountId} introuvable.");

        var now        = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1);

        var totalCost    = await _costRepo.GetTotalCostAsync(gcpAccountId, monthStart, now);
        var topServices  = await _costRepo.GetTopServicesAsync(gcpAccountId, 5);
        var monthlyMap   = await _costRepo.GetMonthlyCostSummaryAsync(gcpAccountId, 6);

        // Anomalies : services dont le coût dépasse 2x la moyenne
        var allCosts     = await _costRepo.GetByPeriodAsync(gcpAccountId, monthStart, now);
        var avgCost      = allCosts.Any() ? allCosts.Average(c => c.Amount) : 0;
        var anomalies    = allCosts.Count(c => c.Amount > avgCost * 2);

        return new GcpSummaryDto
        {
            TotalCost         = totalCost,
            Currency          = "USD",
            BudgetAmount      = 0,   // mis à jour par BudgetService
            BudgetUsedPercent = 0,
            AnomaliesCount    = anomalies,
            PotentialSavings  = totalCost * 0.15m,
            TopServices       = topServices.Select(c => new GcpCostDto
            {
                ServiceName  = c.ServiceName,
                ServiceId    = c.ServiceId,
                Amount       = c.Amount,
                Currency     = c.Currency,
                PeriodStart  = monthStart,
                PeriodEnd    = now
            }).ToList(),
            GeneratedAt = DateTime.UtcNow
        };
    }

    public async Task<IEnumerable<GcpCostDto>> GetCostsByPeriodAsync(
        int gcpAccountId, DateTime start, DateTime end)
    {
        var records = await _costRepo.GetByPeriodAsync(gcpAccountId, start, end);
        return records.Select(c => new GcpCostDto
        {
            ServiceName  = c.ServiceName,
            ServiceId    = c.ServiceId,
            Amount       = c.Amount,
            Currency     = c.Currency,
            PeriodStart  = c.PeriodStart,
            PeriodEnd    = c.PeriodEnd,
            Region       = c.Region,
            ResourceName = c.ResourceName
        });
    }

    public async Task<Dictionary<string, decimal>> GetCostsByServiceAsync(
        int gcpAccountId, DateTime start, DateTime end)
        => await _costRepo.GetCostGroupedByServiceAsync(gcpAccountId, start, end);

    public async Task<Dictionary<string, decimal>> GetMonthlyCostsAsync(
        int gcpAccountId, int months = 6)
        => await _costRepo.GetMonthlyCostSummaryAsync(gcpAccountId, months);

    public async Task<decimal> GetTotalSpendAsync(
        int gcpAccountId, DateTime start, DateTime end)
        => await _costRepo.GetTotalCostAsync(gcpAccountId, start, end);

    public async Task SyncFromGcpAsync(int gcpAccountId, string billingAccountId)
        => await _billing.SyncCostsToDbAsync(gcpAccountId, billingAccountId);
}