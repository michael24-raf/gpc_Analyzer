using Microsoft.EntityFrameworkCore;
using MonProjetWeb.Application.Common.Interfaces;
using MonProjetWeb.Domain.Entities;
using MonProjetWeb.Persistence.Context;

namespace MonProjetWeb.Persistence.Repositories;

public class CostRepository : GenericRepository<CostRecord>, ICostRepository
{
    public CostRepository(ApplicationDbContext db) : base(db) { }

    public async Task<IEnumerable<CostRecord>> GetByAccountAsync(int gcpAccountId)
        => await _dbSet
            .Where(c => c.GcpAccountId == gcpAccountId)
            .OrderByDescending(c => c.PeriodStart)
            .ToListAsync();

    public async Task<IEnumerable<CostRecord>> GetByPeriodAsync(
        int gcpAccountId, DateTime start, DateTime end)
        => await _dbSet
            .Where(c => c.GcpAccountId == gcpAccountId
                     && c.PeriodStart  >= start
                     && c.PeriodEnd    <= end)
            .OrderByDescending(c => c.PeriodStart)
            .ToListAsync();

    public async Task<IEnumerable<CostRecord>> GetByServiceAsync(
        int gcpAccountId, string serviceId)
        => await _dbSet
            .Where(c => c.GcpAccountId == gcpAccountId
                     && c.ServiceId    == serviceId)
            .OrderByDescending(c => c.PeriodStart)
            .ToListAsync();

    public async Task<decimal> GetTotalCostAsync(
        int gcpAccountId, DateTime start, DateTime end)
        => await _dbSet
            .Where(c => c.GcpAccountId == gcpAccountId
                     && c.PeriodStart  >= start
                     && c.PeriodEnd    <= end)
            .SumAsync(c => c.Amount);

    public async Task<IEnumerable<CostRecord>> GetTopServicesAsync(
        int gcpAccountId, int top = 5)
        => await _dbSet
            .Where(c => c.GcpAccountId == gcpAccountId)
            .GroupBy(c => c.ServiceName)
            .Select(g => new CostRecord
            {
                ServiceName = g.Key,
                ServiceId   = g.First().ServiceId,
                Amount      = g.Sum(c => c.Amount),
                Currency    = g.First().Currency
            })
            .OrderByDescending(c => c.Amount)
            .Take(top)
            .ToListAsync();

    public async Task<Dictionary<string, decimal>> GetCostGroupedByServiceAsync(
        int gcpAccountId, DateTime start, DateTime end)
        => await _dbSet
            .Where(c => c.GcpAccountId == gcpAccountId
                     && c.PeriodStart  >= start
                     && c.PeriodEnd    <= end)
            .GroupBy(c => c.ServiceName)
            .Select(g => new { Service = g.Key, Total = g.Sum(c => c.Amount) })
            .ToDictionaryAsync(x => x.Service, x => x.Total);

   public async Task<Dictionary<string, decimal>> GetMonthlyCostSummaryAsync(
    int gcpAccountId, int months = 6)
{
    var from = DateTime.UtcNow.AddMonths(-months);

    var records = await _dbSet
        .Where(c => c.GcpAccountId == gcpAccountId
                 && c.PeriodStart  >= from)
        .ToListAsync(); // évaluation côté client ici

    return records
        .GroupBy(c => new { c.PeriodStart.Year, c.PeriodStart.Month })
        .Select(g => new
        {
            Month = $"{g.Key.Year}-{g.Key.Month:D2}",
            Total = g.Sum(c => c.Amount)
        })
        .OrderBy(x => x.Month)
        .ToDictionary(x => x.Month, x => x.Total);
}
}