using DormitoryManagement.Domain.Common;
using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Domain.Entities;

public class Floor : SoftDeleteEntity
{
    public int FloorNumber { get; set; }
    public string? Name { get; set; }
    public Guid BuildingId { get; set; }
    public Building? Building { get; set; }
    public ICollection<Room> Rooms { get; set; } = new List<Room>();
}
