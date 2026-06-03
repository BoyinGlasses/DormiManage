using DormitoryManagement.Domain.Common;
using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Domain.Entities;

public class Payment : AuditableEntity
{
    public string PaymentCode { get; set; } = string.Empty;
    public Guid StudentId { get; set; }
    public Student? Student { get; set; }
    public decimal Amount { get; set; }
    public Guid? TargetInvoiceId { get; set; }
    public Invoice? TargetInvoice { get; set; }
    public PaymentMethod Method { get; set; } = PaymentMethod.MockGateway;
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? TransactionRef { get; set; }
    public DateTime? PaidAt { get; set; }
    public ICollection<PaymentAllocation> Allocations { get; set; } = new List<PaymentAllocation>();
}
