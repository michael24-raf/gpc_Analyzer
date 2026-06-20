using MonProjetWeb.Application.Common.DTOs.Auth;

namespace MonProjetWeb.Application.Common.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
}