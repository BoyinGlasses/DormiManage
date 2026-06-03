using DormitoryManagement.Domain.Common;
using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Domain.Entities;

public class InvoiceItem : AuditableEntity
{
    public Guid InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }
    public Guid? FeeTypeId { get; set; }
    public FeeType? FeeType { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; } = 1m;
    public decimal UnitPrice { get; set; }
    public decimal Amount { get; set; }
}
