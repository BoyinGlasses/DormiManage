using DormitoryManagement.Application.DTOs.SupportTickets;

namespace DormitoryManagement.Application.Services.SupportTickets;

public interface ISupportTicketService
{
    Task<IReadOnlyList<SupportTicketDto>> GetTicketsAsync(CancellationToken ct = default);
    Task<SupportTicketDto?> GetTicketAsync(Guid ticketId, CancellationToken ct = default);
    Task<SupportTicketDto> CreateTicketAsync(CreateSupportTicketRequest request, CancellationToken ct = default);
    Task AssignTicketAsync(Guid ticketId, Guid managerId, CancellationToken ct = default);
    Task AddResponseAsync(Guid ticketId, string message, CancellationToken ct = default);
    Task UpdateStatusAsync(UpdateSupportTicketStatusRequest request, CancellationToken ct = default);
    Task CloseTicketAsync(Guid ticketId, CancellationToken ct = default);
}
