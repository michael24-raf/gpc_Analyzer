using MonProjetWeb.Domain.Common;

namespace MonProjetWeb.Domain.Entities;

public enum AlertSeverity { Low, Medium, High, Critical }
public enum AlertStatus   { Active, Acknowledged, Resolved }

public class Alert : BaseEntity
{
    public string        Title       { get; set; } = string.Empty;
    public string        Message     { get; set; } = string.Empty;
    public AlertSeverity Severity    { get; set; } = AlertSeverity.Medium;
    public AlertStatus   Status      { get; set; } = AlertStatus.Active;
    public decimal?      ThresholdValue  { get; set; }
    public decimal?      ActualValue     { get; set; }

    // FK
    public int        GcpAccountId { get; set; }
    public GcpAccount GcpAccount   { get; set; } = null!;

    public int?    BudgetId { get; set; }
    public Budget? Budget   { get; set; }
}