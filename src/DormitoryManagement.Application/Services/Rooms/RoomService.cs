using DormitoryManagement.Application.Abstractions.Auth;
using DormitoryManagement.Application.Abstractions.Data;
using DormitoryManagement.Application.Abstractions.Repositories;
using DormitoryManagement.Application.Common;
using DormitoryManagement.Application.DTOs.Rooms;
using DormitoryManagement.Application.Validation;
using DormitoryManagement.Domain.Constants;
using DormitoryManagement.Domain.Entities;
using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Application.Services.Rooms;

public sealed class RoomService : IRoomService
{
    private readonly IRoomRepository _rooms;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPermissionService _permissions;

    public RoomService(IRoomRepository rooms, IUnitOfWork unitOfWork, IPermissionService permissions)
    {
        _rooms = rooms;
        _unitOfWork = unitOfWork;
        _permissions = permissions;
    }

    public async Task<PagedResult<RoomDto>> GetRoomsAsync(RoomFilterRequest? request = null, CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.RoomsRead, ct);
        request ??= new RoomFilterRequest();
        var rooms = ApplyRoomSort(
                ApplyRoomFilters(_rooms.Query().Where(room => !room.IsDeleted && room.GenderType != RoomGenderType.Mixed), request),
                request)
            .ToList();

        var mappedRooms = MapRooms(rooms);
        if (request.Status.HasValue)
        {
            mappedRooms = mappedRooms.Where(room => room.Status == request.Status.Value).ToArray();
        }

        var totalCount = mappedRooms.Count;
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Max(1, request.PageSize);
        var pageRooms = mappedRooms
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<RoomDto>(pageRooms, totalCount, pageNumber, pageSize);
    }

    public async Task<IReadOnlyList<RoomDto>> GetAvailableRoomsAsync(RoomFilterRequest? request = null, CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.RoomsRead, ct);
        request ??= new RoomFilterRequest();
        request.Status = RoomStatus.Available;
        var rooms = ApplyRoomSort(
                ApplyRoomFilters(_rooms.Query().Where(room => !room.IsDeleted && room.GenderType != RoomGenderType.Mixed), request),
                request)
            .ToList();

        return MapRooms(rooms)
            .Where(room => room.AvailableSlots > 0 && room.Status == RoomStatus.Available)
            .ToArray();
    }

    public async Task<RoomDto> CreateRoomAsync(CreateRoomRequest request, CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.RoomsWrite, ct);
        RequestValidator.ValidateAndThrow(request);
        EnsureSupportedGenderType(request.GenderType);
        // TODO: Enforce unique building/floor/room number.
        return new RoomDto { Id = Guid.NewGuid(), RoomNumber = request.RoomNumber, Capacity = request.Capacity, AvailableSlots = request.Capacity, MonthlyPrice = request.MonthlyPrice, GenderType = request.GenderType };
    }

    public async Task<RoomDto> UpdateRoomAsync(Guid roomId, CreateRoomRequest request, CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.RoomsWrite, ct);
        RequestValidator.ValidateAndThrow(request);
        EnsureSupportedGenderType(request.GenderType);
        // TODO: Update only structural room fields allowed by policy.
        return new RoomDto { Id = roomId, RoomNumber = request.RoomNumber, Capacity = request.Capacity, AvailableSlots = request.Capacity, MonthlyPrice = request.MonthlyPrice, GenderType = request.GenderType };
    }

    public async Task ChangeRoomStatusAsync(Guid roomId, RoomStatus status, CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.RoomsWrite, ct);
        // TODO: Prevent Full/Available transitions inconsistent with current occupancy.
    }

    private static IQueryable<Room> ApplyRoomFilters(IQueryable<Room> query, RoomFilterRequest request)
    {
        if (request.BuildingId.HasValue)
        {
            query = query.Where(room => room.BuildingId == request.BuildingId.Value);
        }

        if (request.FloorId.HasValue)
        {
            query = query.Where(room => room.FloorId == request.FloorId.Value);
        }

        if (request.GenderType.HasValue)
        {
            query = query.Where(room => room.GenderType == request.GenderType.Value);
        }

        return query;
    }

    private static IOrderedQueryable<Room> ApplyRoomSort(IQueryable<Room> query, RoomFilterRequest request) =>
        request.PriceSortOrder switch
        {
            RoomPriceSortOrder.LowToHigh => query.OrderBy(room => room.MonthlyPrice).ThenBy(room => room.RoomNumber),
            RoomPriceSortOrder.HighToLow => query.OrderByDescending(room => room.MonthlyPrice).ThenBy(room => room.RoomNumber),
            _ => query.OrderBy(room => room.RoomNumber)
        };

    private static void EnsureSupportedGenderType(RoomGenderType genderType)
    {
        if (genderType is not (RoomGenderType.Male or RoomGenderType.Female))
        {
            throw new InvalidOperationException("Room gender type must be Male or Female.");
        }
    }

    private IReadOnlyList<RoomDto> MapRooms(IReadOnlyList<Room> rooms)
    {
        if (rooms.Count == 0)
        {
            return Array.Empty<RoomDto>();
        }

        var roomIds = rooms.Select(room => room.Id).ToHashSet();
        var buildingIds = rooms.Select(room => room.BuildingId).ToHashSet();
        var floorIds = rooms.Select(room => room.FloorId).ToHashSet();
        var buildings = _unitOfWork.Repository<Building>().Query()
            .Where(building => buildingIds.Contains(building.Id))
            .ToDictionary(building => building.Id);
        var floors = _unitOfWork.Repository<Floor>().Query()
            .Where(floor => floorIds.Contains(floor.Id))
            .ToDictionary(floor => floor.Id);
        var activeAssignments = _unitOfWork.Repository<RoomAssignment>().Query()
            .Where(assignment => assignment.IsActive && roomIds.Contains(assignment.RoomId))
            .GroupBy(assignment => assignment.RoomId)
            .ToDictionary(group => group.Key, group => group.Count());
        var holdStatuses = new[] { RegistrationStatus.Pending, RegistrationStatus.PaymentPending };
        var pendingRegistrations = _unitOfWork.Repository<RoomRegistration>().Query()
            .Where(registration => holdStatuses.Contains(registration.Status) && roomIds.Contains(registration.RoomId))
            .GroupBy(registration => registration.RoomId)
            .ToDictionary(group => group.Key, group => group.Count());

        return rooms.Select(room =>
        {
            activeAssignments.TryGetValue(room.Id, out var activeCount);
            pendingRegistrations.TryGetValue(room.Id, out var heldSlots);
            var occupiedSlots = Math.Max(room.CurrentOccupancy, activeCount);
            var availableSlots = Math.Max(0, room.Capacity - occupiedSlots - heldSlots);
            var status = room.Status;
            if (status is RoomStatus.Available or RoomStatus.Full)
            {
                status = availableSlots <= 0 ? RoomStatus.Full : RoomStatus.Available;
            }

            return new RoomDto
            {
                Id = room.Id,
                BuildingId = room.BuildingId,
                BuildingName = buildings.TryGetValue(room.BuildingId, out var building) ? building.Name : string.Empty,
                FloorId = room.FloorId,
                FloorNumber = floors.TryGetValue(room.FloorId, out var floor) ? floor.FloorNumber : 0,
                RoomNumber = room.RoomNumber,
                Capacity = room.Capacity,
                CurrentOccupancy = occupiedSlots,
                HeldSlots = heldSlots,
                AvailableSlots = availableSlots,
                MonthlyPrice = room.MonthlyPrice,
                Status = status,
                GenderType = room.GenderType
            };
        }).ToArray();
    }
}
