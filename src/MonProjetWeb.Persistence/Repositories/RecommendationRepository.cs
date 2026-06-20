using Microsoft.EntityFrameworkCore;
using MonProjetWeb.Application.Common.Interfaces;
using MonProjetWeb.Domain.Entities;
using MonProjetWeb.Persistence.Context;

namespace MonProjetWeb.Persistence.Repositories;

public class RecommendationRepository : GenericRepository<Recommendation>, IRecommendationRepository
{
    public RecommendationRepository(ApplicationDbContext db) : base(db) { }

    public async Task<IEnumerable<Recommendation>> GetByAccountAsync(int gcpAccountId)
        => await _dbSet
            .Where(r => r.GcpAccountId == gcpAccountId)
            .OrderByDescending(r => r.EstimatedSavings)
            .ToListAsync();

    public async Task<IEnumerable<Recommendation>> GetPendingAsync(int gcpAccountId)
        => await _dbSet
            .Where(r => r.GcpAccountId == gcpAccountId
                     && r.Status       == RecommendationStatus.Pending)
            .OrderByDescending(r => r.EstimatedSavings)
            .ToListAsync();

    public async Task<decimal> GetTotalPotentialSavingsAsync(int gcpAccountId)
        => await _dbSet
            .Where(r => r.GcpAccountId == gcpAccountId
                     && r.Status       == RecommendationStatus.Pending)
            .SumAsync(r => r.EstimatedSavings);

    public async Task<IEnumerable<Recommendation>> GetByTypeAsync(
        int gcpAccountId, RecommendationType type)
        => await _dbSet
            .Where(r => r.GcpAccountId == gcpAccountId
                     && r.Type         == type)
            .ToListAsync();
}