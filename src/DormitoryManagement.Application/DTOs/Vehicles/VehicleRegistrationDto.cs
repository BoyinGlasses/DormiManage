using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Application.DTOs.Vehicles;

public sealed class VehicleRegistrationDto
{
    public Guid Id { get; set; }
    public int RowNumber { get; set; }
    public Guid StudentId { get; set; }
    public string LicensePlate { get; set; } = string.Empty;
    public string NormalizedPlate { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; }
    public DateTime? PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public int MonthCount { get; set; }
    public string VehicleType { get; set; } = string.Empty;
    public VehicleStatus Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public DateTime? ExpiryDate { get; set; }
    public InvoiceStatus InvoiceStatus { get; set; }
}
