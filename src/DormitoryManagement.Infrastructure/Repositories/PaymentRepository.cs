using DormitoryManagement.Application.Abstractions.Repositories;
using DormitoryManagement.Domain.Entities;
using DormitoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagement.Infrastructure.Repositories;

public sealed class PaymentRepository : IPaymentRepository
{
    private readonly DormitoryDbContext _dbContext;

    public PaymentRepository(DormitoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IQueryable<Payment> Query() => _dbContext.Payments.AsQueryable();

    public Task<Payment?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _dbContext.Payments.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task AddAsync(Payment entity, CancellationToken ct = default) => _dbContext.Payments.AddAsync(entity, ct).AsTask();

    public void Update(Payment entity) => _dbContext.Payments.Update(entity);
}
