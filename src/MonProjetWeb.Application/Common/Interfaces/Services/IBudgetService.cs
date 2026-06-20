using MonProjetWeb.Application.Common.DTOs.GCP;
using MonProjetWeb.Domain.Entities;

namespace MonProjetWeb.Application.Common.Interfaces.Services;

public interface IBudgetService
{
    Task<IEnumerable<Budget>> GetBudgetsAsync(int gcpAccountId);
    Task<Budget?> GetBudgetByIdAsync(int budgetId);
    Task<Budget> CreateBudgetAsync(int gcpAccountId, string name, decimal amount, DateTime start, DateTime end, decimal threshold = 80);
    Task<Budget> UpdateBudgetAsync(int budgetId, decimal amount, decimal threshold);
    Task DeleteBudgetAsync(int budgetId);
    Task<IEnumerable<Budget>> GetExceededBudgetsAsync(int gcpAccountId);
    Task SyncBudgetsFromGcpAsync(int gcpAccountId, string billingAccountId);
}