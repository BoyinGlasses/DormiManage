using DormitoryManagement.Domain.Common;
using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Domain.Entities;

public class RoomRegistration : AuditableEntity
{
    public Guid StudentId { get; set; }
    public Student? Student { get; set; }
    public Guid RoomId { get; set; }
    public Room? Room { get; set; }
    public RegistrationStatus Status { get; set; } = RegistrationStatus.Pending;
    public int ContractTermMonths { get; set; } = 12;
    public bool IncludesInternet { get; set; }
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public Guid? ReviewedByUserId { get; set; }
    public User? ReviewedByUser { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? RejectionReason { get; set; }
}
