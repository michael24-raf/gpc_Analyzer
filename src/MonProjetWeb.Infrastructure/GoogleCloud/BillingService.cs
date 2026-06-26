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

    // Essayer BigQuery en premier
    List<GcpCostDto> costs;
    var isAvailable = await _gcp.GetCostsByServiceAsync(billingAccountId, start, now);
    costs = isAvailable;

    // Supprimer les anciens enregistrements du mois en cours
    var existing = await _db.CostRecords
        .Where(c => c.GcpAccountId == gcpAccountId
                 && c.PeriodStart  >= start)
        .ToListAsync();

    if (existing.Any())
    {
        _db.CostRecords.RemoveRange(existing);
        await _db.SaveChangesAsync();
    }

    // Insérer les nouveaux
    foreach (var cost in costs)
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

    await _db.SaveChangesAsync();
    _logger.LogInformation("{Count} enregistrements synchronisés", costs.Count);
}
}