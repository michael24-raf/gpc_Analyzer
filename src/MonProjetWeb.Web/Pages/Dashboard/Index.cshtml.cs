using Microsoft.AspNetCore.Mvc.RazorPages;
using MonProjetWeb.Application.Common.Interfaces.Services;
using MonProjetWeb.Application.Common.DTOs.GCP;
using MonProjetWeb.Domain.Entities;

namespace MonProjetWeb.Web.Pages.Dashboard;

public class IndexModel : PageModel
{
    private readonly ICostAnalysisService   _costService;
    private readonly IBudgetService         _budgetService;
    private readonly IAlertService          _alertService;
    private readonly IRecommendationService _recService;

    public IndexModel(
        ICostAnalysisService costService,
        IBudgetService budgetService,
        IAlertService alertService,
        IRecommendationService recService)
    {
        _costService   = costService;
        _budgetService = budgetService;
        _alertService  = alertService;
        _recService    = recService;
    }

    // ── Données dashboard ────────────────────────────────────────────────────
    public decimal TotalCost         { get; set; }
    public decimal PotentialSavings  { get; set; }
    public int     AnomaliesCount    { get; set; }
    public int     ActiveAlertCount  { get; set; }

    public List<GcpCostDto>    TopServices     { get; set; } = new();
    public List<Budget>        Budgets         { get; set; } = new();
    public List<Alert>         ActiveAlerts    { get; set; } = new();
    public List<Recommendation> Recommendations { get; set; } = new();

    // Pour les graphiques (JSON)
    public string MonthlyLabels  { get; set; } = "[]";
    public string MonthlyData    { get; set; } = "[]";
    public string ServiceLabels  { get; set; } = "[]";
    public string ServiceData    { get; set; } = "[]";

    // ID du compte GCP (en dur pour l'instant, sera dynamique avec auth)
    private const int GcpAccountId = 1;

    public async Task OnGetAsync()
    {
        var now        = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1);

        // Résumé
        var summary = await _costService.GetDashboardSummaryAsync(GcpAccountId);
        TotalCost        = summary.TotalCost;
        PotentialSavings = summary.PotentialSavings;
        AnomaliesCount   = summary.AnomaliesCount;
        TopServices      = summary.TopServices;

        // Alertes
        var alerts       = await _alertService.GetActiveAlertsAsync(GcpAccountId);
        ActiveAlerts     = alerts.Take(5).ToList();
        ActiveAlertCount = alerts.Count();

        // Budgets
        var budgets = await _budgetService.GetBudgetsAsync(GcpAccountId);
        Budgets = budgets.Take(4).ToList();

        // Recommandations
        var recs      = await _recService.GetPendingRecommendationsAsync(GcpAccountId);
        Recommendations = recs.Take(4).ToList();

        // Données graphique mensuel
        var monthly = await _costService.GetMonthlyCostsAsync(GcpAccountId, 6);
        MonthlyLabels = System.Text.Json.JsonSerializer.Serialize(monthly.Keys.ToList());
        MonthlyData   = System.Text.Json.JsonSerializer.Serialize(monthly.Values.ToList());

        // Données graphique par service
        var bySvc = await _costService.GetCostsByServiceAsync(GcpAccountId, monthStart, now);
        ServiceLabels = System.Text.Json.JsonSerializer.Serialize(bySvc.Keys.ToList());
        ServiceData   = System.Text.Json.JsonSerializer.Serialize(bySvc.Values.ToList());
    }
}