using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Application.DTOs.Rooms;

public sealed class RoomFilterRequest
{
    public Guid? BuildingId { get; set; }
    public Guid? FloorId { get; set; }
    public RoomStatus? Status { get; set; }
    public RoomGenderType? GenderType { get; set; }
    public RoomPriceSortOrder PriceSortOrder { get; set; } = RoomPriceSortOrder.None;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public enum RoomPriceSortOrder
{
    None,
    LowToHigh,
    HighToLow
}
