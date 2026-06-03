using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Application.DTOs.Rooms;

public sealed class RoomDto
{
    public Guid Id { get; set; }
    public Guid BuildingId { get; set; }
    public string BuildingName { get; set; } = string.Empty;
    public Guid FloorId { get; set; }
    public int FloorNumber { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int CurrentOccupancy { get; set; }
    public int HeldSlots { get; set; }
    public int AvailableSlots { get; set; }
    public decimal MonthlyPrice { get; set; }
    public RoomStatus Status { get; set; }
    public RoomGenderType GenderType { get; set; }
}
