using DormitoryManagement.Application.Abstractions.Auth;
using DormitoryManagement.Application.Abstractions.Services;
using DormitoryManagement.Application.DTOs.Audit;
using DormitoryManagement.Domain.Entities;
using DormitoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagement.Infrastructure.Services;

public sealed class AuditLogService : IAuditLogService
{
    private readonly DormitoryDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public AuditLogService(DormitoryDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task WriteAsync(string action, string entityName, Guid? entityId = null, string? details = null, CancellationToken ct = default)
    {
        _dbContext.AuditLogs.Add(new AuditLog
        {
            UserId = _currentUser.UserId,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            Details = details,
            Workstation = Environment.MachineName,
            CreatedAt = DateTime.UtcNow
        });

        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<AuditLogDto>> GetRecentAsync(string? searchText = null, int take = 100, CancellationToken ct = default)
    {
        var query = _dbContext.AuditLogs
            .Include(log => log.User)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var term = searchText.Trim();
            query = query.Where(log =>
                log.Action.Contains(term) ||
                log.EntityName.Contains(term) ||
                (log.Details != null && log.Details.Contains(term)) ||
                (log.User != null && (log.User.Username.Contains(term) || log.User.FullName.Contains(term))));
        }

        return await query
            .OrderByDescending(log => log.CreatedAt)
            .Take(Math.Clamp(take, 1, 500))
            .Select(log => new AuditLogDto
            {
                Id = log.Id,
                UserId = log.UserId,
                UserName = log.User == null
                    ? "System"
                    : string.IsNullOrWhiteSpace(log.User.FullName) ? log.User.Username : log.User.FullName,
                Action = log.Action,
                EntityName = log.EntityName,
                EntityId = log.EntityId,
                Details = log.Details,
                Workstation = log.Workstation,
                CreatedAt = log.CreatedAt
            })
            .ToListAsync(ct);
    }
}
