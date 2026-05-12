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
}
