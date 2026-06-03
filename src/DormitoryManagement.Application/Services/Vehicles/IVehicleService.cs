using DormitoryManagement.Application.DTOs.Vehicles;

namespace DormitoryManagement.Application.Services.Vehicles;

public interface IVehicleService
{
    Task<IReadOnlyList<VehicleRegistrationDto>> GetCurrentStudentVehicleRegistrationsAsync(DateTime? asOfDate = null, CancellationToken ct = default);
    Task<VehicleRegistrationDto> RegisterVehicleAsync(CreateVehicleRegistrationRequest request, CancellationToken ct = default);
    Task ApproveVehicleAsync(Guid registrationId, CancellationToken ct = default);
    Task RejectVehicleAsync(Guid registrationId, string reason, CancellationToken ct = default);
    Task CancelVehicleAsync(Guid registrationId, CancellationToken ct = default);
}
