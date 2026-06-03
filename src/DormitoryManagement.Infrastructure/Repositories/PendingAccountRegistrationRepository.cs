using DormitoryManagement.Application.Abstractions.Repositories;
using DormitoryManagement.Domain.Entities;
using DormitoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagement.Infrastructure.Repositories;

public sealed class PendingAccountRegistrationRepository : IPendingAccountRegistrationRepository
{
    private readonly DormitoryDbContext _dbContext;

    public PendingAccountRegistrationRepository(DormitoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IQueryable<PendingAccountRegistration> Query() => _dbContext.PendingAccountRegistrations.AsQueryable();

    public Task<PendingAccountRegistration?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _dbContext.PendingAccountRegistrations.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<PendingAccountRegistration?> GetByIdForUpdateAsync(Guid id, CancellationToken ct = default) =>
        _dbContext.PendingAccountRegistrations
            .FromSqlInterpolated($"SELECT * FROM pending_account_registrations WITH (UPDLOCK, ROWLOCK) WHERE Id = {id}")
            .AsTracking()
            .FirstOrDefaultAsync(ct);

    public Task AddAsync(PendingAccountRegistration registration, CancellationToken ct = default) =>
        _dbContext.PendingAccountRegistrations.AddAsync(registration, ct).AsTask();

    public void Update(PendingAccountRegistration registration) => _dbContext.PendingAccountRegistrations.Update(registration);

    public void Remove(PendingAccountRegistration registration) => _dbContext.PendingAccountRegistrations.Remove(registration);

    public void RemoveRange(IEnumerable<PendingAccountRegistration> registrations) =>
        _dbContext.PendingAccountRegistrations.RemoveRange(registrations);
}
