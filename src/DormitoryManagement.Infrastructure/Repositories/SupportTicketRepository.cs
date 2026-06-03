using DormitoryManagement.Application.Abstractions.Repositories;
using DormitoryManagement.Domain.Entities;
using DormitoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagement.Infrastructure.Repositories;

public sealed class SupportTicketRepository : ISupportTicketRepository
{
    private readonly DormitoryDbContext _dbContext;

    public SupportTicketRepository(DormitoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IQueryable<SupportTicket> Query() => _dbContext.SupportTickets.AsQueryable();

    public Task<SupportTicket?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _dbContext.SupportTickets.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task AddAsync(SupportTicket entity, CancellationToken ct = default) => _dbContext.SupportTickets.AddAsync(entity, ct).AsTask();

    public void Update(SupportTicket entity) => _dbContext.SupportTickets.Update(entity);
}
