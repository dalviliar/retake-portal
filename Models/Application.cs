namespace RetakePortal.Models;

public class Application
{
    public int Id { get; set; }
    public string IIN { get; set; } = string.Empty;
    public string StudentFullName { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public string Institute { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public int Course { get; set; }
    public string EducationLevel { get; set; } = string.Empty;
    public string Status { get; set; } = "pending";
    public string? RejectionReason { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public int? ReviewedBy { get; set; }
    public bool ExpulsionConflict { get; set; }
    public DateTime? DirectorReviewedAt { get; set; }
    public int? DirectorReviewedBy { get; set; }
    public string? DirectorName { get; set; }

    public List<ApplicationItem> Items { get; set; } = [];

    public bool RequiresDirector => Items.Any(i => i.Grade is "F" or "I");

    public string StatusDisplay => Status switch
    {
        "pending"           => "На рассмотрении",
        "pending_director"  => "У директора",
        "director_approved" => "Одобрено директором",
        "approved"          => "Одобрено",
        "rejected"          => "Отклонено",
        _                   => Status
    };

    public string StatusBadgeClass => Status switch
    {
        "pending"           => "bg-warning text-dark",
        "pending_director"  => "bg-info text-dark",
        "director_approved" => "bg-primary",
        "approved"          => "bg-success",
        "rejected"          => "bg-danger",
        _                   => "bg-secondary"
    };

    public string EducationLevelDisplay => EducationLevel switch
    {
        "bachelor"    => "Бакалавриат",
        "master_sci"  => "Магистратура (научно-педагогическая)",
        "master_prof" => "Магистратура (профильная)",
        "doctoral"    => "Докторантура",
        _             => EducationLevel
    };
}
