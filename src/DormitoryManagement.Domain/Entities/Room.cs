using DormitoryManagement.Domain.Common;
using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Domain.Entities;

public class Room : SoftDeleteEntity
{
    public string RoomNumber { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int CurrentOccupancy { get; set; }
    public decimal MonthlyPrice { get; set; }
    public RoomStatus Status { get; set; } = RoomStatus.Available;
    public RoomGenderType GenderType { get; set; } = RoomGenderType.Mixed;
    public Guid BuildingId { get; set; }
    public Building? Building { get; set; }
    public Guid FloorId { get; set; }
    public Floor? Floor { get; set; }
    public ICollection<RoomRegistration> Registrations { get; set; } = new List<RoomRegistration>();
    public ICollection<RoomAssignment> Assignments { get; set; } = new List<RoomAssignment>();
    public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    public ICollection<UtilityReading> UtilityReadings { get; set; } = new List<UtilityReading>();
}
