using MonProjetWeb.Domain.Common;

namespace MonProjetWeb.Domain.Entities;

public enum RecommendationType { UnusedResource, CapacityReduction, VmOptimization, StorageCleanup }
public enum RecommendationStatus { Pending, Applied, Dismissed }

public class Recommendation : BaseEntity
{
    public RecommendationType   Type            { get; set; }
    public RecommendationStatus Status          { get; set; } = RecommendationStatus.Pending;
    public string  Title            { get; set; } = string.Empty;
    public string  Description      { get; set; } = string.Empty;
    public string  ResourceName     { get; set; } = string.Empty;
    public decimal EstimatedSavings { get; set; }
    public string  Currency         { get; set; } = "USD";

    // FK
    public int        GcpAccountId { get; set; }
    public GcpAccount GcpAccount   { get; set; } = null!;
}