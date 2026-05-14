using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RetakePortal.Models;
using RetakePortal.Services;

namespace RetakePortal.Pages.Student;

public class StatusModel : PageModel
{
    private readonly ApplicationService _apps;
    private readonly SsoService _sso;
    private readonly FileUploadService _files;
    private readonly IConfiguration _config;

    public StatusModel(ApplicationService apps, SsoService sso, FileUploadService files, IConfiguration config)
    {
        _apps = apps;
        _sso = sso;
        _files = files;
        _config = config;
    }

    public List<Application> Applications { get; set; } = [];
    public List<RetakeSchedule> RetakeSchedules { get; set; } = [];
    public string? IINInput { get; set; }
    public bool Searched { get; set; }
    public string? PaymentError { get; set; }

    public async Task OnGetAsync(string? iin)
    {
        var sessionIIN = HttpContext.Session.GetString(Pages.SessionKeys.StudentIIN);
        IINInput = iin ?? sessionIIN;

        if (!string.IsNullOrEmpty(IINInput))
        {
            Searched = true;
            Applications = await _apps.GetApplicationsByIINAsync(IINInput);

            if (Applications.Any(a => a.Status is "approved" or "scheduled"))
            {
                var semester = _config["AppSettings:CurrentSemester"] ?? "";
                RetakeSchedules = await _sso.GetRetakeScheduleAsync(IINInput, semester);
            }
        }
    }

    public async Task<IActionResult> OnPostAsync(int appId)
    {
        var sessionIIN = HttpContext.Session.GetString(Pages.SessionKeys.StudentIIN);
        IINInput = sessionIIN;
        Searched = true;

        var app = await _apps.GetApplicationByIdAsync(appId);
        if (app == null || app.IIN != sessionIIN || app.Status != "pending_payment")
            return RedirectToPage(new { iin = sessionIIN });

        bool anyUploaded = false;
        foreach (var item in app.Items)
        {
            var file = Request.Form.Files.GetFile($"receipts_{item.DisciplineName}");
            if (file != null && file.Length > 0)
            {
                var url = await _files.UploadAsync(file, "receipts");
                if (url != null)
                {
                    await _apps.SubmitPaymentReceiptAsync(appId, item.DisciplineName, url);
                    anyUploaded = true;
                }
            }
        }

        if (!anyUploaded)
        {
            Applications = await _apps.GetApplicationsByIINAsync(IINInput!);
            PaymentError = "Прикрепите хотя бы один чек об оплате";
            return Page();
        }

        await _apps.SetPaymentSubmittedAsync(appId);
        return RedirectToPage(new { iin = sessionIIN });
    }
}
