using Microsoft.AspNetCore.Mvc;
using RetakePortal.Services;

namespace RetakePortal.Pages.Director;

public class StatsModel : RetakePortal.Pages.DirectorPageModel
{
    private readonly AuthService _auth;
    private readonly ApplicationService _apps;
    public StatsModel(AuthService auth, ApplicationService apps)
    {
        _auth = auth;
        _apps = apps;
    }

    public List<SpecialistStat> Stats { get; set; } = [];
    public int PendingDirectorCount { get; set; }

    public async Task OnGetAsync()
    {
        Stats = await _auth.GetSpecialistStatsAsync();
        var all = await _apps.GetAllApplicationsAsync();
        PendingDirectorCount = all.Count(a => a.Status == "pending_director");
    }

    public IActionResult OnPostLogout()
    {
        HttpContext.Session.Clear();
        return RedirectToPage("/Director/Login");
    }
}
