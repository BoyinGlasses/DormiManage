namespace DormitoryManagement.Application.DTOs.Payments;

public sealed class PayOsCreatePaymentRequest
{
    public long OrderCode { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string BuyerName { get; set; } = string.Empty;
}
