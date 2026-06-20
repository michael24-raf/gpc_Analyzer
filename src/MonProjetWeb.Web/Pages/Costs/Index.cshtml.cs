using Microsoft.AspNetCore.Mvc.RazorPages;
using MonProjetWeb.Application.Common.Interfaces.Services;
using MonProjetWeb.Application.Common.DTOs.GCP;

namespace MonProjetWeb.Web.Pages.Costs;

public class IndexModel : PageModel
{
    private readonly ICostAnalysisService _costService;
    private const int GcpAccountId = 1;

    public IndexModel(ICostAnalysisService costService)
        => _costService = costService;

    public List<GcpCostDto> Costs        { get; set; } = new();
    public decimal          TotalCost    { get; set; }
    public string           MonthlyLabels { get; set; } = "[]";
    public string           MonthlyData   { get; set; } = "[]";
    public string           ServiceLabels { get; set; } = "[]";
    public string           ServiceData   { get; set; } = "[]";

    public DateTime StartDate { get; set; } = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
    public DateTime EndDate   { get; set; } = DateTime.UtcNow;

    public async Task OnGetAsync(DateTime? startDate, DateTime? endDate)
    {
        StartDate = startDate ?? new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        EndDate   = endDate   ?? DateTime.UtcNow;

        var costs = await _costService.GetCostsByPeriodAsync(GcpAccountId, StartDate, EndDate);
        Costs     = costs.OrderByDescending(c => c.Amount).ToList();
        TotalCost = Costs.Sum(c => c.Amount);

        var monthly = await _costService.GetMonthlyCostsAsync(GcpAccountId, 6);
        MonthlyLabels = System.Text.Json.JsonSerializer.Serialize(monthly.Keys.ToList());
        MonthlyData   = System.Text.Json.JsonSerializer.Serialize(monthly.Values.ToList());

        var bySvc = await _costService.GetCostsByServiceAsync(GcpAccountId, StartDate, EndDate);
        ServiceLabels = System.Text.Json.JsonSerializer.Serialize(bySvc.Keys.ToList());
        ServiceData   = System.Text.Json.JsonSerializer.Serialize(bySvc.Values.ToList());
    }
}