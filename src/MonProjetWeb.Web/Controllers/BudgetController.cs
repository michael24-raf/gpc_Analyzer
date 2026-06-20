using Microsoft.AspNetCore.Mvc;
using MonProjetWeb.Application.Common.Interfaces.Services;

namespace MonProjetWeb.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BudgetController : ControllerBase
{
    private readonly IBudgetService _budgetService;
    private readonly IConfiguration _config;

    public BudgetController(IBudgetService budgetService, IConfiguration config)
    {
        _budgetService = budgetService;
        _config        = config;
    }

    private string BillingAccountId => _config["GoogleCloud:BillingAccountId"]!;

    /// <summary>Lister tous les budgets d'un compte GCP</summary>
    [HttpGet("{gcpAccountId:int}")]
    public async Task<IActionResult> GetBudgets(int gcpAccountId)
    {
        var budgets = await _budgetService.GetBudgetsAsync(gcpAccountId);
        return Ok(budgets);
    }

    /// <summary>Détail d'un budget avec ses alertes</summary>
    [HttpGet("detail/{budgetId:int}")]
    public async Task<IActionResult> GetBudget(int budgetId)
    {
        var budget = await _budgetService.GetBudgetByIdAsync(budgetId);
        if (budget is null) return NotFound(new { message = "Budget introuvable." });
        return Ok(budget);
    }

    /// <summary>Créer un nouveau budget</summary>
    [HttpPost]
    public async Task<IActionResult> CreateBudget([FromBody] CreateBudgetRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var budget = await _budgetService.CreateBudgetAsync(
            request.GcpAccountId,
            request.Name,
            request.Amount,
            request.PeriodStart,
            request.PeriodEnd,
            request.AlertThreshold);

        return CreatedAtAction(nameof(GetBudget), new { budgetId = budget.Id }, budget);
    }

    /// <summary>Mettre à jour un budget</summary>
    [HttpPut("{budgetId:int}")]
    public async Task<IActionResult> UpdateBudget(
        int budgetId, [FromBody] UpdateBudgetRequest request)
    {
        try
        {
            var budget = await _budgetService.UpdateBudgetAsync(
                budgetId, request.Amount, request.AlertThreshold);
            return Ok(budget);
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>Supprimer un budget</summary>
    [HttpDelete("{budgetId:int}")]
    public async Task<IActionResult> DeleteBudget(int budgetId)
    {
        try
        {
            await _budgetService.DeleteBudgetAsync(budgetId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>Budgets dépassés</summary>
    [HttpGet("{gcpAccountId:int}/exceeded")]
    public async Task<IActionResult> GetExceededBudgets(int gcpAccountId)
    {
        var budgets = await _budgetService.GetExceededBudgetsAsync(gcpAccountId);
        return Ok(budgets);
    }

    /// <summary>Synchroniser les budgets depuis GCP</summary>
    [HttpPost("{gcpAccountId:int}/sync")]
    public async Task<IActionResult> SyncBudgets(int gcpAccountId)
    {
        await _budgetService.SyncBudgetsFromGcpAsync(gcpAccountId, BillingAccountId);
        return Ok(new { message = "Budgets synchronisés avec succès." });
    }
}

// ── Request DTOs ─────────────────────────────────────────────────────────────

public record CreateBudgetRequest(
    int      GcpAccountId,
    string   Name,
    decimal  Amount,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    decimal  AlertThreshold = 80);

public record UpdateBudgetRequest(
    decimal Amount,
    decimal AlertThreshold);