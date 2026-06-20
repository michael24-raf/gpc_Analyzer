using MonProjetWeb.Application.Common.Interfaces;
using MonProjetWeb.Application.Common.Interfaces.Services;
using MonProjetWeb.Domain.Entities;

namespace MonProjetWeb.Application.Services;

public class RecommendationService : IRecommendationService
{
    private readonly IRecommendationRepository _recRepo;
    private readonly ICostRepository           _costRepo;

    public RecommendationService(
        IRecommendationRepository recRepo,
        ICostRepository costRepo)
    {
        _recRepo  = recRepo;
        _costRepo = costRepo;
    }

    public async Task<IEnumerable<Recommendation>> GetRecommendationsAsync(int gcpAccountId)
        => await _recRepo.GetByAccountAsync(gcpAccountId);

    public async Task<IEnumerable<Recommendation>> GetPendingRecommendationsAsync(int gcpAccountId)
        => await _recRepo.GetPendingAsync(gcpAccountId);

    public async Task<decimal> GetTotalPotentialSavingsAsync(int gcpAccountId)
        => await _recRepo.GetTotalPotentialSavingsAsync(gcpAccountId);

    public async Task GenerateRecommendationsAsync(int gcpAccountId)
    {
        var now   = DateTime.UtcNow;
        var start = now.AddMonths(-3);

        var costsByService = await _costRepo.GetCostGroupedByServiceAsync(
            gcpAccountId, start, now);

        foreach (var (service, totalCost) in costsByService)
        {
            await TryAddRecommendation(gcpAccountId, service, totalCost);
        }

        await _recRepo.SaveChangesAsync();
    }

    public async Task ApplyRecommendationAsync(int recommendationId)
    {
        var rec = await _recRepo.GetByIdAsync(recommendationId)
            ?? throw new Exception($"Recommandation {recommendationId} introuvable.");

        rec.Status    = RecommendationStatus.Applied;
        rec.UpdatedAt = DateTime.UtcNow;

        _recRepo.Update(rec);
        await _recRepo.SaveChangesAsync();
    }

    public async Task DismissRecommendationAsync(int recommendationId)
    {
        var rec = await _recRepo.GetByIdAsync(recommendationId)
            ?? throw new Exception($"Recommandation {recommendationId} introuvable.");

        rec.Status    = RecommendationStatus.Dismissed;
        rec.UpdatedAt = DateTime.UtcNow;

        _recRepo.Update(rec);
        await _recRepo.SaveChangesAsync();
    }

    // ── Logique de génération ────────────────────────────────────────────────

    private async Task TryAddRecommendation(
        int gcpAccountId, string service, decimal totalCost)
    {
        var alreadyExists = await _recRepo.AnyAsync(r =>
            r.GcpAccountId  == gcpAccountId &&
            r.ResourceName  == service      &&
            r.Status        == RecommendationStatus.Pending);

        if (alreadyExists) return;

        // Ressource inutilisée : coût très faible sur 3 mois
        if (totalCost < 5m)
        {
            await _recRepo.AddAsync(new Recommendation
            {
                GcpAccountId     = gcpAccountId,
                Type             = RecommendationType.UnusedResource,
                Title            = $"Ressource inutilisée : {service}",
                Description      = $"Le service '{service}' a coûté seulement {totalCost:F2} USD sur 3 mois. Envisagez sa suppression.",
                ResourceName     = service,
                EstimatedSavings = totalCost,
                Currency         = "USD",
                Status           = RecommendationStatus.Pending,
                CreatedAt        = DateTime.UtcNow
            });
            return;
        }

        // Réduction de capacité : coût élevé sur service compute
        if (totalCost > 500m && service.Contains("Compute", StringComparison.OrdinalIgnoreCase))
        {
            await _recRepo.AddAsync(new Recommendation
            {
                GcpAccountId     = gcpAccountId,
                Type             = RecommendationType.CapacityReduction,
                Title            = $"Réduction de capacité : {service}",
                Description      = $"Le service '{service}' représente {totalCost:F2} USD sur 3 mois. Une réduction de taille de VM pourrait économiser ~20%.",
                ResourceName     = service,
                EstimatedSavings = totalCost * 0.20m,
                Currency         = "USD",
                Status           = RecommendationStatus.Pending,
                CreatedAt        = DateTime.UtcNow
            });
            return;
        }

        // Optimisation VM : Kubernetes ou Cloud Run coûteux
        if (totalCost > 300m && (
            service.Contains("Kubernetes", StringComparison.OrdinalIgnoreCase) ||
            service.Contains("Cloud Run",  StringComparison.OrdinalIgnoreCase)))
        {
            await _recRepo.AddAsync(new Recommendation
            {
                GcpAccountId     = gcpAccountId,
                Type             = RecommendationType.VmOptimization,
                Title            = $"Optimisation VM : {service}",
                Description      = $"Migrer vers des instances préemptibles ou Spot pour '{service}' pourrait réduire les coûts de 30%.",
                ResourceName     = service,
                EstimatedSavings = totalCost * 0.30m,
                Currency         = "USD",
                Status           = RecommendationStatus.Pending,
                CreatedAt        = DateTime.UtcNow
            });
        }
    }
}