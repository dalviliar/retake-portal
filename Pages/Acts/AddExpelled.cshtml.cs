using Microsoft.AspNetCore.Mvc;
using RetakePortal.Models;
using RetakePortal.Services;

namespace RetakePortal.Pages.Acts;

public class AddExpelledModel : Pages.ActsSpecialistPageModel
{
    private readonly ExpelledStudentService _expelled;
    private readonly FileUploadService _files;

    public AddExpelledModel(ExpelledStudentService expelled, FileUploadService files)
    {
        _expelled = expelled;
        _files = files;
    }

    [BindProperty] public string IIN { get; set; } = string.Empty;
    [BindProperty] public string DisciplineName { get; set; } = string.Empty;
    [BindProperty] public DateTime ExpulsionDate { get; set; } = DateTime.Today;
    public string? ErrorMessage { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(IIN) || IIN.Length != 12 || !IIN.All(char.IsDigit))
        {
            ErrorMessage = "Введите корректный ИИН (12 цифр)";
            return Page();
        }
        if (string.IsNullOrWhiteSpace(DisciplineName))
        {
            ErrorMessage = "Укажите название дисциплины";
            return Page();
        }

        var actFile = Request.Form.Files.GetFile("ActFile");
        var actUrl = await _files.UploadAsync(actFile, "acts");

        await _expelled.AddAsync(new ExpelledStudent
        {
            IIN = IIN.Trim(),
            DisciplineName = DisciplineName.Trim(),
            ExpulsionDate = ExpulsionDate,
            ActDocumentUrl = actUrl,
            AddedBy = SpecialistId
        });

        return RedirectToPage("/Acts/Dashboard");
    }
}
