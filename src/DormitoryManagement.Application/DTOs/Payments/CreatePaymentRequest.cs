using System.ComponentModel.DataAnnotations;
using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Application.DTOs.Payments;

public sealed class CreatePaymentRequest
{
    [Required]
    public Guid StudentId { get; set; }

    [Required]
    public Guid InvoiceId { get; set; }

    [Range(1, 999999999)]
    public decimal Amount { get; set; }

    public PaymentMethod Method { get; set; } = PaymentMethod.QrBanking;
}
