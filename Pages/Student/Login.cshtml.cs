using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RetakePortal.Services;

namespace RetakePortal.Pages.Student;

public class LoginModel : PageModel
{
    private readonly SsoService _sso;
    private readonly ApplicationService _apps;
    private readonly IConfiguration _config;

    public LoginModel(SsoService sso, ApplicationService apps, IConfiguration config)
    {
        _sso = sso;
        _apps = apps;
        _config = config;
    }

    [BindProperty]
    public string IIN { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public bool HasExistingApplication { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(IIN))
        {
            ErrorMessage = "Введите ИИН";
            return Page();
        }

        var student = await _sso.GetStudentByIINAsync(IIN);
        if (student == null)
        {
            var existing = await _apps.GetApplicationsByIINAsync(IIN);
            if (existing.Any())
            {
                HasExistingApplication = true;
            }
            else
            {
                ErrorMessage = "Студент с данным ИИН не найден в системе";
            }
            return Page();
        }

        HttpContext.Session.SetString(Pages.SessionKeys.StudentIIN, IIN);
        HttpContext.Session.SetString(Pages.SessionKeys.StudentName, student.FullName);
        return RedirectToPage("/Student/Dashboard");
    }
}
