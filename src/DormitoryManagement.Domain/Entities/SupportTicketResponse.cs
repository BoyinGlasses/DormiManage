using DormitoryManagement.Domain.Common;
using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Domain.Entities;

public class SupportTicketResponse : AuditableEntity
{
    public Guid SupportTicketId { get; set; }
    public SupportTicket? SupportTicket { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime RespondedAt { get; set; } = DateTime.UtcNow;
}
