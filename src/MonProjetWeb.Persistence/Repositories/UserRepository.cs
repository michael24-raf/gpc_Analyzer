using Microsoft.EntityFrameworkCore;
using MonProjetWeb.Application.Common.Interfaces;
using MonProjetWeb.Domain.Entities;
using MonProjetWeb.Persistence.Context;

namespace MonProjetWeb.Persistence.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext db) : base(db) { }

    public async Task<User?> GetByEmailAsync(string email)
        => await _dbSet
            .FirstOrDefaultAsync(u => u.Email == email);

    public async Task<User?> GetWithAccountsAsync(int userId)
        => await _dbSet
            .Include(u => u.GcpAccounts)
            .FirstOrDefaultAsync(u => u.Id == userId);

    public async Task<IEnumerable<User>> GetActiveUsersAsync()
        => await _dbSet
            .Where(u => u.IsActive)
            .ToListAsync();
}