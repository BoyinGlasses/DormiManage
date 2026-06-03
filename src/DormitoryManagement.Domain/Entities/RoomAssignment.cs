using DormitoryManagement.Domain.Common;
using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Domain.Entities;

public class RoomAssignment : AuditableEntity
{
    public Guid StudentId { get; set; }
    public Student? Student { get; set; }
    public Guid RoomId { get; set; }
    public Room? Room { get; set; }
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
}
