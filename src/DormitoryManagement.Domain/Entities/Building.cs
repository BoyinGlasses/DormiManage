using DormitoryManagement.Domain.Common;
using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Domain.Entities;

public class Building : SoftDeleteEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<Floor> Floors { get; set; } = new List<Floor>();
    public ICollection<Room> Rooms { get; set; } = new List<Room>();
    public ICollection<Manager> Managers { get; set; } = new List<Manager>();
}
