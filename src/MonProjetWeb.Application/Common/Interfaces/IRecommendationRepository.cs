using MonProjetWeb.Domain.Entities;

namespace MonProjetWeb.Application.Common.Interfaces;

public interface IRecommendationRepository : IGenericRepository<Recommendation>
{
    Task<IEnumerable<Recommendation>> GetByAccountAsync(int gcpAccountId);
    Task<IEnumerable<Recommendation>> GetPendingAsync(int gcpAccountId);
    Task<decimal> GetTotalPotentialSavingsAsync(int gcpAccountId);
    Task<IEnumerable<Recommendation>> GetByTypeAsync(int gcpAccountId, RecommendationType type);
}