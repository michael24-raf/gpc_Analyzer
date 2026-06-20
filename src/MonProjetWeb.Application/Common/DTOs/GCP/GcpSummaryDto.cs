namespace MonProjetWeb.Application.Common.DTOs.GCP;

public class GcpSummaryDto
{
    public decimal TotalCost         { get; set; }
    public string  Currency          { get; set; } = "USD";
    public decimal BudgetAmount      { get; set; }
    public decimal BudgetUsedPercent { get; set; }
    public int     AnomaliesCount    { get; set; }
    public decimal PotentialSavings  { get; set; }
    public List<GcpCostDto>    TopServices { get; set; } = new();
    public List<GcpBudgetDto>  Budgets     { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}