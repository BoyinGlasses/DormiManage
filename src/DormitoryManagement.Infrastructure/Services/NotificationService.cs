using DormitoryManagement.Application.Abstractions.Auth;
using DormitoryManagement.Application.Abstractions.Services;
using DormitoryManagement.Application.DTOs.Notifications;
using DormitoryManagement.Domain.Entities;
using DormitoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagement.Infrastructure.Services;

public sealed class NotificationService : INotificationService
{
    private readonly DormitoryDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public NotificationService(DormitoryDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task NotifyUserAsync(Guid userId, string title, string message, CancellationToken ct = default)
    {
        var notification = new Notification { Title = title, Message = message };
        notification.UserNotifications.Add(new UserNotification { UserId = userId });
        _dbContext.Notifications.Add(notification);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task NotifyRoleAsync(string roleName, string title, string message, CancellationToken ct = default)
    {
        var users = await _dbContext.Users.Where(x => x.Role != null && x.Role.Name == roleName).Select(x => x.Id).ToListAsync(ct);
        if (users.Count == 0)
        {
            return;
        }

        var notification = new Notification { Title = title, Message = message };
        foreach (var userId in users)
        {
            notification.UserNotifications.Add(new UserNotification { UserId = userId });
        }

        _dbContext.Notifications.Add(notification);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<NotificationDto>> GetCurrentUserNotificationsAsync(int take = 20, CancellationToken ct = default)
    {
        if (_currentUser.UserId is not { } userId)
        {
            return Array.Empty<NotificationDto>();
        }

        return await _dbContext.UserNotifications
            .Include(row => row.Notification)
            .AsNoTracking()
            .Where(row => row.UserId == userId && row.Notification != null)
            .OrderByDescending(row => row.Notification!.SentAt)
            .Take(Math.Clamp(take, 1, 100))
            .Select(row => new NotificationDto
            {
                Id = row.NotificationId,
                UserNotificationId = row.Id,
                Title = row.Notification!.Title,
                Message = row.Notification.Message,
                IsRead = row.IsRead,
                CreatedAt = row.Notification.SentAt
            })
            .ToListAsync(ct);
    }

    public async Task<int> GetCurrentUserUnreadCountAsync(CancellationToken ct = default)
    {
        if (_currentUser.UserId is not { } userId)
        {
            return 0;
        }

        return await _dbContext.UserNotifications.CountAsync(row => row.UserId == userId && !row.IsRead, ct);
    }

    public async Task MarkAsReadAsync(Guid userNotificationId, CancellationToken ct = default)
    {
        if (_currentUser.UserId is not { } userId)
        {
            return;
        }

        var row = await _dbContext.UserNotifications.FirstOrDefaultAsync(x => x.Id == userNotificationId && x.UserId == userId, ct);
        if (row is null || row.IsRead)
        {
            return;
        }

        row.IsRead = true;
        row.ReadAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task MarkAllCurrentUserNotificationsAsReadAsync(CancellationToken ct = default)
    {
        if (_currentUser.UserId is not { } userId)
        {
            return;
        }

        var rows = await _dbContext.UserNotifications
            .Where(row => row.UserId == userId && !row.IsRead)
            .ToListAsync(ct);
        foreach (var row in rows)
        {
            row.IsRead = true;
            row.ReadAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(ct);
    }
}
