namespace DormitoryManagement.Application.DTOs.Payments;

public sealed class PayOsOptions
{
    public const string SectionName = "PayOs";

    public string ClientId { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ChecksumKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api-merchant.payos.vn";
    public string ReturnUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;
    public string WebhookUrl { get; set; } = string.Empty;
    public string WebhookListenPrefix { get; set; } = "http://localhost:5126/payos/webhook/";
    public bool AutoConfirmWebhook { get; set; }
}
