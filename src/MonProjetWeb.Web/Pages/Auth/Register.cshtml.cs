using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MonProjetWeb.Application.Common.DTOs.Auth;
using MonProjetWeb.Application.Common.Interfaces;

namespace MonProjetWeb.Web.Pages.Auth;

public class RegisterModel : PageModel
{
    private readonly IAuthService _auth;

    public RegisterModel(IAuthService auth) => _auth = auth;

    [BindProperty]
    public RegisterDto Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var result = await _auth.RegisterAsync(Input);

        if (!result.Success)
        {
            ErrorMessage = result.Message;
            return Page();
        }

        Response.Cookies.Append("jwt", result.Token!, new CookieOptions
        {
            HttpOnly = true,
            Secure   = false,
            SameSite = SameSiteMode.Strict,
            Expires  = result.ExpiresAt
        });

        return RedirectToPage("/Dashboard/Index");
    }
}