using DormitoryManagement.Domain.Entities;

namespace DormitoryManagement.Application.Abstractions.Repositories;

public interface IPendingAccountRegistrationRepository
{
    IQueryable<PendingAccountRegistration> Query();
    Task<PendingAccountRegistration?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PendingAccountRegistration?> GetByIdForUpdateAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(PendingAccountRegistration registration, CancellationToken ct = default);
    void Update(PendingAccountRegistration registration);
    void Remove(PendingAccountRegistration registration);
    void RemoveRange(IEnumerable<PendingAccountRegistration> registrations);
}
