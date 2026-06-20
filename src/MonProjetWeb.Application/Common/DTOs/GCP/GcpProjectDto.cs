namespace MonProjetWeb.Application.Common.DTOs.GCP;

public class GcpProjectDto
{
    public string ProjectId    { get; set; } = string.Empty;
    public string ProjectName  { get; set; } = string.Empty;
    public string BillingAccountId { get; set; } = string.Empty;
    public bool   BillingEnabled   { get; set; }
}