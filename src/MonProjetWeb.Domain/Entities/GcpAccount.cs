using MonProjetWeb.Domain.Common;

namespace MonProjetWeb.Domain.Entities;

public class GcpAccount : BaseEntity
{
    public string AccountName       { get; set; } = string.Empty;
    public string ProjectId         { get; set; } = string.Empty;
    public string BillingAccountId  { get; set; } = string.Empty;
    public bool   IsActive          { get; set; } = true;

    // FK
    public int  UserId { get; set; }
    public User User   { get; set; } = null!;

    // Navigation
    public ICollection<CostRecord>      CostRecords     { get; set; } = new List<CostRecord>();
    public ICollection<Budget>          Budgets         { get; set; } = new List<Budget>();
    public ICollection<Alert>           Alerts          { get; set; } = new List<Alert>();
    public ICollection<Recommendation>  Recommendations { get; set; } = new List<Recommendation>();
}