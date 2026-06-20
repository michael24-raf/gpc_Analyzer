using MonProjetWeb.Application.Common.Interfaces;
using MonProjetWeb.Application.Common.Interfaces.Services;
using MonProjetWeb.Domain.Entities;

namespace MonProjetWeb.Application.Services;

public class AlertService : IAlertService
{
    private readonly IAlertRepository  _alertRepo;
    private readonly IBudgetRepository _budgetRepo;

    public AlertService(IAlertRepository alertRepo, IBudgetRepository budgetRepo)
    {
        _alertRepo  = alertRepo;
        _budgetRepo = budgetRepo;
    }

    public async Task<IEnumerable<Alert>> GetAlertsAsync(int gcpAccountId)
        => await _alertRepo.GetByAccountAsync(gcpAccountId);

    public async Task<IEnumerable<Alert>> GetActiveAlertsAsync(int gcpAccountId)
        => await _alertRepo.GetActiveAlertsAsync(gcpAccountId);

    public async Task<Alert> CreateAlertAsync(
        int gcpAccountId, string title, string message,
        AlertSeverity severity, int? budgetId = null)
    {
        var alert = new Alert
        {
            GcpAccountId = gcpAccountId,
            Title        = title,
            Message      = message,
            Severity     = severity,
            Status       = AlertStatus.Active,
            BudgetId     = budgetId,
            CreatedAt    = DateTime.UtcNow
        };

        await _alertRepo.AddAsync(alert);
        await _alertRepo.SaveChangesAsync();
        return alert;
    }

    public async Task AcknowledgeAlertAsync(int alertId)
    {
        var alert = await _alertRepo.GetByIdAsync(alertId)
            ?? throw new Exception($"Alerte {alertId} introuvable.");

        alert.Status    = AlertStatus.Acknowledged;
        alert.UpdatedAt = DateTime.UtcNow;

        _alertRepo.Update(alert);
        await _alertRepo.SaveChangesAsync();
    }

    public async Task ResolveAlertAsync(int alertId)
    {
        var alert = await _alertRepo.GetByIdAsync(alertId)
            ?? throw new Exception($"Alerte {alertId} introuvable.");

        alert.Status    = AlertStatus.Resolved;
        alert.UpdatedAt = DateTime.UtcNow;

        _alertRepo.Update(alert);
        await _alertRepo.SaveChangesAsync();
    }

    public async Task CheckAndGenerateBudgetAlertsAsync(int gcpAccountId)
    {
        var exceededBudgets = await _budgetRepo.GetExceededBudgetsAsync(gcpAccountId);

        foreach (var budget in exceededBudgets)
        {
            // Vérifier si une alerte active existe déjà pour ce budget
            var alreadyAlerted = await _alertRepo.AnyAsync(a =>
                a.GcpAccountId == gcpAccountId &&
                a.BudgetId     == budget.Id    &&
                a.Status       == AlertStatus.Active);

            if (alreadyAlerted) continue;

            var percent  = budget.Amount > 0
                ? Math.Round(budget.SpentAmount / budget.Amount * 100, 1)
                : 0;

            var severity = percent >= 100 ? AlertSeverity.Critical
                         : percent >= 90  ? AlertSeverity.High
                         : AlertSeverity.Medium;

            await _alertRepo.AddAsync(new Alert
            {
                GcpAccountId   = gcpAccountId,
                BudgetId       = budget.Id,
                Title          = $"Budget dépassé : {budget.Name}",
                Message        = $"Le budget '{budget.Name}' est consommé à {percent}% ({budget.SpentAmount:F2}/{budget.Amount:F2} {budget.Currency}).",
                Severity       = severity,
                Status         = AlertStatus.Active,
                ThresholdValue = budget.Amount * budget.AlertThreshold / 100,
                ActualValue    = budget.SpentAmount,
                CreatedAt      = DateTime.UtcNow
            });
        }

        await _alertRepo.SaveChangesAsync();
    }

    public async Task<int> GetActiveAlertCountAsync(int gcpAccountId)
        => await _alertRepo.GetActiveCountAsync(gcpAccountId);
}