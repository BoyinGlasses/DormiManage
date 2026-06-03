using DormitoryManagement.Domain.Common;
using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Domain.Entities;

public class UtilityReading : AuditableEntity
{
    public Guid RoomId { get; set; }
    public Room? Room { get; set; }
    public string BillingPeriod { get; set; } = string.Empty;
    public decimal ElectricityPrevious { get; set; }
    public decimal ElectricityCurrent { get; set; }
    public decimal WaterPrevious { get; set; }
    public decimal WaterCurrent { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
}
