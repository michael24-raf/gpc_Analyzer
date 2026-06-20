using Microsoft.EntityFrameworkCore;
using MonProjetWeb.Application.Common.Interfaces;
using MonProjetWeb.Domain.Entities;
using MonProjetWeb.Persistence.Context;

namespace MonProjetWeb.Persistence.Repositories;

public class AlertRepository : GenericRepository<Alert>, IAlertRepository
{
    public AlertRepository(ApplicationDbContext db) : base(db) { }

    public async Task<IEnumerable<Alert>> GetByAccountAsync(int gcpAccountId)
        => await _dbSet
            .Where(a => a.GcpAccountId == gcpAccountId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<Alert>> GetActiveAlertsAsync(int gcpAccountId)
        => await _dbSet
            .Where(a => a.GcpAccountId == gcpAccountId
                     && a.Status       == AlertStatus.Active)
            .OrderByDescending(a => a.Severity)
            .ToListAsync();

    public async Task<IEnumerable<Alert>> GetBySeverityAsync(
        int gcpAccountId, AlertSeverity severity)
        => await _dbSet
            .Where(a => a.GcpAccountId == gcpAccountId
                     && a.Severity     == severity)
            .ToListAsync();

    public async Task<int> GetActiveCountAsync(int gcpAccountId)
        => await _dbSet
            .CountAsync(a => a.GcpAccountId == gcpAccountId
                          && a.Status       == AlertStatus.Active);
}