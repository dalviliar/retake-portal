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
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int TotalCount { get; set; }
    private const int PageSize = 50;

    public async Task<IActionResult> OnGetAsync(string? status, string? discipline, int page = 1)
    {
        StatusFilter = status ?? "";
        DisciplineFilter = discipline ?? "";
        CurrentPage = Math.Max(1, page);

        AllDisciplines = await _apps.GetAllDisciplineNamesAsync();
        ActionRequiredCount = await _apps.GetActionRequiredCountAsync();

        var (items, total) = await _apps.GetApplicationsPagedAsync(
            StatusFilter, DisciplineFilter, CurrentPage, PageSize);

        Applications = items;
        TotalCount = total;
        TotalPages = (int)Math.Ceiling((double)total / PageSize);
        if (TotalPages < 1) TotalPages = 1;

        return Page();
    }
}
