namespace RetakePortal.Models;

public class RetakeSchedule
{
    public string DisciplineName { get; set; } = string.Empty;
    public DateTime? ExamDate { get; set; }
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }
    public string? Room { get; set; }
    public string? Building { get; set; }
}
