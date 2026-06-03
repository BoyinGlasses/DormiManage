using DormitoryManagement.Domain.Common;
using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Domain.Entities;

public class UserNotification : BaseEntity
{
    public Guid NotificationId { get; set; }
    public Notification? Notification { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
}
