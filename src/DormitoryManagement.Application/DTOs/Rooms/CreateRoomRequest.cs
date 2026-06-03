using System.ComponentModel.DataAnnotations;
using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Application.DTOs.Rooms;

public sealed class CreateRoomRequest
{
    [Required]
    public Guid BuildingId { get; set; }

    [Required]
    public Guid FloorId { get; set; }

    [Required, StringLength(20)]
    public string RoomNumber { get; set; } = string.Empty;

    [Range(1, 20)]
    public int Capacity { get; set; }

    [Range(0, 999999999)]
    public decimal MonthlyPrice { get; set; }

    public RoomGenderType GenderType { get; set; } = RoomGenderType.Male;
}
