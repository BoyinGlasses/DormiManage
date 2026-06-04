namespace DormitoryManagement.Application.DTOs.Payments;

public sealed class PayOsWebhookEventDto
{
    public long OrderCode { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public DateTime TransactionDateTime { get; set; }
    public string Status { get; set; } = string.Empty;
}
