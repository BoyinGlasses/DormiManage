using DormitoryManagement.Domain.Common;
using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Domain.Entities;

public class SupportTicket : AuditableEntity
{
    public Guid? StudentId { get; set; }
    public Student? Student { get; set; }
    public Guid CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }
    public Guid? AssignedToManagerId { get; set; }
    public Manager? AssignedToManager { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SupportTicketCategory Category { get; set; } = SupportTicketCategory.Other;
    public SupportTicketStatus Status { get; set; } = SupportTicketStatus.New;
    public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;
    public DateTime? ResolvedAt { get; set; }
    public ICollection<SupportTicketResponse> Responses { get; set; } = new List<SupportTicketResponse>();
}
