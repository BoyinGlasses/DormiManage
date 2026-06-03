using System.ComponentModel.DataAnnotations;

namespace DormitoryManagement.Application.DTOs.Payments;

public sealed class ConfirmPaymentRequest
{
    [Required]
    public Guid PaymentId { get; set; }

    [StringLength(100)]
    public string? TransactionRef { get; set; }
}
