using System.ComponentModel.DataAnnotations;

namespace DormitoryManagement.Application.DTOs.Payments;

public sealed class CreatePaymentExtensionRequest
{
    [Required]
    public Guid InvoiceId { get; set; }

    [Required]
    public DateTime RequestedDueDate { get; set; }

    [Required, StringLength(500, MinimumLength = 3)]
    public string Reason { get; set; } = string.Empty;
}
