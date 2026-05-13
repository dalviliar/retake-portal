using Microsoft.AspNetCore.Mvc;
using RetakePortal.Models;
using RetakePortal.Services;
using StudentModel = RetakePortal.Models.Student;

namespace RetakePortal.Pages.Acts;

public class AddExpelledModel : Pages.ActsSpecialistPageModel
{
    private readonly ExpelledStudentService _expelled;
    private readonly SsoService _sso;

    public AddExpelledModel(ExpelledStudentService expelled, SsoService sso)
    {
        _expelled = expelled;
        _sso = sso;
    }

    [BindProperty(SupportsGet = true)] public string? Q { get; set; }
    [BindProperty(SupportsGet = true)] public string? SelectedIIN { get; set; }
    [BindProperty] public string IIN { get; set; } = string.Empty;
    [BindProperty] public DateTime ExpulsionDate { get; set; } = DateTime.Today;
    [BindProperty] public List<string> SelectedDisciplines { get; set; } = [];

    public List<StudentModel> SearchResults { get; set; } = [];
    public StudentModel? FoundStudent { get; set; }
    public List<Grade> AvailableDisciplines { get; set; } = [];
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        if (!string.IsNullOrWhiteSpace(SelectedIIN))
        {
            await LoadStudentDataAsync(SelectedIIN.Trim());
        }
        else if (!string.IsNullOrWhiteSpace(Q))
        {
            var q = Q.Trim();
            // Try exact IIN lookup first; if nothing found, fall back to name search
            StudentModel? byIin = await _sso.GetStudentByIINAsync(q);
            if (byIin != null)
            {
                await LoadStudentDataAsync(q);
            }
            else if (q.Length >= 2)
            {
                SearchResults = await _sso.SearchByNameAsync(q);
                if (!SearchResults.Any())
                    ErrorMessage = "Студент не найден";
            }
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!SelectedDisciplines.Any())
        {
            await LoadStudentDataAsync(IIN);
            ErrorMessage = "Выберите хотя бы одну дисциплину";
            return Page();
        }

        await _expelled.BulkAddAsync(IIN, SelectedDisciplines.ToArray(), ExpulsionDate, null, SpecialistId);
        return RedirectToPage("/Acts/Dashboard");
    }

    private async Task LoadStudentDataAsync(string iin)
    {
        FoundStudent = (StudentModel?)await _sso.GetStudentByIINAsync(iin);
        if (FoundStudent == null) { ErrorMessage = "Студент с таким ИИН не найден"; return; }

        var grades = await _sso.GetAllFailedGradesAsync(iin);
        var alreadyExpelled = await _expelled.GetExpelledDisciplinesAsync(iin);

        AvailableDisciplines = grades
            .Where(g => !alreadyExpelled.Contains(g.DisciplineName))
            .ToList();
    }
}
