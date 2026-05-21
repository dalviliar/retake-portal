namespace RetakePortal.Models;

public class ApplicationItem
{
    public int Id { get; set; }
    public int ApplicationId { get; set; }
    public string DisciplineName { get; set; } = string.Empty;
    public string Grade { get; set; } = string.Empty;
    public int Credits { get; set; }
    public decimal CostPerCredit { get; set; }
    public decimal TotalCost { get; set; }
    public string? ConfirmationDocumentUrl { get; set; }
    public string? PaymentReceiptUrl { get; set; }
    public string DisciplineCode { get; set; } = string.Empty;
    public string ItemStatus { get; set; } = "pending";
    public int? ItemReviewedBy { get; set; }
    public DateTime? ItemReviewedAt { get; set; }
    public string ItemReviewedByName { get; set; } = "";

    public string DisciplineDisplay => string.IsNullOrEmpty(DisciplineCode)
        ? DisciplineName
        : $"{DisciplineCode} — {DisciplineName}";

    public bool HasBrokenReceipt      => PaymentReceiptUrl?.StartsWith("/uploads/") == true;
    public bool HasBrokenConfirmation => ConfirmationDocumentUrl?.StartsWith("/uploads/") == true;
    public bool NeedsReceiptUpload    => Grade == "FX" && (PaymentReceiptUrl == null || HasBrokenReceipt);
    public bool NeedsConfirmUpload    => Grade is "F" or "I" && (ConfirmationDocumentUrl == null || HasBrokenConfirmation);

    public string ItemStatusBadgeClass => ItemStatus switch {
        "approved" => "bg-success",
        "rejected"  => "bg-danger",
        _           => "bg-secondary"
    };
    public string ItemStatusDisplay => ItemStatus switch {
        "approved" => "Одобрено",
        "rejected"  => "Отклонено",
        _           => "Ожидает"
    };
}
