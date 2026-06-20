using MonProjetWeb.Domain.Entities;

namespace MonProjetWeb.Application.Common.Interfaces;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetWithAccountsAsync(int userId);
    Task<IEnumerable<User>> GetActiveUsersAsync();
}