using System.ComponentModel.DataAnnotations;

namespace DormitoryManagement.Application.DTOs.Billing;

public sealed class UtilityBillingPreviewRequest
{
    [Required]
    public Guid RoomId { get; set; }

    [Required, RegularExpression("^\\d{4}-(0[1-9]|1[0-2])$")]
    public string BillingPeriod { get; set; } = string.Empty;

    [Range(0, 9999999)]
    public decimal? ElectricityPrevious { get; set; }

    [Range(0, 9999999)]
    public decimal ElectricityCurrent { get; set; }

    [Range(0, 9999999)]
    public decimal? WaterPrevious { get; set; }

    [Range(0, 9999999)]
    public decimal WaterCurrent { get; set; }
}
