using Microsoft.AspNetCore.Mvc.RazorPages;
using RetakePortal.Models;
using RetakePortal.Services;

namespace RetakePortal.Pages.Student;

public class StatusModel : PageModel
{
    private readonly ApplicationService _apps;
    private readonly SsoService _sso;
    private readonly IConfiguration _config;

    public StatusModel(ApplicationService apps, SsoService sso, IConfiguration config)
    {
        _apps = apps;
        _sso = sso;
        _config = config;
    }

    public List<Application> Applications { get; set; } = [];
    public List<RetakeSchedule> RetakeSchedules { get; set; } = [];
    public string? IINInput { get; set; }
    public bool Searched { get; set; }

    public async Task OnGetAsync(string? iin)
    {
        var sessionIIN = HttpContext.Session.GetString(Pages.SessionKeys.StudentIIN);
        IINInput = iin ?? sessionIIN;

        if (!string.IsNullOrEmpty(IINInput))
        {
            Searched = true;
            Applications = await _apps.GetApplicationsByIINAsync(IINInput);

            if (Applications.Any(a => a.Status == "approved"))
            {
                var semester = _config["AppSettings:CurrentSemester"] ?? "";
                RetakeSchedules = await _sso.GetRetakeScheduleAsync(IINInput, semester);
            }
        }
    }
}
