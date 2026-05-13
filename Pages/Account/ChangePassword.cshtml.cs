using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RetakePortal.Services;

namespace RetakePortal.Pages.Account;

public class ChangePasswordModel : PageModel
{
    private readonly AuthService _auth;
    public ChangePasswordModel(AuthService auth) => _auth = auth;

    [BindProperty] public string CurrentPassword { get; set; } = "";
    [BindProperty] public string NewPassword { get; set; } = "";
    [BindProperty] public string ConfirmPassword { get; set; } = "";
    public string? Message { get; set; }
    public bool Success { get; set; }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext ctx)
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString(SessionKeys.SpecId)))
            ctx.Result = RedirectToPage("/Staff");
        base.OnPageHandlerExecuting(ctx);
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (NewPassword != ConfirmPassword)
        {
            Message = "Новый пароль и подтверждение не совпадают";
            return Page();
        }
        if (NewPassword.Length < 6)
        {
            Message = "Пароль должен быть не менее 6 символов";
            return Page();
        }

        var specId = int.Parse(HttpContext.Session.GetString(SessionKeys.SpecId)!);
        var ok = await _auth.ChangePasswordAsync(specId, CurrentPassword, NewPassword);
        if (!ok)
        {
            Message = "Текущий пароль введён неверно";
            return Page();
        }

        Success = true;
        Message = "Пароль успешно изменён";
        return Page();
    }
}
