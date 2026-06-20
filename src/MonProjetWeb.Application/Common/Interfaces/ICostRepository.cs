using MonProjetWeb.Domain.Entities;

namespace MonProjetWeb.Application.Common.Interfaces;

public interface ICostRepository : IGenericRepository<CostRecord>
{
    Task<IEnumerable<CostRecord>> GetByAccountAsync(int gcpAccountId);
    Task<IEnumerable<CostRecord>> GetByPeriodAsync(int gcpAccountId, DateTime start, DateTime end);
    Task<IEnumerable<CostRecord>> GetByServiceAsync(int gcpAccountId, string serviceId);
    Task<decimal> GetTotalCostAsync(int gcpAccountId, DateTime start, DateTime end);
    Task<IEnumerable<CostRecord>> GetTopServicesAsync(int gcpAccountId, int top = 5);
    Task<Dictionary<string, decimal>> GetCostGroupedByServiceAsync(int gcpAccountId, DateTime start, DateTime end);
    Task<Dictionary<string, decimal>> GetMonthlyCostSummaryAsync(int gcpAccountId, int months = 6);
}