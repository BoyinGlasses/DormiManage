using DormitoryManagement.Domain.Common;
using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Domain.Entities;

public class PaymentExtension : AuditableEntity
{
    public Guid InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }
    public Guid StudentId { get; set; }
    public Student? Student { get; set; }
    public DateTime RequestedDueDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public PaymentExtensionStatus Status { get; set; } = PaymentExtensionStatus.Pending;
    public Guid? ReviewedByUserId { get; set; }
    public User? ReviewedByUser { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? RejectionReason { get; set; }
}
