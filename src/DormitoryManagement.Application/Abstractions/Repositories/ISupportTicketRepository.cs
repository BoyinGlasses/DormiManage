using DormitoryManagement.Domain.Entities;

namespace DormitoryManagement.Application.Abstractions.Repositories;

public interface ISupportTicketRepository
{
    IQueryable<SupportTicket> Query();
    Task<SupportTicket?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(SupportTicket ticket, CancellationToken ct = default);
    void Update(SupportTicket ticket);
}
