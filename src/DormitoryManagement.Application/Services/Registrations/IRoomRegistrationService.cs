using DormitoryManagement.Application.DTOs.Registrations;

namespace DormitoryManagement.Application.Services.Registrations;

public interface IRoomRegistrationService
{
    Task<Guid> CreateRegistrationAsync(CreateRoomRegistrationRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<RoomRegistrationDto>> GetPendingRegistrationsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<RoomRegistrationDto>> GetCurrentStudentRegistrationsAsync(CancellationToken ct = default);
    Task ApproveRegistrationAsync(ApproveRoomRegistrationRequest request, CancellationToken ct = default);
    Task RejectRegistrationAsync(RejectRoomRegistrationRequest request, CancellationToken ct = default);
    Task CancelRegistrationAsync(Guid registrationId, CancellationToken ct = default);
}
