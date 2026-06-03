using DormitoryManagement.Application.DTOs.Billing;

namespace DormitoryManagement.Application.Services.Billing;

public interface IBillingService
{
    Task<UtilityBillingPreviewDto> PreviewUtilityBillingAsync(UtilityBillingPreviewRequest request, CancellationToken ct = default);
    Task UpsertUtilityReadingAsync(UtilityReadingRequest request, CancellationToken ct = default);
    Task<GenerateMonthlyInvoiceResult> GenerateMonthlyInvoicesAsync(GenerateMonthlyInvoiceRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<InvoiceDto>> GetInvoicesAsync(string? billingPeriod = null, CancellationToken ct = default);
    Task<IReadOnlyList<StudentBillingRowDto>> GetStudentBillingRowsAsync(string? billingPeriod = null, CancellationToken ct = default);
    Task<InvoiceDto?> GetInvoiceAsync(Guid invoiceId, CancellationToken ct = default);
    Task<InvoiceDto> CreateInvoiceAsync(Guid studentId, Guid roomId, string billingPeriod, CancellationToken ct = default);
    Task AddInvoiceItemAsync(Guid invoiceId, InvoiceItemDto item, CancellationToken ct = default);
    Task<int> MarkOverdueInvoicesAsync(DateTime asOfDate, CancellationToken ct = default);
    Task AdjustInvoiceAsync(Guid invoiceId, decimal amount, string reason, CancellationToken ct = default);
}
