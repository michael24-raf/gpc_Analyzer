using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MonProjetWeb.Application.Common.Interfaces.Services;
using MonProjetWeb.Application.Common.DTOs.GCP;
using MonProjetWeb.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace MonProjetWeb.Web.Pages.Dashboard;

public class IndexModel : PageModel
{
    private readonly ICostAnalysisService   _costService;
    private readonly IBudgetService         _budgetService;
    private readonly IAlertService          _alertService;
    private readonly IRecommendationService _recService;
    private readonly IAnomalyDetectionService _anomalyService;
    private readonly IConfiguration        _config;

    public IndexModel(
        ICostAnalysisService costService,
        IBudgetService budgetService,
        IAlertService alertService,
        IRecommendationService recService,
        IAnomalyDetectionService anomalyService,
        IConfiguration config)
    {
        _costService    = costService;
        _budgetService  = budgetService;
        _alertService   = alertService;
        _recService     = recService;
        _anomalyService = anomalyService;
        _config         = config;
    }

    public decimal TotalCost         { get; set; }
    public decimal PotentialSavings  { get; set; }
    public int     AnomaliesCount    { get; set; }
    public int     ActiveAlertCount  { get; set; }

    public List<GcpCostDto>     TopServices     { get; set; } = new();
    public List<Budget>         Budgets         { get; set; } = new();
    public List<Alert>          ActiveAlerts    { get; set; } = new();
    public List<Recommendation> Recommendations { get; set; } = new();

    public string MonthlyLabels { get; set; } = "[]";
    public string MonthlyData   { get; set; } = "[]";
    public string ServiceLabels { get; set; } = "[]";
    public string ServiceData   { get; set; } = "[]";

    [TempData] public string? SuccessMessage { get; set; }
    [TempData] public string? ErrorMessage   { get; set; }

    private const int GcpAccountId = 1;
    private string BillingAccountId => _config["GoogleCloud:BillingAccountId"]!;

    public async Task OnGetAsync()
    {
        var now        = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1);

        var summary      = await _costService.GetDashboardSummaryAsync(GcpAccountId);
        TotalCost        = summary.TotalCost;
        PotentialSavings = summary.PotentialSavings;
        AnomaliesCount   = summary.AnomaliesCount;
        TopServices      = summary.TopServices;

        var alerts       = await _alertService.GetActiveAlertsAsync(GcpAccountId);
        ActiveAlerts     = alerts.Take(5).ToList();
        ActiveAlertCount = alerts.Count();

        var budgets = await _budgetService.GetBudgetsAsync(GcpAccountId);
        Budgets     = budgets.Take(4).ToList();

        var recs        = await _recService.GetPendingRecommendationsAsync(GcpAccountId);
        Recommendations = recs.Take(4).ToList();

        var monthly     = await _costService.GetMonthlyCostsAsync(GcpAccountId, 6);
        MonthlyLabels   = System.Text.Json.JsonSerializer.Serialize(monthly.Keys.ToList());
        MonthlyData     = System.Text.Json.JsonSerializer.Serialize(monthly.Values.ToList());

        var bySvc     = await _costService.GetCostsByServiceAsync(GcpAccountId, monthStart, now);
        ServiceLabels = System.Text.Json.JsonSerializer.Serialize(bySvc.Keys.ToList());
        ServiceData   = System.Text.Json.JsonSerializer.Serialize(bySvc.Values.ToList());
    }

    // ── Actions ──────────────────────────────────────────────────────────────

    public async Task<IActionResult> OnPostSyncGcpAsync()
    {
        try
        {
            await _costService.SyncFromGcpAsync(GcpAccountId, BillingAccountId);
            SuccessMessage = "✅ Coûts GCP synchronisés avec succès.";
        }
        catch (Exception ex) { ErrorMessage = ex.Message; }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSyncBudgetsAsync()
    {
        try
        {
            await _budgetService.SyncBudgetsFromGcpAsync(GcpAccountId, BillingAccountId);
            SuccessMessage = "✅ Budgets synchronisés avec succès.";
        }
        catch (Exception ex) { ErrorMessage = ex.Message; }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCheckAlertsAsync()
    {
        try
        {
            await _alertService.CheckAndGenerateBudgetAlertsAsync(GcpAccountId);
            SuccessMessage = "✅ Vérification des budgets effectuée.";
        }
        catch (Exception ex) { ErrorMessage = ex.Message; }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDetectAnomaliesAsync()
    {
        try
        {
            var alerts = await _anomalyService.DetectAnomaliesAsync(GcpAccountId);
            SuccessMessage = $"✅ {alerts.Count()} anomalie(s) détectée(s).";
        }
        catch (Exception ex) { ErrorMessage = ex.Message; }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostGenerateRecommendationsAsync()
    {
        try
        {
            await _recService.GenerateRecommendationsAsync(GcpAccountId);
            SuccessMessage = "✅ Recommandations générées avec succès.";
        }
        catch (Exception ex) { ErrorMessage = ex.Message; }
        return RedirectToPage();
    }
}