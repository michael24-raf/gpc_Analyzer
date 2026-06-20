using Microsoft.AspNetCore.Mvc;
using MonProjetWeb.Application.Common.Interfaces.Services;
using MonProjetWeb.Domain.Entities;

namespace MonProjetWeb.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlertController : ControllerBase
{
    private readonly IAlertService           _alertService;
    private readonly IAnomalyDetectionService _anomalyService;

    public AlertController(
        IAlertService alertService,
        IAnomalyDetectionService anomalyService)
    {
        _alertService   = alertService;
        _anomalyService = anomalyService;
    }

    /// <summary>Toutes les alertes d'un compte GCP</summary>
    [HttpGet("{gcpAccountId:int}")]
    public async Task<IActionResult> GetAlerts(int gcpAccountId)
    {
        var alerts = await _alertService.GetAlertsAsync(gcpAccountId);
        return Ok(alerts);
    }

    /// <summary>Alertes actives uniquement</summary>
    [HttpGet("{gcpAccountId:int}/active")]
    public async Task<IActionResult> GetActiveAlerts(int gcpAccountId)
    {
        var alerts = await _alertService.GetActiveAlertsAsync(gcpAccountId);
        return Ok(alerts);
    }

    /// <summary>Nombre d'alertes actives</summary>
    [HttpGet("{gcpAccountId:int}/count")]
    public async Task<IActionResult> GetActiveCount(int gcpAccountId)
    {
        var count = await _alertService.GetActiveAlertCountAsync(gcpAccountId);
        return Ok(new { count });
    }

    /// <summary>Créer une alerte manuellement</summary>
    [HttpPost]
    public async Task<IActionResult> CreateAlert([FromBody] CreateAlertRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var alert = await _alertService.CreateAlertAsync(
            request.GcpAccountId,
            request.Title,
            request.Message,
            request.Severity,
            request.BudgetId);

        return CreatedAtAction(nameof(GetAlerts),
            new { gcpAccountId = alert.GcpAccountId }, alert);
    }

    /// <summary>Acquitter une alerte</summary>
    [HttpPatch("{alertId:int}/acknowledge")]
    public async Task<IActionResult> Acknowledge(int alertId)
    {
        try
        {
            await _alertService.AcknowledgeAlertAsync(alertId);
            return Ok(new { message = "Alerte acquittée." });
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>Résoudre une alerte</summary>
    [HttpPatch("{alertId:int}/resolve")]
    public async Task<IActionResult> Resolve(int alertId)
    {
        try
        {
            await _alertService.ResolveAlertAsync(alertId);
            return Ok(new { message = "Alerte résolue." });
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>Vérifier et générer les alertes budget</summary>
    [HttpPost("{gcpAccountId:int}/check-budgets")]
    public async Task<IActionResult> CheckBudgetAlerts(int gcpAccountId)
    {
        await _alertService.CheckAndGenerateBudgetAlertsAsync(gcpAccountId);
        return Ok(new { message = "Vérification des budgets effectuée." });
    }

    /// <summary>Détecter les anomalies et générer les alertes</summary>
    [HttpPost("{gcpAccountId:int}/detect-anomalies")]
    public async Task<IActionResult> DetectAnomalies(int gcpAccountId)
    {
        var alerts = await _anomalyService.DetectAnomaliesAsync(gcpAccountId);
        return Ok(new
        {
            message = $"{alerts.Count()} anomalie(s) détectée(s).",
            alerts
        });
    }
}

// ── Request DTO ──────────────────────────────────────────────────────────────

public record CreateAlertRequest(
    int           GcpAccountId,
    string        Title,
    string        Message,
    AlertSeverity Severity,
    int?          BudgetId = null);