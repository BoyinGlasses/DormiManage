using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Application.DTOs.Payments;

public sealed class PaymentDto
{
    public Guid Id { get; set; }
    public string PaymentCode { get; set; } = string.Empty;
    public Guid StudentId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public Guid? TargetInvoiceId { get; set; }
    public string? TargetInvoiceNumber { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; }
    public string? TransactionRef { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
}
