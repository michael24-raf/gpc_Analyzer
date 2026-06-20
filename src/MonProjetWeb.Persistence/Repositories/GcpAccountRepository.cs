using Microsoft.EntityFrameworkCore;
using MonProjetWeb.Application.Common.Interfaces;
using MonProjetWeb.Domain.Entities;
using MonProjetWeb.Persistence.Context;

namespace MonProjetWeb.Persistence.Repositories;

public class GcpAccountRepository : GenericRepository<GcpAccount>, IGcpAccountRepository
{
    public GcpAccountRepository(ApplicationDbContext db) : base(db) { }

    public async Task<IEnumerable<GcpAccount>> GetByUserAsync(int userId)
        => await _dbSet
            .Where(a => a.UserId == userId && a.IsActive)
            .ToListAsync();

    public async Task<GcpAccount?> GetWithDetailsAsync(int accountId)
        => await _dbSet
            .Include(a => a.CostRecords)
            .Include(a => a.Budgets)
            .Include(a => a.Alerts)
            .Include(a => a.Recommendations)
            .FirstOrDefaultAsync(a => a.Id == accountId);

    public async Task<GcpAccount?> GetByProjectIdAsync(string projectId)
        => await _dbSet
            .FirstOrDefaultAsync(a => a.ProjectId == projectId);
}