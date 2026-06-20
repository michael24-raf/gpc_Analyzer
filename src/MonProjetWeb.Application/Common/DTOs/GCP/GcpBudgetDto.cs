namespace MonProjetWeb.Application.Common.DTOs.GCP;

public class GcpBudgetDto
{
    public string  BudgetId     { get; set; } = string.Empty;
    public string  DisplayName  { get; set; } = string.Empty;
    public decimal BudgetAmount { get; set; }
    public decimal CurrentSpend { get; set; }
    public string  Currency     { get; set; } = "USD";
    public decimal UsagePercent => BudgetAmount > 0
        ? Math.Round(CurrentSpend / BudgetAmount * 100, 2)
        : 0;
}