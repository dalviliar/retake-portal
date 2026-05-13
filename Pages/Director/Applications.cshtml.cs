using Microsoft.AspNetCore.Mvc;
using RetakePortal.Models;
using RetakePortal.Services;

namespace RetakePortal.Pages.Director;

public class ApplicationsModel : RetakePortal.Pages.DirectorPageModel
{
    private readonly ApplicationService _apps;
    private readonly AuthService _auth;
    public ApplicationsModel(ApplicationService apps, AuthService auth)
    {
        _apps = apps;
        _auth = auth;
    }

    public List<Application> Applications { get; set; } = [];

    public async Task OnGetAsync()
    {
        Applications = await _apps.GetApplicationsForDirectorAsync();
    }

    public IActionResult OnPostLogout()
    {
        HttpContext.Session.Clear();
        return RedirectToPage("/Director/Login");
    }
}
