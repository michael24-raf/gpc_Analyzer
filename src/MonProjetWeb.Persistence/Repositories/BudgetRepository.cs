using Microsoft.EntityFrameworkCore;
using MonProjetWeb.Application.Common.Interfaces;
using MonProjetWeb.Domain.Entities;
using MonProjetWeb.Persistence.Context;

namespace MonProjetWeb.Persistence.Repositories;

public class BudgetRepository : GenericRepository<Budget>, IBudgetRepository
{
    public BudgetRepository(ApplicationDbContext db) : base(db) { }

    public async Task<IEnumerable<Budget>> GetByAccountAsync(int gcpAccountId)
        => await _dbSet
            .Where(b => b.GcpAccountId == gcpAccountId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<Budget>> GetActiveBudgetsAsync(int gcpAccountId)
        => await _dbSet
            .Where(b => b.GcpAccountId == gcpAccountId
                     && b.PeriodEnd    >= DateTime.UtcNow)
            .ToListAsync();

    public async Task<IEnumerable<Budget>> GetExceededBudgetsAsync(int gcpAccountId)
        => await _dbSet
            .Where(b => b.GcpAccountId == gcpAccountId
                     && b.SpentAmount  >= b.Amount * b.AlertThreshold / 100)
            .ToListAsync();

    public async Task<Budget?> GetWithAlertsAsync(int budgetId)
        => await _dbSet
            .Include(b => b.Alerts)
            .FirstOrDefaultAsync(b => b.Id == budgetId);
}