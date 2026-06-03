using DormitoryManagement.Application.Abstractions.Repositories;
using DormitoryManagement.Domain.Entities;
using DormitoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagement.Infrastructure.Repositories;

public sealed class RoomRepository : IRoomRepository
{
    private readonly DormitoryDbContext _dbContext;

    public RoomRepository(DormitoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IQueryable<Room> Query() => _dbContext.Rooms.AsQueryable();

    public Task<Room?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _dbContext.Rooms.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task AddAsync(Room entity, CancellationToken ct = default) => _dbContext.Rooms.AddAsync(entity, ct).AsTask();

    public void Update(Room entity) => _dbContext.Rooms.Update(entity);
}
