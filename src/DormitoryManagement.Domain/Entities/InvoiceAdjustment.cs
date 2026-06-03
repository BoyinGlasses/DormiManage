using DormitoryManagement.Domain.Common;
using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Domain.Entities;

public class InvoiceAdjustment : AuditableEntity
{
    public Guid InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public Guid? CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }
}
