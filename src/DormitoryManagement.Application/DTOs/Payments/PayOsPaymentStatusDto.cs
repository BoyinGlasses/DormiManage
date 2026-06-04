namespace DormitoryManagement.Application.DTOs.Payments;

public sealed class PayOsPaymentStatusDto
{
    public long OrderCode { get; set; }
    public string PaymentLinkId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal AmountPaid { get; set; }
    public string Description { get; set; } = string.Empty;
    public string CheckoutUrl { get; set; } = string.Empty;
    public string QrCode { get; set; } = string.Empty;
}
