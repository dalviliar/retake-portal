using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RetakePortal.Services;

namespace RetakePortal.Pages.OR;

public class LoginModel : PageModel
{
    private readonly AuthService _auth;
    public LoginModel(AuthService auth) => _auth = auth;

    [BindProperty] public string Username { get; set; } = string.Empty;
    [BindProperty] public string Password { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        var specialist = await _auth.AuthenticateAsync(Username, Password);
        if (specialist == null || specialist.Role != "or_specialist")
        {
            ErrorMessage = "Неверный логин или пароль";
            return Page();
        }

        HttpContext.Session.SetString(Pages.SessionKeys.SpecId, specialist.Id.ToString());
        HttpContext.Session.SetString(Pages.SessionKeys.SpecRole, specialist.Role);
        HttpContext.Session.SetString(Pages.SessionKeys.SpecName, specialist.FullName);
        return RedirectToPage("/OR/Dashboard");
    }
}
