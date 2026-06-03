using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Application.DTOs.SupportTickets;

public sealed class SupportTicketDto
{
    public Guid Id { get; set; }
    public Guid? StudentId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public string AssignedTo { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SupportTicketCategory Category { get; set; }
    public SupportTicketStatus Status { get; set; }
    public PriorityLevel Priority { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public IReadOnlyList<SupportTicketResponseDto> Responses { get; set; } = Array.Empty<SupportTicketResponseDto>();
}

public sealed class SupportTicketResponseDto
{
    public Guid Id { get; set; }
    public string Author { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
