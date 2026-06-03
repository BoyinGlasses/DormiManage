using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Application.DTOs.Payments;

public sealed class PaymentExtensionDto
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid StudentId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public DateTime RequestedDueDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public PaymentExtensionStatus Status { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? RejectionReason { get; set; }
}
