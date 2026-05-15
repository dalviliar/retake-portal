using RetakePortal.Models;
using RetakePortal.Services;

namespace RetakePortal.Pages.Director;

public class AnalyticsModel : RetakePortal.Pages.DirectorPageModel
{
    private readonly ApplicationService _apps;
    public AnalyticsModel(ApplicationService apps) => _apps = apps;

    public int Total           { get; set; }
    public int PendingDirector { get; set; }
    public int Approved        { get; set; }
    public int Rejected        { get; set; }
    public int PaidCount       { get; set; }
    public int FreeCount       { get; set; }

    public Dictionary<string, int> ByStatus    { get; set; } = [];
    public Dictionary<string, int> ByGrade     { get; set; } = [];
    public Dictionary<string, int> ByInstitute { get; set; } = [];
    public Dictionary<string, int> ByCourse    { get; set; } = [];
    public Dictionary<string, int> ByDay       { get; set; } = [];
    public List<(string Discipline, int Count)> TopDisciplines { get; set; } = [];

    public async Task OnGetAsync()
    {
        var apps = await _apps.GetAllApplicationsAsync();

        Total           = apps.Count;
        PendingDirector = apps.Count(a => a.Status == "pending_director");
        Approved        = apps.Count(a => a.Status is "approved" or "director_approved" or "scheduled");
        Rejected        = apps.Count(a => a.Status == "rejected");
        PaidCount       = apps.Count(a => a.PaymentType == "paid");
        FreeCount       = apps.Count(a => a.PaymentType == "free");

        var statusLabels = new Dictionary<string, string>
        {
            ["pending"]          = "На рассмотрении ОР",
            ["pending_director"] = "У директора",
            ["director_approved"]= "Одобрено директором",
            ["pending_payment"]  = "Ожидает оплаты",
            ["payment_submitted"]= "Оплата на проверке",
            ["approved"]         = "Одобрено",
            ["rejected"]         = "Отклонено",
            ["scheduled"]        = "Назначено"
        };

        ByStatus = apps
            .GroupBy(a => statusLabels.GetValueOrDefault(a.Status, a.Status))
            .OrderByDescending(g => g.Count())
            .ToDictionary(g => g.Key, g => g.Count());

        ByGrade = apps
            .SelectMany(a => a.Items)
            .GroupBy(i => i.Grade)
            .OrderByDescending(g => g.Count())
            .ToDictionary(g => g.Key, g => g.Count());

        ByInstitute = apps
            .GroupBy(a => a.Institute)
            .OrderByDescending(g => g.Count())
            .Take(10)
            .ToDictionary(g => g.Key, g => g.Count());

        ByCourse = apps
            .GroupBy(a => a.Course.ToString())
            .OrderBy(g => g.Key)
            .ToDictionary(g => g.Key + " курс", g => g.Count());

        ByDay = apps
            .GroupBy(a => a.SubmittedAt.AddHours(5).ToString("dd.MM"))
            .OrderBy(g => g.Key)
            .ToDictionary(g => g.Key, g => g.Count());

        TopDisciplines = apps
            .SelectMany(a => a.Items)
            .GroupBy(i => i.DisciplineName)
            .OrderByDescending(g => g.Count())
            .Take(10)
            .Select(g => (g.Key, g.Count()))
            .ToList();
    }
}
