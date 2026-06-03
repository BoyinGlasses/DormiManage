namespace DormitoryManagement.Application.DTOs.Dashboard;

public sealed class DashboardInvoiceDto
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public string Student { get; set; } = string.Empty;
    public string DueDate { get; set; } = string.Empty;
    public decimal Remaining { get; set; }
    public string Status { get; set; } = string.Empty;
}
