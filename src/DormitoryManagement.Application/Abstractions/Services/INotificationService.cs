using DormitoryManagement.Application.DTOs.Notifications;

namespace DormitoryManagement.Application.Abstractions.Services;

public interface INotificationService
{
    Task NotifyUserAsync(Guid userId, string title, string message, CancellationToken ct = default);
    Task NotifyRoleAsync(string roleName, string title, string message, CancellationToken ct = default);
    Task<IReadOnlyList<NotificationDto>> GetCurrentUserNotificationsAsync(int take = 20, CancellationToken ct = default);
    Task<int> GetCurrentUserUnreadCountAsync(CancellationToken ct = default);
    Task MarkAsReadAsync(Guid userNotificationId, CancellationToken ct = default);
    Task MarkAllCurrentUserNotificationsAsReadAsync(CancellationToken ct = default);
}
