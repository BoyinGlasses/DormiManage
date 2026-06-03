using DormitoryManagement.Application.DTOs.Payments;

namespace DormitoryManagement.Application.Services.Payments;

public interface IPaymentExtensionService
{
    Task<PaymentExtensionDto> RequestExtensionAsync(CreatePaymentExtensionRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<PaymentExtensionDto>> GetPendingExtensionsAsync(CancellationToken ct = default);
    Task ApproveExtensionAsync(Guid extensionId, CancellationToken ct = default);
    Task RejectExtensionAsync(Guid extensionId, string reason, CancellationToken ct = default);
}
