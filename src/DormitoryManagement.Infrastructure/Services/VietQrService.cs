using DormitoryManagement.Application.Abstractions.Payments;
using DormitoryManagement.Application.DTOs.Payments;

namespace DormitoryManagement.Infrastructure.Services;

public sealed class VietQrService : IVietQrService
{
    private readonly QrBankingOptions _options;

    public VietQrService(QrBankingOptions options)
    {
        _options = options;
    }

    public Task<string> GenerateQrDataUrlAsync(decimal amount, string transferContent, CancellationToken ct = default)
    {
        if (amount <= 0m)
        {
            throw new InvalidOperationException("QR amount must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(transferContent))
        {
            throw new InvalidOperationException("Transfer content is required.");
        }

        if (string.IsNullOrWhiteSpace(_options.BankAcqId) || string.IsNullOrWhiteSpace(_options.BankAccountNo))
        {
            throw new InvalidOperationException("QR banking account configuration is missing.");
        }

        var amountText = decimal.Truncate(amount).ToString("0");
        var accountName = Uri.EscapeDataString(_options.BankAccountName ?? string.Empty);
        var info = Uri.EscapeDataString(transferContent.Trim());
        var url = $"https://img.vietqr.io/image/{_options.BankAcqId.Trim()}-{_options.BankAccountNo.Trim()}-compact2.png?amount={amountText}&addInfo={info}&accountName={accountName}";
        return Task.FromResult(url);
    }
}
