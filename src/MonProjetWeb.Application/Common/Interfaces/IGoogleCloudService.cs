using MonProjetWeb.Application.Common.DTOs.GCP;

namespace MonProjetWeb.Application.Common.Interfaces;

public interface IGoogleCloudService
{
    /// <summary>Récupère les coûts par service pour une période donnée</summary>
    Task<List<GcpCostDto>> GetCostsByServiceAsync(
        string billingAccountId,
        DateTime startDate,
        DateTime endDate);

    /// <summary>Récupère les coûts mensuels sur les N derniers mois</summary>
    Task<List<GcpCostDto>> GetMonthlyCostsAsync(
        string billingAccountId,
        int months = 6);

    /// <summary>Récupère les budgets GCP</summary>
    Task<List<GcpBudgetDto>> GetBudgetsAsync(string billingAccountId);

    /// <summary>Récupère le résumé global GCP</summary>
    Task<GcpSummaryDto> GetSummaryAsync(string billingAccountId);
}