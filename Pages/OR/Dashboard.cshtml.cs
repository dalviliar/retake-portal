using Microsoft.AspNetCore.Mvc;
using RetakePortal.Models;
using RetakePortal.Services;

namespace RetakePortal.Pages.OR;

public class DashboardModel : Pages.ORSpecialistPageModel
{
    private readonly ApplicationService _apps;
    public DashboardModel(ApplicationService apps) => _apps = apps;

    public List<Application> Applications { get; set; } = [];
    public List<DisciplineInfo> AllDisciplines { get; set; } = [];
    public string StatusFilter { get; set; } = "";
    public List<string> DisciplineFilters { get; set; } = [];
    public List<string> DeptFilters { get; set; } = [];
    public int ActionRequiredCount { get; set; }
    public int ExpulsionConflictCount { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int TotalCount { get; set; }
    private const int PageSize = 50;

    public async Task<IActionResult> OnGetAsync(string? status, List<string>? disciplines, List<string>? depts, int p = 1)
    {
        StatusFilter = status ?? "";
        DisciplineFilters = disciplines ?? [];
        DeptFilters = depts ?? [];
        CurrentPage = Math.Max(1, p);

        AllDisciplines = await _apps.GetAllDisciplinesAsync();
        ActionRequiredCount = await _apps.GetActionRequiredCountAsync();
        ExpulsionConflictCount = await _apps.GetExpulsionConflictCountAsync();

        var (items, total) = await _apps.GetApplicationsPagedAsync(
            StatusFilter, DisciplineFilters.ToArray(), CurrentPage, PageSize);

        Applications = items;
        TotalCount = total;
        TotalPages = (int)Math.Ceiling((double)total / PageSize);
        if (TotalPages < 1) TotalPages = 1;

        return Page();
    }
}
