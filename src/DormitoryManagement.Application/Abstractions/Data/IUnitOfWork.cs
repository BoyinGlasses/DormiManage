using DormitoryManagement.Domain.Common;

namespace DormitoryManagement.Application.Abstractions.Data;

public interface IUnitOfWork
{
    IRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity;
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken ct = default);
}

public interface IUnitOfWorkTransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken ct = default);
    Task RollbackAsync(CancellationToken ct = default);
}
