using MonProjetWeb.Domain.Entities;

namespace MonProjetWeb.Application.Common.Interfaces;

public interface IBudgetRepository : IGenericRepository<Budget>
{
    Task<IEnumerable<Budget>> GetByAccountAsync(int gcpAccountId);
    Task<IEnumerable<Budget>> GetActiveBudgetsAsync(int gcpAccountId);
    Task<IEnumerable<Budget>> GetExceededBudgetsAsync(int gcpAccountId);
    Task<Budget?> GetWithAlertsAsync(int budgetId);
}