using Microsoft.AspNetCore.Mvc;
using RetakePortal.Models;
using RetakePortal.Services;

namespace RetakePortal.Pages.OR;

public class ApplicationDetailModel : Pages.ORSpecialistPageModel
{
    private readonly ApplicationService _apps;
    public ApplicationDetailModel(ApplicationService apps) => _apps = apps;

    public Application? App { get; set; }
    [BindProperty] public int AppId { get; set; }
    [BindProperty] public string? Reason { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        App = await _apps.GetApplicationByIdAsync(id);
        if (App != null) AppId = App.Id;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string decision)
    {
        App = await _apps.GetApplicationByIdAsync(AppId);

        if (decision == "rejected" && string.IsNullOrWhiteSpace(Reason))
        {
            ErrorMessage = "При отклонении необходимо указать причину";
            return Page();
        }

        await _apps.ReviewApplicationAsync(AppId, decision, Reason?.Trim(), SpecialistId);
        return RedirectToPage("/OR/Dashboard");
    }
}
