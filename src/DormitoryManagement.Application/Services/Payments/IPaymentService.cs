using DormitoryManagement.Application.DTOs.Billing;
using DormitoryManagement.Application.DTOs.Payments;

namespace DormitoryManagement.Application.Services.Payments;

public interface IPaymentService
{
    Task<IReadOnlyList<OutstandingInvoiceDto>> GetOutstandingInvoicesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<PaymentDto>> GetPendingPaymentsAsync(CancellationToken ct = default);
    Task<PaymentDto> CreateMockPaymentAsync(CreatePaymentRequest request, CancellationToken ct = default);
    Task<InvoicePaymentQrDto> GenerateInvoiceQrAsync(Guid invoiceId, CancellationToken ct = default);
    Task<InvoicePaymentQrDto> GetInvoicePaymentQrAsync(Guid invoiceId, CancellationToken ct = default);
    Task<BankTransferProcessResultDto> ProcessBankTransferAsync(BankTransferNotificationDto notification, CancellationToken ct = default);
    Task<PaymentDto> ConfirmPaymentAsync(ConfirmPaymentRequest request, CancellationToken ct = default);
    Task AllocatePaymentAsync(Guid paymentId, Guid invoiceId, decimal amount, CancellationToken ct = default);
    Task CancelPaymentAsync(Guid paymentId, string reason, CancellationToken ct = default);
}
