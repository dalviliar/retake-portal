using Microsoft.AspNetCore.Mvc;
using RetakePortal.Services;

namespace RetakePortal.Pages.Director;

public class StatsModel : RetakePortal.Pages.DirectorPageModel
{
    private readonly AuthService _auth;
    public StatsModel(AuthService auth) => _auth = auth;

    public List<SpecialistStat> Stats { get; set; } = [];

    public async Task OnGetAsync()
    {
        Stats = await _auth.GetSpecialistStatsAsync();
    }

    public IActionResult OnPostLogout()
    {
        HttpContext.Session.Clear();
        return RedirectToPage("/Director/Login");
    }
}
