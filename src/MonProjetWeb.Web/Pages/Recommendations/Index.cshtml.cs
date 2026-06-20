using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MonProjetWeb.Application.Common.Interfaces.Services;
using MonProjetWeb.Domain.Entities;

namespace MonProjetWeb.Web.Pages.Recommendations;

public class IndexModel : PageModel
{
    private readonly IRecommendationService _recService;
    private const int GcpAccountId = 1;

    public IndexModel(IRecommendationService recService)
        => _recService = recService;

    public List<Recommendation> Recommendations  { get; set; } = new();
    public decimal              TotalSavings     { get; set; }
    public int                  PendingCount     { get; set; }

    [TempData] public string? SuccessMessage { get; set; }

    public async Task OnGetAsync()
    {
        var recs        = await _recService.GetRecommendationsAsync(GcpAccountId);
        Recommendations = recs.OrderByDescending(r => r.EstimatedSavings).ToList();
        TotalSavings    = await _recService.GetTotalPotentialSavingsAsync(GcpAccountId);
        PendingCount    = Recommendations.Count(r =>
            r.Status == RecommendationStatus.Pending);
    }

    public async Task<IActionResult> OnPostGenerateAsync()
    {
        await _recService.GenerateRecommendationsAsync(GcpAccountId);
        SuccessMessage = "Recommandations générées avec succès.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostApplyAsync(int recId)
    {
        await _recService.ApplyRecommendationAsync(recId);
        SuccessMessage = "Recommandation appliquée.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDismissAsync(int recId)
    {
        await _recService.DismissRecommendationAsync(recId);
        SuccessMessage = "Recommandation ignorée.";
        return RedirectToPage();
    }
}