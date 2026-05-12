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

    [BindProperty] public string OrUsername { get; set; } = "or_specialist";
    [BindProperty] public string OrPassword { get; set; } = string.Empty;
    [BindProperty] public string OrFullName { get; set; } = string.Empty;
    [BindProperty] public string ActsUsername { get; set; } = "acts_specialist";
    [BindProperty] public string ActsPassword { get; set; } = string.Empty;
    [BindProperty] public string ActsFullName { get; set; } = string.Empty;

    public async Task OnGetAsync()
    {
        AlreadySetup = await _auth.HasAnySpecialistAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        AlreadySetup = await _auth.HasAnySpecialistAsync();
        if (AlreadySetup) return Page();

        if (string.IsNullOrWhiteSpace(OrPassword) || string.IsNullOrWhiteSpace(ActsPassword))
        {
            ErrorMessage = "Пароли не могут быть пустыми";
            return Page();
        }

        await _auth.CreateSpecialistAsync(OrUsername, OrPassword, "or_specialist", OrFullName);
        await _auth.CreateSpecialistAsync(ActsUsername, ActsPassword, "acts_specialist", ActsFullName);
        Success = true;
        return Page();
    }
}
