using DormitoryManagement.Domain.Common;
using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Domain.Entities;

public class VehicleRegistration : AuditableEntity
{
    public Guid StudentId { get; set; }
    public Student? Student { get; set; }
    public string LicensePlate { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public int MonthCount { get; set; }
    public decimal Amount { get; set; }
    public Guid? InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }
    public DateTime? PaymentDate { get; set; }
    public VehicleStatus Status { get; set; } = VehicleStatus.Pending;
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public DateTime? ApprovedAt { get; set; }
}
