using DormitoryManagement.Application.DTOs.Payments;

namespace DormitoryManagement.Application.Abstractions.Payments;

public interface IPayOsService
{
    Task<PayOsPaymentLinkDto> CreatePaymentLinkAsync(PayOsCreatePaymentRequest request, CancellationToken ct = default);
    Task<PayOsPaymentStatusDto> GetPaymentLinkAsync(long orderCode, CancellationToken ct = default);
    Task ConfirmWebhookAsync(string webhookUrl, CancellationToken ct = default);
    PayOsWebhookEventDto ParseWebhook(string payload);
}
