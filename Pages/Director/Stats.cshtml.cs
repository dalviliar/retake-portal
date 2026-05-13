using Microsoft.AspNetCore.Mvc;
using RetakePortal.Models;
using RetakePortal.Services;

namespace RetakePortal.Pages.Director;

public class StatsModel : RetakePortal.Pages.DirectorPageModel
{
    private readonly ApplicationService _apps;
    public StatsModel(ApplicationService apps) => _apps = apps;

    public List<Application> Applications { get; set; } = [];
    public int PendingDirectorCount { get; set; }

    public async Task OnGetAsync()
    {
        Applications = await _apps.GetAllApplicationsAsync();
        PendingDirectorCount = Applications.Count(a => a.Status == "pending_director");
    }

    public IActionResult OnPostLogout()
    {
        HttpContext.Session.Clear();
        return RedirectToPage("/Director/Login");
    }
}
