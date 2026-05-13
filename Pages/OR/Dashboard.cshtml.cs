using Microsoft.AspNetCore.Mvc;
using RetakePortal.Models;
using RetakePortal.Services;

namespace RetakePortal.Pages.OR;

public class DashboardModel : Pages.ORSpecialistPageModel
{
    private readonly ApplicationService _apps;
    public DashboardModel(ApplicationService apps) => _apps = apps;

    public List<Application> Applications { get; set; } = [];
    public List<string> AllDisciplines { get; set; } = [];
    public string StatusFilter { get; set; } = "";
    public string DisciplineFilter { get; set; } = "";
    public int ActionRequiredCount { get; set; }

    public async Task<IActionResult> OnGetAsync(string? status, string? discipline)
    {
        StatusFilter = status ?? "";
        DisciplineFilter = discipline ?? "";

        var all = await _apps.GetAllApplicationsAsync();
        AllDisciplines = await _apps.GetAllDisciplineNamesAsync();

        ActionRequiredCount = all.Count(a => a.Status is "pending" or "director_approved");

        var filtered = string.IsNullOrEmpty(StatusFilter)
            ? all
            : all.Where(a => a.Status == StatusFilter).ToList();

        if (!string.IsNullOrEmpty(DisciplineFilter))
            filtered = filtered.Where(a => a.Items.Any(i => i.DisciplineName == DisciplineFilter)).ToList();

        Applications = filtered;
        return Page();
    }
}
