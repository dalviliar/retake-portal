namespace RetakePortal.Models;

public class ExpelledStudent
{
    public int Id { get; set; }
    public string IIN { get; set; } = string.Empty;
    public string StudentFullName { get; set; } = string.Empty;
    public string DisciplineName { get; set; } = string.Empty;
    public DateTime ExpulsionDate { get; set; }
    public string? ActDocumentUrl { get; set; }
    public int? AddedBy { get; set; }
    public DateTime AddedAt { get; set; }
}
