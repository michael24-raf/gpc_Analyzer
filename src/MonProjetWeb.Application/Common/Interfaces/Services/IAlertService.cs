using MonProjetWeb.Domain.Entities;

namespace MonProjetWeb.Application.Common.Interfaces.Services;

public interface IAlertService
{
    Task<IEnumerable<Alert>> GetAlertsAsync(int gcpAccountId);
    Task<IEnumerable<Alert>> GetActiveAlertsAsync(int gcpAccountId);
    Task<Alert> CreateAlertAsync(int gcpAccountId, string title, string message, AlertSeverity severity, int? budgetId = null);
    Task AcknowledgeAlertAsync(int alertId);
    Task ResolveAlertAsync(int alertId);
    Task CheckAndGenerateBudgetAlertsAsync(int gcpAccountId);
    Task<int> GetActiveAlertCountAsync(int gcpAccountId);
}