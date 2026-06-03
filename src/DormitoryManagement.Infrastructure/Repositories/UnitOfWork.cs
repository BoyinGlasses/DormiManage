using DormitoryManagement.Application.Abstractions.Data;
using DormitoryManagement.Domain.Common;
using DormitoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace DormitoryManagement.Infrastructure.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly DormitoryDbContext _dbContext;
    private readonly Dictionary<Type, object> _repositories = new();

    public UnitOfWork(DormitoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity
    {
        var entityType = typeof(TEntity);
        if (!_repositories.TryGetValue(entityType, out var repository))
        {
            repository = new EfRepository<TEntity>(_dbContext);
            _repositories[entityType] = repository;
        }

        return (IRepository<TEntity>)repository;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _dbContext.SaveChangesAsync(ct);

    public async Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken ct = default)
    {
        var transaction = await _dbContext.Database.BeginTransactionAsync(ct);
        return new EfUnitOfWorkTransaction(transaction);
    }

    private sealed class EfUnitOfWorkTransaction : IUnitOfWorkTransaction
    {
        private readonly IDbContextTransaction _transaction;

        public EfUnitOfWorkTransaction(IDbContextTransaction transaction)
        {
            _transaction = transaction;
        }

        public Task CommitAsync(CancellationToken ct = default) => _transaction.CommitAsync(ct);

        public Task RollbackAsync(CancellationToken ct = default) => _transaction.RollbackAsync(ct);

        public ValueTask DisposeAsync() => _transaction.DisposeAsync();
    }
}
