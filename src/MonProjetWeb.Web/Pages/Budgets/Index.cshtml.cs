using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MonProjetWeb.Application.Common.Interfaces.Services;
using MonProjetWeb.Domain.Entities;

namespace MonProjetWeb.Web.Pages.Budgets;

public class IndexModel : PageModel
{
    private readonly IBudgetService _budgetService;
    private const int GcpAccountId = 1;

    public IndexModel(IBudgetService budgetService)
        => _budgetService = budgetService;

    public List<Budget> Budgets         { get; set; } = new();
    public List<Budget> ExceededBudgets { get; set; } = new();

    [TempData] public string? SuccessMessage { get; set; }
    [TempData] public string? ErrorMessage   { get; set; }

    public async Task OnGetAsync()
    {
        var budgets  = await _budgetService.GetBudgetsAsync(GcpAccountId);
        Budgets      = budgets.ToList();
        var exceeded = await _budgetService.GetExceededBudgetsAsync(GcpAccountId);
        ExceededBudgets = exceeded.ToList();
    }

    public async Task<IActionResult> OnPostCreateAsync(
        string name, decimal amount, DateTime periodStart,
        DateTime periodEnd, decimal alertThreshold)
    {
        try
        {
            await _budgetService.CreateBudgetAsync(
                GcpAccountId, name, amount, periodStart, periodEnd, alertThreshold);
            SuccessMessage = $"Budget '{name}' créé avec succès.";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int budgetId)
    {
        try
        {
            await _budgetService.DeleteBudgetAsync(budgetId);
            SuccessMessage = "Budget supprimé.";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        return RedirectToPage();
    }
}