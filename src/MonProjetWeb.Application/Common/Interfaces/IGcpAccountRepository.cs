using MonProjetWeb.Domain.Entities;

namespace MonProjetWeb.Application.Common.Interfaces;

public interface IGcpAccountRepository : IGenericRepository<GcpAccount>
{
    Task<IEnumerable<GcpAccount>> GetByUserAsync(int userId);
    Task<GcpAccount?> GetWithDetailsAsync(int accountId);
    Task<GcpAccount?> GetByProjectIdAsync(string projectId);
}