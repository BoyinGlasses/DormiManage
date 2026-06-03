using System.ComponentModel.DataAnnotations;

namespace DormitoryManagement.Application.DTOs.Billing;

public sealed class GenerateMonthlyInvoiceRequest
{
    public Guid? RoomId { get; set; }

    [Required, RegularExpression("^\\d{4}-\\d{2}$")]
    public string BillingPeriod { get; set; } = string.Empty;

    [Required]
    public DateTime DueDate { get; set; }
}
