using DormitoryManagement.Application.Common;
using DormitoryManagement.Application.DTOs.Rooms;
using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Application.Services.Rooms;

public interface IRoomService
{
    Task<PagedResult<RoomDto>> GetRoomsAsync(RoomFilterRequest? request = null, CancellationToken ct = default);
    Task<IReadOnlyList<RoomDto>> GetAvailableRoomsAsync(RoomFilterRequest? request = null, CancellationToken ct = default);
    Task<RoomDto> CreateRoomAsync(CreateRoomRequest request, CancellationToken ct = default);
    Task<RoomDto> UpdateRoomAsync(Guid roomId, CreateRoomRequest request, CancellationToken ct = default);
    Task ChangeRoomStatusAsync(Guid roomId, RoomStatus status, CancellationToken ct = default);
}
