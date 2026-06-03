using System.ComponentModel.DataAnnotations;
using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Application.DTOs.SupportTickets;

public sealed class UpdateSupportTicketStatusRequest
{
    [Required]
    public Guid TicketId { get; set; }

    public SupportTicketStatus Status { get; set; }

    [StringLength(1000)]
    public string? Note { get; set; }
}
