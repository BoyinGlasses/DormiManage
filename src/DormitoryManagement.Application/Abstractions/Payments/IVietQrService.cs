namespace DormitoryManagement.Application.Abstractions.Payments;

public interface IVietQrService
{
    Task<string> GenerateQrDataUrlAsync(decimal amount, string transferContent, CancellationToken ct = default);
}
