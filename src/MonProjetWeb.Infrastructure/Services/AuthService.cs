using Microsoft.EntityFrameworkCore;
using MonProjetWeb.Application.Common.DTOs.Auth;
using MonProjetWeb.Application.Common.Interfaces;
using MonProjetWeb.Domain.Entities;
using MonProjetWeb.Domain.Enums;
using MonProjetWeb.Persistence.Context;

namespace MonProjetWeb.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _db;
    private readonly IJwtService          _jwt;

    public AuthService(ApplicationDbContext db, IJwtService jwt)
    {
        _db  = db;
        _jwt = jwt;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        // Vérifier si l'email existe déjà
        var exists = await _db.Users.AnyAsync(u => u.Email == dto.Email);
        if (exists)
            return new AuthResponseDto { Success = false, Message = "Cet email est déjà utilisé." };

        // Créer l'utilisateur
        var user = new User
        {
            FirstName    = dto.FirstName,
            LastName     = dto.LastName,
            Email        = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role         = UserRole.Viewer,
            IsActive     = true,
            CreatedAt    = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var token = _jwt.GenerateToken(user);

        return new AuthResponseDto
        {
            Success   = true,
            Message   = "Inscription réussie.",
            Token     = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60),
            User      = MapToDto(user)
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == dto.Email && u.IsActive);

        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return new AuthResponseDto { Success = false, Message = "Email ou mot de passe incorrect." };

        var token = _jwt.GenerateToken(user);

        return new AuthResponseDto
        {
            Success   = true,
            Message   = "Connexion réussie.",
            Token     = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60),
            User      = MapToDto(user)
        };
    }

    private static UserDto MapToDto(User user) => new()
    {
        Id        = user.Id,
        FirstName = user.FirstName,
        LastName  = user.LastName,
        Email     = user.Email,
        Role      = user.Role
    };
}