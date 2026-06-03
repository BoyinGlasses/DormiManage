using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Application.DTOs.Payments;

public sealed class PaymentNavigationContextDto
{
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public InvoiceKind InvoiceKind { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string BillingPeriod { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
}
