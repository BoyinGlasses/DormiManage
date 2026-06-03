using DormitoryManagement.Application.Abstractions.Data;
using DormitoryManagement.Domain.Common;
using DormitoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagement.Infrastructure.Repositories;

public class EfRepository<TEntity> : IRepository<TEntity> where TEntity : BaseEntity
{
    protected readonly DormitoryDbContext DbContext;
    protected readonly DbSet<TEntity> DbSet;

    public EfRepository(DormitoryDbContext dbContext)
    {
        DbContext = dbContext;
        DbSet = dbContext.Set<TEntity>();
    }

    public IQueryable<TEntity> Query() => DbSet.AsQueryable();

    public Task<TEntity?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        DbSet.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<TEntity>> ListAsync(CancellationToken ct = default) =>
        await DbSet.ToListAsync(ct);

    public Task AddAsync(TEntity entity, CancellationToken ct = default) => DbSet.AddAsync(entity, ct).AsTask();

    public void Update(TEntity entity) => DbSet.Update(entity);

    public void Remove(TEntity entity) => DbSet.Remove(entity);
}
