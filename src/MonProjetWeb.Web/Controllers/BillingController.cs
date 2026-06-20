using Microsoft.AspNetCore.Mvc;
using MonProjetWeb.Application.Common.Interfaces;
using MonProjetWeb.Application.Common.Interfaces.Services;

namespace MonProjetWeb.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BillingController : ControllerBase
{
    private readonly ICostAnalysisService _costService;
    private readonly IConfiguration       _config;

    public BillingController(ICostAnalysisService costService, IConfiguration config)
    {
        _costService = costService;
        _config      = config;
    }

    private string BillingAccountId => _config["GoogleCloud:BillingAccountId"]!;

    /// <summary>Résumé dashboard du compte GCP</summary>
    [HttpGet("summary/{gcpAccountId:int}")]
    public async Task<IActionResult> GetSummary(int gcpAccountId)
    {
        var summary = await _costService.GetDashboardSummaryAsync(gcpAccountId);
        return Ok(summary);
    }

    /// <summary>Coûts par service sur une période</summary>
    [HttpGet("costs/{gcpAccountId:int}/by-service")]
    public async Task<IActionResult> GetCostsByService(
        int gcpAccountId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        if (startDate >= endDate)
            return BadRequest("La date de début doit être antérieure à la date de fin.");

        var costs = await _costService.GetCostsByServiceAsync(gcpAccountId, startDate, endDate);
        return Ok(costs);
    }

    /// <summary>Coûts mensuels sur N mois</summary>
    [HttpGet("costs/{gcpAccountId:int}/monthly")]
    public async Task<IActionResult> GetMonthlyCosts(
        int gcpAccountId,
        [FromQuery] int months = 6)
    {
        var costs = await _costService.GetMonthlyCostsAsync(gcpAccountId, months);
        return Ok(costs);
    }

    /// <summary>Coût total sur une période</summary>
    [HttpGet("costs/{gcpAccountId:int}/total")]
    public async Task<IActionResult> GetTotalCost(
        int gcpAccountId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var total = await _costService.GetTotalSpendAsync(gcpAccountId, startDate, endDate);
        return Ok(new { total, currency = "USD" });
    }

    /// <summary>Synchroniser les coûts depuis GCP</summary>
    [HttpPost("sync/{gcpAccountId:int}")]
    public async Task<IActionResult> SyncCosts(int gcpAccountId)
    {
        await _costService.SyncFromGcpAsync(gcpAccountId, BillingAccountId);
        return Ok(new { message = "Synchronisation effectuée avec succès." });
    }
}