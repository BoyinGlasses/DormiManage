using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Application.DTOs.Billing;

public sealed class StudentBillingRowDto
{
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public InvoiceKind InvoiceKind { get; set; }
    public string Name { get; set; } = string.Empty;
    public string RoomNumber { get; set; } = string.Empty;
    public string BillingPeriod { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public DateTime DueDate { get; set; }
    public InvoiceStatus Status { get; set; }
    public bool CanShowExtendAction { get; set; }
    public bool CanExtend { get; set; }
    public bool CanPay { get; set; } = true;
}
