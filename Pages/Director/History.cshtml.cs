using Microsoft.AspNetCore.Mvc;
using RetakePortal.Models;
using RetakePortal.Services;

namespace RetakePortal.Pages.Director;

public class DirectorHistoryModel : Pages.DirectorPageModel
{
    private readonly ApplicationService _apps;
    public DirectorHistoryModel(ApplicationService apps) => _apps = apps;

    public List<Application> Applications { get; set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        Applications = await _apps.GetDirectorHistoryAsync(SpecialistId);
        return Page();
    }
}
