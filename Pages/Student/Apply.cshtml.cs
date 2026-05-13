using Microsoft.AspNetCore.Mvc;
using RetakePortal.Models;
using RetakePortal.Services;
using System.Text.Json;

namespace RetakePortal.Pages.Student;

public class ApplyModel : Pages.StudentPageModel
{
    private readonly SsoService _sso;
    private readonly ExpelledStudentService _expelled;
    private readonly ApplicationService _apps;
    private readonly FileUploadService _files;
    private readonly IConfiguration _config;

    public ApplyModel(SsoService sso, ExpelledStudentService expelled,
        ApplicationService apps, FileUploadService files, IConfiguration config)
    {
        _sso = sso;
        _expelled = expelled;
        _apps = apps;
        _files = files;
        _config = config;
    }

    public List<Grade> AvailableGrades { get; set; } = [];
    public decimal CreditCost { get; set; }
    public string CostsJson => JsonSerializer.Serialize(
        AvailableGrades.Select(g => g.GradeValue == "FX" ? CreditCost : 0m));
    public string GradesJson => JsonSerializer.Serialize(AvailableGrades.Select(g => g.GradeValue));
    public string CreditsJson => JsonSerializer.Serialize(AvailableGrades.Select(g => g.Credits));
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadGradesAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadGradesAsync();

        var selectedRaw = Request.Form["SelectedIndices"].ToArray();
        if (selectedRaw.Length == 0)
        {
            ErrorMessage = "Выберите хотя бы одну дисциплину";
            return Page();
        }

        var disciplineNames = Request.Form["DisciplineNames"].ToArray();
        var gradeValues = Request.Form["GradeValues"].ToArray();
        var creditsArr = Request.Form["Credits"].ToArray();

        var selectedIndices = selectedRaw.Select(s => int.Parse(s!)).ToList();
        var student = await _sso.GetStudentByIINAsync(StudentIIN);
        if (student == null) return RedirectToPage("/Student/Login");

        // FX = 1 credit cost; F and I = free
        decimal total = selectedIndices.Sum(i =>
            (gradeValues[i] ?? "FX") == "FX" ? student.CreditCost : 0m);

        var app = new Application
        {
            IIN = StudentIIN,
            StudentFullName = student.FullName,
            Specialty = student.Specialty,
            Institute = student.Institute,
            Department = student.Department,
            Course = student.Course,
            EducationLevel = student.EducationLevel,
            TotalAmount = total
        };

        int appId = await _apps.CreateApplicationAsync(app);

        foreach (var i in selectedIndices)
        {
            var grade = gradeValues[i] ?? "FX";
            var credits = int.Parse(creditsArr[i]!);
            // FX = fixed 1 credit cost; F and I = free
            var costPer = grade == "FX" ? student.CreditCost : 0m;
            var totalCost = costPer;

            string? confUrl = null;
            string? receiptUrl = null;

            if (grade is "F" or "I")
            {
                var confFile = Request.Form.Files.GetFile($"confDoc_{i}");
                confUrl = await _files.UploadAsync(confFile, "confirmation");
                if (confUrl == null)
                {
                    ErrorMessage = $"Для оценки {grade} необходим подтверждающий документ (справка)";
                    return Page();
                }
            }
            else // FX
            {
                var receiptFile = Request.Form.Files.GetFile($"payReceipt_{i}");
                receiptUrl = await _files.UploadAsync(receiptFile, "receipts");
                if (receiptUrl == null)
                {
                    ErrorMessage = "Прикрепите чек об оплате";
                    return Page();
                }
            }

            await _apps.AddApplicationItemAsync(new ApplicationItem
            {
                ApplicationId = appId,
                DisciplineName = disciplineNames[i] ?? string.Empty,
                Grade = grade,
                Credits = credits,
                CostPerCredit = costPer,
                TotalCost = totalCost,
                ConfirmationDocumentUrl = confUrl,
                PaymentReceiptUrl = receiptUrl
            });
        }

        return RedirectToPage("/Student/Status");
    }

    private async Task LoadGradesAsync()
    {
        var student = await _sso.GetStudentByIINAsync(StudentIIN);
        if (student == null) return;
        CreditCost = student.CreditCost;

        var semester = _config["AppSettings:CurrentSemester"] ?? "";
        var grades = await _sso.GetFailedGradesAsync(StudentIIN, semester);
        var expelled = await _expelled.GetExpelledDisciplinesAsync(StudentIIN);
        var applied = await _apps.GetAppliedDisciplinesAsync(StudentIIN);
        AvailableGrades = grades
            .Where(g => !expelled.Contains(g.DisciplineName) && !applied.Contains(g.DisciplineName))
            .ToList();
    }
}
