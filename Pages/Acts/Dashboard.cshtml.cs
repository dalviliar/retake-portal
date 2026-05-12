using Microsoft.AspNetCore.Mvc;
using RetakePortal.Models;
using RetakePortal.Services;

namespace RetakePortal.Pages.Acts;

public class DashboardModel : Pages.ActsSpecialistPageModel
{
    private readonly ExpelledStudentService _expelled;
    public DashboardModel(ExpelledStudentService expelled) => _expelled = expelled;

    public List<ExpelledStudent> ExpelledStudents { get; set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        ExpelledStudents = await _expelled.GetAllAsync();
        return Page();
    }
}
