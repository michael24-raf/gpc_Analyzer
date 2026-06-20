using MonProjetWeb.Domain.Common;

namespace MonProjetWeb.Domain.Entities;

public class Budget : BaseEntity
{
    public string  Name          { get; set; } = string.Empty;
    public decimal Amount        { get; set; }
    public decimal SpentAmount   { get; set; }
    public string  Currency      { get; set; } = "USD";
    public DateTime PeriodStart  { get; set; }
    public DateTime PeriodEnd    { get; set; }
    public decimal AlertThreshold { get; set; } = 80; // %

    // FK
    public int        GcpAccountId { get; set; }
    public GcpAccount GcpAccount   { get; set; } = null!;

    // Navigation
    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
}