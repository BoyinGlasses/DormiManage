using DormitoryManagement.Application.DTOs.Audit;

namespace DormitoryManagement.Application.Abstractions.Services;

public interface IAuditLogService
{
    Task WriteAsync(string action, string entityName, Guid? entityId = null, string? details = null, CancellationToken ct = default);
    Task<IReadOnlyList<AuditLogDto>> GetRecentAsync(string? searchText = null, int take = 100, CancellationToken ct = default);
}
