using System.ComponentModel.DataAnnotations;
using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Application.DTOs.SupportTickets;

public sealed class CreateSupportTicketRequest
{
    public Guid? StudentId { get; set; }

    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(4000)]
    public string Description { get; set; } = string.Empty;

    public SupportTicketCategory Category { get; set; } = SupportTicketCategory.Other;
    public PriorityLevel Priority { get; set; } = PriorityLevel.Medium;
}
