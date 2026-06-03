using DormitoryManagement.Domain.Entities;

namespace DormitoryManagement.Application.Abstractions.Repositories;

public interface IRoomRepository
{
    IQueryable<Room> Query();
    Task<Room?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Room room, CancellationToken ct = default);
    void Update(Room room);
}
