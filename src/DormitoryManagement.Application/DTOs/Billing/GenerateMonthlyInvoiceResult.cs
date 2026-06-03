namespace DormitoryManagement.Application.DTOs.Billing;

public sealed class GenerateMonthlyInvoiceResult
{
    public int CreatedCount { get; set; }
    public int SkippedCount { get; set; }
    public int MissingUtilityReadingCount { get; set; }
    public IReadOnlyList<string> Warnings { get; set; } = Array.Empty<string>();
}
