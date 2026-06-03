using System.ComponentModel.DataAnnotations;

namespace DormitoryManagement.Application.DTOs.Billing;

public sealed class GenerateInvoiceQrRequest
{
    [Required]
    public Guid InvoiceId { get; set; }
}
