using MonProjetWeb.Application.Common.Interfaces;
using MonProjetWeb.Application.Common.Interfaces.Services;
using MonProjetWeb.Domain.Entities;

namespace MonProjetWeb.Application.Services;

public class BudgetService : IBudgetService
{
    private readonly IBudgetRepository      _budgetRepo;
    private readonly ICostRepository        _costRepo;
    private readonly IGoogleCloudService    _gcp;

    public BudgetService(
        IBudgetRepository budgetRepo,
        ICostRepository costRepo,
        IGoogleCloudService gcp)
    {
        _budgetRepo = budgetRepo;
        _costRepo   = costRepo;
        _gcp        = gcp;
    }

    public async Task<IEnumerable<Budget>> GetBudgetsAsync(int gcpAccountId)
        => await _budgetRepo.GetByAccountAsync(gcpAccountId);

    public async Task<Budget?> GetBudgetByIdAsync(int budgetId)
        => await _budgetRepo.GetWithAlertsAsync(budgetId);

    public async Task<Budget> CreateBudgetAsync(
        int gcpAccountId, string name, decimal amount,
        DateTime start, DateTime end, decimal threshold = 80)
    {
        var budget = new Budget
        {
            GcpAccountId   = gcpAccountId,
            Name           = name,
            Amount         = amount,
            SpentAmount    = 0,
            Currency       = "USD",
            PeriodStart    = start,
            PeriodEnd      = end,
            AlertThreshold = threshold,
            CreatedAt      = DateTime.UtcNow
        };

        await _budgetRepo.AddAsync(budget);
        await _budgetRepo.SaveChangesAsync();
        return budget;
    }

    public async Task<Budget> UpdateBudgetAsync(
        int budgetId, decimal amount, decimal threshold)
    {
        var budget = await _budgetRepo.GetByIdAsync(budgetId)
            ?? throw new Exception($"Budget {budgetId} introuvable.");

        budget.Amount         = amount;
        budget.AlertThreshold = threshold;
        budget.UpdatedAt      = DateTime.UtcNow;

        _budgetRepo.Update(budget);
        await _budgetRepo.SaveChangesAsync();
        return budget;
    }

    public async Task DeleteBudgetAsync(int budgetId)
    {
        var budget = await _budgetRepo.GetByIdAsync(budgetId)
            ?? throw new Exception($"Budget {budgetId} introuvable.");

        _budgetRepo.Remove(budget);
        await _budgetRepo.SaveChangesAsync();
    }

    public async Task<IEnumerable<Budget>> GetExceededBudgetsAsync(int gcpAccountId)
        => await _budgetRepo.GetExceededBudgetsAsync(gcpAccountId);

    public async Task SyncBudgetsFromGcpAsync(int gcpAccountId, string billingAccountId)
    {
        var now        = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1);

        // Récupérer le coût total réel depuis la DB
        var totalSpent = await _costRepo.GetTotalCostAsync(
            gcpAccountId, monthStart, now);

        // Mettre à jour UNIQUEMENT les budgets existants — ne pas en créer
        var allBudgets = await _budgetRepo.GetByAccountAsync(gcpAccountId);
        foreach (var budget in allBudgets)
        {
            budget.SpentAmount = totalSpent;
            budget.UpdatedAt   = DateTime.UtcNow;
            _budgetRepo.Update(budget);
        }

        await _budgetRepo.SaveChangesAsync();
    }
}