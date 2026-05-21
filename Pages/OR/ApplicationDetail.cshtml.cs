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
    [BindProperty] public int ItemId { get; set; }
    [BindProperty] public string? Reason { get; set; }
    [BindProperty(SupportsGet = true)] public string? ReturnUrl { get; set; }
    public string? ErrorMessage { get; set; }

    private IActionResult BackToDashboard() =>
        IsValidReturn(ReturnUrl) ? Redirect(ReturnUrl!) : RedirectToPage("/OR/Dashboard");

    private static bool IsValidReturn(string? url) =>
        !string.IsNullOrEmpty(url) && url.StartsWith("/OR/Dashboard");

    public async Task<IActionResult> OnGetAsync(int id)
    {
        App = await _apps.GetApplicationByIdAsync(id);
        if (App != null) AppId = App.Id;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string decision)
    {
        App = await _apps.GetApplicationByIdAsync(AppId);

        if (decision == "approve_item")
        {
            await _apps.ApproveItemAsync(AppId, ItemId, SpecialistId);
            return RedirectToPage("/OR/ApplicationDetail", new { id = AppId, returnUrl = ReturnUrl });
        }

        if (decision == "reject_item")
        {
            if (string.IsNullOrWhiteSpace(Reason))
            {
                App = await _apps.GetApplicationByIdAsync(AppId);
                ErrorMessage = "При отклонении необходимо указать причину";
                return Page();
            }
            await _apps.RejectItemAsync(AppId, ItemId, Reason?.Trim(), SpecialistId);
            return BackToDashboard();
        }

        if (decision == "schedule")
        {
            await _apps.ScheduleRetakeAsync(AppId, SpecialistId);
            return RedirectToPage("/OR/ApplicationDetail", new { id = AppId, returnUrl = ReturnUrl });
        }

        if (decision == "verify_payment")
        {
            await _apps.ReviewApplicationAsync(AppId, "approved", null, SpecialistId);
            await _apps.ScheduleRetakeAsync(AppId, SpecialistId);
            return RedirectToPage("/OR/ApplicationDetail", new { id = AppId, returnUrl = ReturnUrl });
        }

        if (decision == "rejected" && string.IsNullOrWhiteSpace(Reason))
        {
            ErrorMessage = "При отклонении необходимо указать причину";
            return Page();
        }

        if (decision == "rejected" && App?.Status == "approved")
        {
            await _apps.RejectApprovedApplicationAsync(AppId, "Акт нарушения", SpecialistId);
            return BackToDashboard();
        }

        var status = decision switch
        {
            "send_to_director" => "pending_director",
            "approved"         => "approved",
            "rejected"         => "rejected",
            _                  => "rejected"
        };

        await _apps.ReviewApplicationAsync(AppId, status, Reason?.Trim(), SpecialistId);
        return BackToDashboard();
    }
}
