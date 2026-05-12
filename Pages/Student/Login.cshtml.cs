using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RetakePortal.Services;

namespace RetakePortal.Pages.Student;

public class LoginModel : PageModel
{
    private readonly SsoService _sso;
    private readonly IConfiguration _config;

    public LoginModel(SsoService sso, IConfiguration config)
    {
        _sso = sso;
        _config = config;
    }

    [BindProperty]
    public string IIN { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(IIN) || IIN.Length != 12 || !IIN.All(char.IsDigit))
        {
            ErrorMessage = "Введите корректный ИИН (12 цифр)";
            return Page();
        }

        var student = await _sso.GetStudentByIINAsync(IIN);
        if (student == null)
        {
            ErrorMessage = "Студент с данным ИИН не найден в системе";
            return Page();
        }

        HttpContext.Session.SetString(Pages.SessionKeys.StudentIIN, IIN);
        HttpContext.Session.SetString(Pages.SessionKeys.StudentName, student.FullName);
        return RedirectToPage("/Student/Dashboard");
    }
}
