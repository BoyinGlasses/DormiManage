using DormitoryManagement.Application.Abstractions.Repositories;
using DormitoryManagement.Domain.Entities;
using DormitoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagement.Infrastructure.Repositories;

public sealed class StudentRepository : IStudentRepository
{
    private readonly DormitoryDbContext _dbContext;

    public StudentRepository(DormitoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IQueryable<Student> Query() => _dbContext.Students.AsQueryable();

    public Task<Student?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _dbContext.Students.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<Student?> GetByStudentCodeAsync(string studentCode, CancellationToken ct = default)
    {
        var normalized = studentCode.Trim().ToLowerInvariant();
        return _dbContext.Students.FirstOrDefaultAsync(x => x.StudentCode.ToLower() == normalized, ct);
    }

    public Task AddAsync(Student entity, CancellationToken ct = default) => _dbContext.Students.AddAsync(entity, ct).AsTask();

    public void Update(Student entity) => _dbContext.Students.Update(entity);
}
