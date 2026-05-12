using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RetakePortal.Models;
using RetakePortal.Services;

namespace RetakePortal.Pages.Student;

public class StatusModel : PageModel
{
    private readonly ApplicationService _apps;

    public StatusModel(ApplicationService apps) => _apps = apps;

    public List<Application> Applications { get; set; } = [];
    public string? IINInput { get; set; }
    public bool Searched { get; set; }

    public async Task OnGetAsync(string? iin)
    {
        // If logged in as student, use session IIN by default
        var sessionIIN = HttpContext.Session.GetString(Pages.SessionKeys.StudentIIN);
        IINInput = iin ?? sessionIIN;

        if (!string.IsNullOrEmpty(IINInput))
        {
            Searched = true;
            Applications = await _apps.GetApplicationsByIINAsync(IINInput);
        }
    }
}
