using DormitoryManagement.Domain.Entities;

namespace DormitoryManagement.Application.Abstractions.Repositories;

public interface IPaymentRepository
{
    IQueryable<Payment> Query();
    Task<Payment?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Payment payment, CancellationToken ct = default);
    void Update(Payment payment);
}
