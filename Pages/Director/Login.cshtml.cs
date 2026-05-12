using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RetakePortal.Pages;
using RetakePortal.Services;

namespace RetakePortal.Pages.Director;

public class DirectorLoginModel : PageModel
{
    private readonly AuthService _auth;
    public DirectorLoginModel(AuthService auth) => _auth = auth;

    [BindProperty] public string Username { get; set; } = string.Empty;
    [BindProperty] public string Password { get; set; } = string.Empty;
    public string? Error { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        var specialist = await _auth.AuthenticateAsync(Username, Password);
        if (specialist == null || specialist.Role != "director")
        {
            Error = "Неверный логин или пароль";
            return Page();
        }
        HttpContext.Session.SetString(SessionKeys.SpecId, specialist.Id.ToString());
        HttpContext.Session.SetString(SessionKeys.SpecRole, specialist.Role);
        HttpContext.Session.SetString(SessionKeys.SpecName, specialist.FullName);
        return RedirectToPage("/Director/Stats");
    }
}
