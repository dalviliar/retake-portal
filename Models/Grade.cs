namespace RetakePortal.Models;

public class Grade
{
    public string DisciplineName { get; set; } = string.Empty;
    public string? GradeValue { get; set; }
    public int Credits { get; set; }
    public string Semester { get; set; } = string.Empty;
    public bool IsExpelled { get; set; }
}
