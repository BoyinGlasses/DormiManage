using DormitoryManagement.Domain.Common;
using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Domain.Entities;

public class Manager : SoftDeleteEntity
{
    public string StaffCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool IsBuildingManager { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public Guid? BuildingId { get; set; }
    public Building? Building { get; set; }
    public ICollection<SupportTicket> AssignedTickets { get; set; } = new List<SupportTicket>();
}
