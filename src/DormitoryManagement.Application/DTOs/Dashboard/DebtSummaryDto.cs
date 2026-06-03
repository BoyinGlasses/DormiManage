namespace DormitoryManagement.Application.DTOs.Dashboard;

public sealed class DebtSummaryDto
{
    public decimal TotalDebt { get; set; }
    public int StudentCount { get; set; }
    public int UnpaidInvoiceCount { get; set; }
    public int OverdueInvoiceCount { get; set; }
}
