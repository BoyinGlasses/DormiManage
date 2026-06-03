namespace DormitoryManagement.Application.DTOs.Billing;

public sealed class UtilityBillingPreviewDto
{
    public Guid RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string BillingPeriod { get; set; } = string.Empty;
    public decimal ElectricityPrevious { get; set; }
    public decimal ElectricityCurrent { get; set; }
    public decimal ElectricityConsumption { get; set; }
    public decimal ElectricityAmount { get; set; }
    public decimal WaterPrevious { get; set; }
    public decimal WaterCurrent { get; set; }
    public decimal WaterConsumption { get; set; }
    public decimal WaterAmount { get; set; }
    public int ActiveMemberCount { get; set; }
    public int InternetSubscriberCount { get; set; }
    public decimal InternetUnitPrice { get; set; }
    public decimal InternetAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PerMemberAmount { get; set; }
    public DateTime DueDate { get; set; }
    public IReadOnlyList<string> ValidationMessages { get; set; } = Array.Empty<string>();
}
