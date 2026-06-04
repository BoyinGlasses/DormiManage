namespace DormitoryManagement.Application.DTOs.Payments;

public sealed class PayOsPaymentLinkDto
{
    public long OrderCode { get; set; }
    public string PaymentLinkId { get; set; } = string.Empty;
    public string CheckoutUrl { get; set; } = string.Empty;
    public string QrCode { get; set; } = string.Empty;
    public string QrDataUrl { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
