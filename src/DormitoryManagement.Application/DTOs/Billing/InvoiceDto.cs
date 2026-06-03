using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Application.DTOs.Billing;

public sealed class InvoiceDto
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid StudentId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public Guid RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string BillingPeriod { get; set; } = string.Empty;
    public InvoiceKind InvoiceKind { get; set; }
    public Guid? ContractId { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount => TotalAmount - PaidAmount;
    public InvoiceStatus Status { get; set; }
    public IReadOnlyCollection<InvoiceItemDto> Items { get; set; } = Array.Empty<InvoiceItemDto>();
}
