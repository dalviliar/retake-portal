using Microsoft.AspNetCore.Mvc;
using RetakePortal.Models;
using RetakePortal.Services;

namespace RetakePortal.Pages.Student;

public class DashboardModel : Pages.StudentPageModel
{
    private readonly SsoService _sso;
    private readonly ExpelledStudentService _expelled;
    private readonly ApplicationService _apps;
    private readonly IConfiguration _config;

    public DashboardModel(SsoService sso, ExpelledStudentService expelled,
        ApplicationService apps, IConfiguration config)
    {
        _sso = sso;
        _expelled = expelled;
        _apps = apps;
        _config = config;
    }

    public Models.Student? Student { get; set; }
    public List<Grade> Grades { get; set; } = [];
    public bool HasPendingApplication { get; set; }
    public bool IsExpelledFromAll { get; set; }
    public string CurrentSemester => _config["AppSettings:CurrentSemester"] ?? "";

    public async Task<IActionResult> OnGetAsync()
    {
        Student = await _sso.GetStudentByIINAsync(StudentIIN);
        if (Student == null) return RedirectToPage("/Student/Login");

        var semester = CurrentSemester;
        Grades = await _sso.GetFailedGradesAsync(StudentIIN, semester);
        var expelledDisciplines = await _expelled.GetExpelledDisciplinesAsync(StudentIIN);

        foreach (var g in Grades)
            g.IsExpelled = expelledDisciplines.Contains(g.DisciplineName);

        HasPendingApplication = await _apps.HasPendingApplicationAsync(StudentIIN);
        IsExpelledFromAll = Grades.Any() && Grades.All(g => g.IsExpelled);

        return Page();
    }
}
