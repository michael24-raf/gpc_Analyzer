namespace MonProjetWeb.Application.Common.DTOs.GCP;

public class GcpCostDto
{
    public string  ServiceName  { get; set; } = string.Empty;
    public string  ServiceId    { get; set; } = string.Empty;
    public decimal Amount       { get; set; }
    public string  Currency     { get; set; } = "USD";
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd   { get; set; }
    public string  Region       { get; set; } = string.Empty;
    public string  ResourceName { get; set; } = string.Empty;
}