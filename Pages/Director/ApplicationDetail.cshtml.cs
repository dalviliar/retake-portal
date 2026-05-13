using Microsoft.AspNetCore.Mvc;
using RetakePortal.Models;
using RetakePortal.Services;

namespace RetakePortal.Pages.Director;

public class DirectorApplicationDetailModel : RetakePortal.Pages.DirectorPageModel
{
    private readonly ApplicationService _apps;
    public DirectorApplicationDetailModel(ApplicationService apps) => _apps = apps;

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

        var status = decision == "approved" ? "director_approved" : "rejected";
        await _apps.DirectorReviewApplicationAsync(AppId, status, Reason?.Trim(), SpecialistId);
        return RedirectToPage("/Director/Applications");
    }
}
