using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MonProjetWeb.Application.Common.DTOs.GCP;
using MonProjetWeb.Application.Common.Interfaces;
using MonProjetWeb.Domain.Entities;
using MonProjetWeb.Persistence.Context;

namespace MonProjetWeb.Infrastructure.GoogleCloud;

public class BillingService : IBillingService
{
    private readonly IGoogleCloudService  _gcp;
    private readonly ApplicationDbContext _db;
    private readonly ILogger<BillingService> _logger;

    public BillingService(
        IGoogleCloudService gcp,
        ApplicationDbContext db,
        ILogger<BillingService> logger)
    {
        _gcp    = gcp;
        _db     = db;
        _logger = logger;
    }

    public async Task<List<GcpCostDto>> GetCurrentMonthCostsAsync(string billingAccountId)
    {
        var now   = DateTime.UtcNow;
        var start = new DateTime(now.Year, now.Month, 1);
        return await _gcp.GetCostsByServiceAsync(billingAccountId, start, now);
    }

    public async Task<decimal> GetTotalSpendAsync(
        string billingAccountId, DateTime from, DateTime to)
    {
        var costs = await _gcp.GetCostsByServiceAsync(billingAccountId, from, to);
        return costs.Sum(c => c.Amount);
    }

    public async Task<List<GcpBudgetDto>> GetBudgetStatusAsync(string billingAccountId)
        => await _gcp.GetBudgetsAsync(billingAccountId);

    public async Task SyncCostsToDbAsync(int gcpAccountId, string billingAccountId)
    {
        _logger.LogInformation(
            "Synchronisation des coûts pour le compte GCP {Id}", gcpAccountId);

        var now   = DateTime.UtcNow;
        var start = new DateTime(now.Year, now.Month, 1);
        var costs = await _gcp.GetCostsByServiceAsync(billingAccountId, start, now);

        foreach (var cost in costs)
        {
            // Éviter les doublons
            var exists = await _db.CostRecords.AnyAsync(c =>
                c.GcpAccountId == gcpAccountId &&
                c.ServiceId    == cost.ServiceId &&
                c.PeriodStart  == cost.PeriodStart);

            if (!exists)
            {
                _db.CostRecords.Add(new CostRecord
                {
                    GcpAccountId = gcpAccountId,
                    ServiceName  = cost.ServiceName,
                    ServiceId    = cost.ServiceId,
                    Amount       = cost.Amount,
                    Currency     = cost.Currency,
                    PeriodStart  = cost.PeriodStart,
                    PeriodEnd    = cost.PeriodEnd,
                    Region       = cost.Region,
                    ResourceName = cost.ResourceName,
                    CreatedAt    = DateTime.UtcNow
                });
            }
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("{Count} enregistrements synchronisés", costs.Count);
    }
}