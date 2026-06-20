using MonProjetWeb.Domain.Entities;

namespace MonProjetWeb.Application.Common.Interfaces.Services;

public interface IAnomalyDetectionService
{
    Task<IEnumerable<Alert>> DetectAnomaliesAsync(int gcpAccountId);
    Task<bool> IsAnomalyAsync(int gcpAccountId, string serviceId, decimal currentAmount);
    Task<decimal> GetAverageSpendAsync(int gcpAccountId, string serviceId, int historicalDays = 30);
}