using Microsoft.AspNetCore.Mvc;
using MonProjetWeb.Application.Common.Interfaces.Services;

namespace MonProjetWeb.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecommendationController : ControllerBase
{
    private readonly IRecommendationService _recService;

    public RecommendationController(IRecommendationService recService)
    {
        _recService = recService;
    }

    /// <summary>Toutes les recommandations d'un compte GCP</summary>
    [HttpGet("{gcpAccountId:int}")]
    public async Task<IActionResult> GetRecommendations(int gcpAccountId)
    {
        var recs = await _recService.GetRecommendationsAsync(gcpAccountId);
        return Ok(recs);
    }

    /// <summary>Recommandations en attente uniquement</summary>
    [HttpGet("{gcpAccountId:int}/pending")]
    public async Task<IActionResult> GetPending(int gcpAccountId)
    {
        var recs = await _recService.GetPendingRecommendationsAsync(gcpAccountId);
        return Ok(recs);
    }

    /// <summary>Total des économies potentielles</summary>
    [HttpGet("{gcpAccountId:int}/savings")]
    public async Task<IActionResult> GetPotentialSavings(int gcpAccountId)
    {
        var savings = await _recService.GetTotalPotentialSavingsAsync(gcpAccountId);
        return Ok(new { potentialSavings = savings, currency = "USD" });
    }

    /// <summary>Générer automatiquement des recommandations</summary>
    [HttpPost("{gcpAccountId:int}/generate")]
    public async Task<IActionResult> Generate(int gcpAccountId)
    {
        await _recService.GenerateRecommendationsAsync(gcpAccountId);
        var recs = await _recService.GetPendingRecommendationsAsync(gcpAccountId);
        return Ok(new
        {
            message = $"{recs.Count()} recommandation(s) générée(s).",
            recommendations = recs
        });
    }

    /// <summary>Appliquer une recommandation</summary>
    [HttpPatch("{recommendationId:int}/apply")]
    public async Task<IActionResult> Apply(int recommendationId)
    {
        try
        {
            await _recService.ApplyRecommendationAsync(recommendationId);
            return Ok(new { message = "Recommandation appliquée." });
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>Ignorer une recommandation</summary>
    [HttpPatch("{recommendationId:int}/dismiss")]
    public async Task<IActionResult> Dismiss(int recommendationId)
    {
        try
        {
            await _recService.DismissRecommendationAsync(recommendationId);
            return Ok(new { message = "Recommandation ignorée." });
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}