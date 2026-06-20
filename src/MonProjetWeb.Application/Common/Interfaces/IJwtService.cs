using MonProjetWeb.Domain.Entities;

namespace MonProjetWeb.Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
}