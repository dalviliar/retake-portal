using Microsoft.AspNetCore.Mvc;
using RetakePortal.Models;
using RetakePortal.Services;

namespace RetakePortal.Pages.OR;

public class DashboardModel : Pages.ORSpecialistPageModel
{
    private readonly ApplicationService _apps;
    public DashboardModel(ApplicationService apps) => _apps = apps;

    public List<Application> Applications { get; set; } = [];
    public string StatusFilter { get; set; } = "";
    public int PendingCount { get; set; }

    public async Task<IActionResult> OnGetAsync(string? status)
    {
        StatusFilter = status ?? "";
        var all = await _apps.GetAllApplicationsAsync();
        PendingCount = all.Count(a => a.Status == "pending");
        Applications = string.IsNullOrEmpty(StatusFilter)
            ? all
            : all.Where(a => a.Status == StatusFilter).ToList();
        return Page();
    }
}
