using DormitoryManagement.Application.Abstractions.Repositories;
using DormitoryManagement.Domain.Entities;
using DormitoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagement.Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly DormitoryDbContext _dbContext;

    public UserRepository(DormitoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _dbContext.Users
            .Include(x => x.Role)
            .Include(x => x.Student)
            .Include(x => x.Manager)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<User?> GetByEmailOrUsernameAsync(string value, CancellationToken ct = default)
    {
        var normalized = value.Trim().ToLowerInvariant();
        return _dbContext.Users
            .Include(x => x.Role)
            .Include(x => x.Student)
            .Include(x => x.Manager)
            .FirstOrDefaultAsync(x => x.Username.ToLower() == normalized || x.Email.ToLower() == normalized, ct);
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        return _dbContext.Users.FirstOrDefaultAsync(x => x.Email.ToLower() == normalized, ct);
    }

    public Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        var normalized = username.Trim().ToLowerInvariant();
        return _dbContext.Users.FirstOrDefaultAsync(x => x.Username.ToLower() == normalized, ct);
    }

    public Task<User?> GetByStudentCodeAsync(string studentCode, CancellationToken ct = default)
    {
        var normalized = studentCode.Trim().ToLowerInvariant();
        return _dbContext.Users
            .Include(x => x.Role)
            .Include(x => x.Student)
            .Include(x => x.Manager)
            .FirstOrDefaultAsync(x => x.Student != null && x.Student.StudentCode.ToLower() == normalized, ct);
    }

    public Task AddAsync(User user, CancellationToken ct = default) => _dbContext.Users.AddAsync(user, ct).AsTask();

    public Task UpdateAsync(User user, CancellationToken ct = default)
    {
        _dbContext.Users.Update(user);
        return Task.CompletedTask;
    }
}
