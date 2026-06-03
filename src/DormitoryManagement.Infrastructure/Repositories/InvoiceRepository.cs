using DormitoryManagement.Application.Abstractions.Repositories;
using DormitoryManagement.Domain.Entities;
using DormitoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagement.Infrastructure.Repositories;

public sealed class InvoiceRepository : IInvoiceRepository
{
    private readonly DormitoryDbContext _dbContext;

    public InvoiceRepository(DormitoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IQueryable<Invoice> Query() => _dbContext.Invoices.AsQueryable();

    public Task<Invoice?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _dbContext.Invoices.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task AddAsync(Invoice entity, CancellationToken ct = default) => _dbContext.Invoices.AddAsync(entity, ct).AsTask();

    public void Update(Invoice entity) => _dbContext.Invoices.Update(entity);
}
