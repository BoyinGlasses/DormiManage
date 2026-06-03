namespace DormitoryManagement.Application.DTOs.Payments;

public sealed class QrBankingOptions
{
    public const string SectionName = "QrBanking";

    public string BankAccountNo { get; set; } = string.Empty;
    public string BankAccountName { get; set; } = string.Empty;
    public string BankAcqId { get; set; } = string.Empty;
    public string VietQrClientId { get; set; } = string.Empty;
    public string VietQrApiKey { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
}
