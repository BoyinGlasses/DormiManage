using DormitoryManagement.Domain.Common;
using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Domain.Entities;

public class User : SoftDeleteEntity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserStatus Status { get; set; } = UserStatus.Active;
    public int FailedLoginCount { get; set; }
    public DateTime? LockedUntil { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public Guid RoleId { get; set; }
    public Role? Role { get; set; }
    public Student? Student { get; set; }
    public Manager? Manager { get; set; }
    public ICollection<SupportTicket> CreatedTickets { get; set; } = new List<SupportTicket>();
    public ICollection<SupportTicketResponse> TicketResponses { get; set; } = new List<SupportTicketResponse>();
    public ICollection<UserNotification> UserNotifications { get; set; } = new List<UserNotification>();
}
