using System.ComponentModel.DataAnnotations;

namespace DormitoryManagement.Application.DTOs.Payments;

public sealed class BankTransferNotificationDto
{
    [Required]
    public string TransactionId { get; set; } = string.Empty;

    [Range(1, 999999999)]
    public decimal Amount { get; set; }

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public DateTime TransactionDate { get; set; }
}
