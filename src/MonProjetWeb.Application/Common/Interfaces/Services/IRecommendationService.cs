using MonProjetWeb.Domain.Entities;

namespace MonProjetWeb.Application.Common.Interfaces.Services;

public interface IRecommendationService
{
    Task<IEnumerable<Recommendation>> GetRecommendationsAsync(int gcpAccountId);
    Task<IEnumerable<Recommendation>> GetPendingRecommendationsAsync(int gcpAccountId);
    Task<decimal> GetTotalPotentialSavingsAsync(int gcpAccountId);
    Task GenerateRecommendationsAsync(int gcpAccountId);
    Task ApplyRecommendationAsync(int recommendationId);
    Task DismissRecommendationAsync(int recommendationId);
}