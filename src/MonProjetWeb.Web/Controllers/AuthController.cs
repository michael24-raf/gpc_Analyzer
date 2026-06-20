using Microsoft.AspNetCore.Mvc;
using MonProjetWeb.Application.Common.DTOs.Auth;
using MonProjetWeb.Application.Common.Interfaces;

namespace MonProjetWeb.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    /// <summary>Inscription d'un nouvel utilisateur</summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _auth.RegisterAsync(dto);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>Connexion et obtention du JWT</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _auth.LoginAsync(dto);

        if (!result.Success)
            return Unauthorized(result);

        return Ok(result);
    }
}