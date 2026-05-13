using Microsoft.AspNetCore.Mvc;
using RetakePortal.Models;
using RetakePortal.Services;

namespace RetakePortal.Pages.Admin;

public class SpecialistsModel : RetakePortal.Pages.AdminPageModel
{
    private readonly AuthService _auth;
    public SpecialistsModel(AuthService auth) => _auth = auth;

    public List<Specialist> Specialists { get; set; } = [];
    public string? Message { get; set; }

    [BindProperty] public string NewRole { get; set; } = "or_specialist";
    [BindProperty] public string NewUsername { get; set; } = string.Empty;
    [BindProperty] public string NewPassword { get; set; } = string.Empty;
    [BindProperty] public string NewFullName { get; set; } = string.Empty;

    public async Task OnGetAsync()
    {
        Specialists = await _auth.GetAllSpecialistsAsync();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        await _auth.CreateSpecialistAsync(NewUsername, NewPassword, NewRole, NewFullName);
        Message = $"Специалист {NewFullName} создан.";
        Specialists = await _auth.GetAllSpecialistsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        await _auth.DeleteSpecialistAsync(id);
        Specialists = await _auth.GetAllSpecialistsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostResetPasswordAsync(int id, string newPassword)
    {
        if (!string.IsNullOrWhiteSpace(newPassword) && newPassword.Length >= 6)
        {
            await _auth.ResetPasswordAsync(id, newPassword);
            Message = "Пароль сброшен.";
        }
        Specialists = await _auth.GetAllSpecialistsAsync();
        return Page();
    }

    public IActionResult OnPostLogout()
    {
        HttpContext.Session.Clear();
        return RedirectToPage("/Admin/Login");
    }
}
