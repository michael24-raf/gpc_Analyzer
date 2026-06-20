using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MonProjetWeb.Application.Common.DTOs.Auth;
using MonProjetWeb.Application.Common.Interfaces;

namespace MonProjetWeb.Web.Pages.Auth;

public class LoginModel : PageModel
{
    private readonly IAuthService _auth;

    public LoginModel(IAuthService auth) => _auth = auth;

    [BindProperty]
    public LoginDto Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var result = await _auth.LoginAsync(Input);

        if (!result.Success)
        {
            ErrorMessage = result.Message;
            return Page();
        }

        // Stocker le token en cookie HttpOnly
        Response.Cookies.Append("jwt", result.Token!, new CookieOptions
        {
            HttpOnly = true,
            Secure   = false, // true en production HTTPS
            SameSite = SameSiteMode.Strict,
            Expires  = result.ExpiresAt
        });

        return RedirectToPage("/Dashboard/Index");
    }
}