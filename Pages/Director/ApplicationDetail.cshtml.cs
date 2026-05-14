using Microsoft.AspNetCore.Mvc;
using RetakePortal.Models;
using RetakePortal.Services;

namespace RetakePortal.Pages.Director;

public class DirectorApplicationDetailModel : RetakePortal.Pages.DirectorPageModel
{
    private readonly ApplicationService _apps;
    private readonly SsoService _sso;
    public DirectorApplicationDetailModel(ApplicationService apps, SsoService sso)
    {
        _apps = apps;
        _sso = sso;
    }

    public Application? App { get; set; }
    [BindProperty] public int AppId { get; set; }
    [BindProperty] public string? Reason { get; set; }
    [BindProperty] public string PaymentType { get; set; } = "free";
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

        if (decision == "approved")
        {
            bool requiresDirector = App?.RequiresDirector ?? false;
            string status;
            string? paymentType = null;
            decimal creditCost = 0;

            if (requiresDirector)
            {
                paymentType = PaymentType == "paid" ? "paid" : "free";
                status = paymentType == "paid" ? "pending_payment" : "director_approved";

                if (paymentType == "paid" && App != null)
                {
                    var student = await _sso.GetStudentByIINAsync(App.IIN);
                    creditCost = student?.CreditCost ?? 0;
                }
            }
            else
            {
                status = "director_approved";
            }

            await _apps.DirectorReviewApplicationAsync(AppId, status, null, SpecialistId, paymentType, creditCost);
        }
        else
        {
            await _apps.DirectorReviewApplicationAsync(AppId, "rejected", Reason?.Trim(), SpecialistId);
        }

        return RedirectToPage("/Director/Applications");
    }
}
