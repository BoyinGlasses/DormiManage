using DormitoryManagement.Domain.Common;
using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Domain.Entities;

public class Invoice : AuditableEntity
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid StudentId { get; set; }
    public Student? Student { get; set; }
    public Guid RoomId { get; set; }
    public Room? Room { get; set; }
    public string BillingPeriod { get; set; } = string.Empty;
    public InvoiceKind InvoiceKind { get; set; } = InvoiceKind.MonthlyUtility;
    public Guid? ContractId { get; set; }
    public Contract? Contract { get; set; }
    public DateTime IssueDate { get; set; } = DateTime.UtcNow;
    public DateTime DueDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    public string? TransferContent { get; set; }
    public string? QrDataUrl { get; set; }
    public string? BankTransactionId { get; set; }
    public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
    public ICollection<InvoiceAdjustment> Adjustments { get; set; } = new List<InvoiceAdjustment>();
    public ICollection<PaymentAllocation> PaymentAllocations { get; set; } = new List<PaymentAllocation>();
    public ICollection<PaymentExtension> PaymentExtensions { get; set; } = new List<PaymentExtension>();
}
