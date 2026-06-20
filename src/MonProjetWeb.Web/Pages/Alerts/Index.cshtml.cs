using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MonProjetWeb.Application.Common.Interfaces.Services;
using MonProjetWeb.Domain.Entities;

namespace MonProjetWeb.Web.Pages.Alerts;

public class IndexModel : PageModel
{
    private readonly IAlertService           _alertService;
    private readonly IAnomalyDetectionService _anomalyService;
    private const int GcpAccountId = 1;

    public IndexModel(IAlertService alertService, IAnomalyDetectionService anomalyService)
    {
        _alertService   = alertService;
        _anomalyService = anomalyService;
    }

    public List<Alert> Alerts       { get; set; } = new();
    public int         ActiveCount  { get; set; }
    public int         TotalCount   { get; set; }

    [TempData] public string? SuccessMessage { get; set; }

    public async Task OnGetAsync()
    {
        var alerts  = await _alertService.GetAlertsAsync(GcpAccountId);
        Alerts      = alerts.OrderByDescending(a => a.CreatedAt).ToList();
        TotalCount  = Alerts.Count;
        ActiveCount = Alerts.Count(a => a.Status == AlertStatus.Active);
    }

    public async Task<IActionResult> OnPostAcknowledgeAsync(int alertId)
    {
        await _alertService.AcknowledgeAlertAsync(alertId);
        SuccessMessage = "Alerte acquittée.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostResolveAsync(int alertId)
    {
        await _alertService.ResolveAlertAsync(alertId);
        SuccessMessage = "Alerte résolue.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDetectAsync()
    {
        var alerts = await _anomalyService.DetectAnomaliesAsync(GcpAccountId);
        await _alertService.CheckAndGenerateBudgetAlertsAsync(GcpAccountId);
        SuccessMessage = $"{alerts.Count()} anomalie(s) détectée(s).";
        return RedirectToPage();
    }
}