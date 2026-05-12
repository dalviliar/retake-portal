using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RetakePortal.Services;

namespace RetakePortal.Pages.Admin;

public class SetupModel : PageModel
{
    private readonly AuthService _auth;
    public SetupModel(AuthService auth) => _auth = auth;

    public bool AlreadySetup { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }

    [BindProperty] public string Username { get; set; } = "admin";
    [BindProperty] public string Password { get; set; } = string.Empty;
    [BindProperty] public string FullName { get; set; } = string.Empty;

    public async Task OnGetAsync()
    {
        AlreadySetup = await _auth.HasAnySpecialistAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        AlreadySetup = await _auth.HasAnySpecialistAsync();
        if (AlreadySetup) return Page();

        if (string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Пароль не может быть пустым";
            return Page();
        }

        await _auth.CreateSpecialistAsync(Username, Password, "admin", FullName);
        Success = true;
        return Page();
    }
}
