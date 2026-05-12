using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RetakePortal.Services;

namespace RetakePortal.Pages.Admin;

public class ImportModel : PageModel
{
    private readonly ImportService _import;
    private readonly IConfiguration _config;

    public ImportModel(ImportService import, IConfiguration config)
    {
        _import = import;
        _config = config;
    }

    public ImportResult? Result { get; set; }
    public string? SuccessMessage { get; set; }
    public string CurrentSemester => _config["AppSettings:CurrentSemester"] ?? "";

    public void OnGet() { }

    public async Task<IActionResult> OnPostStudentsAsync(IFormFile file)
    {
        Result = await _import.ImportStudentsAsync(file);
        SuccessMessage = "Студенты загружены.";
        return Page();
    }

    public async Task<IActionResult> OnPostGradesAsync(IFormFile file)
    {
        Result = await _import.ImportGradesAsync(file);
        SuccessMessage = "Оценки загружены.";
        return Page();
    }
}
