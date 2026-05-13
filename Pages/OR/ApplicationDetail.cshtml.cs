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

        if (decision == "schedule")
        {
            await _apps.ScheduleRetakeAsync(AppId, SpecialistId);
            return RedirectToPage("/OR/ApplicationDetail", new { id = AppId });
        }

        if (decision == "rejected" && string.IsNullOrWhiteSpace(Reason))
        {
            ErrorMessage = "При отклонении необходимо указать причину";
            return Page();
        }

        if (decision == "rejected" && App?.Status == "approved")
        {
            await _apps.RejectApprovedApplicationAsync(AppId, Reason?.Trim(), SpecialistId);
            return RedirectToPage("/OR/Dashboard");
        }

        var status = decision switch
        {
            "send_to_director" => "pending_director",
            "approved"         => "approved",
            "rejected"         => "rejected",
            _                  => "rejected"
        };

        await _apps.ReviewApplicationAsync(AppId, status, Reason?.Trim(), SpecialistId);
        return RedirectToPage("/OR/Dashboard");
    }
}
