using MonProjetWeb.Domain.Entities;

namespace MonProjetWeb.Application.Common.Interfaces;

public interface IAlertRepository : IGenericRepository<Alert>
{
    Task<IEnumerable<Alert>> GetByAccountAsync(int gcpAccountId);
    Task<IEnumerable<Alert>> GetActiveAlertsAsync(int gcpAccountId);
    Task<IEnumerable<Alert>> GetBySeverityAsync(int gcpAccountId, AlertSeverity severity);
    Task<int> GetActiveCountAsync(int gcpAccountId);
}