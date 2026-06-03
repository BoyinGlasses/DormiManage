using DormitoryManagement.Domain.Entities;

namespace DormitoryManagement.Application.Abstractions.Repositories;

public interface IInvoiceRepository
{
    IQueryable<Invoice> Query();
    Task<Invoice?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Invoice invoice, CancellationToken ct = default);
    void Update(Invoice invoice);
}
