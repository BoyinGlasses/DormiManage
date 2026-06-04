using DormitoryManagement.Application.Common;
using DormitoryManagement.Application.DTOs.Forum;

namespace DormitoryManagement.Application.Services.Forum;

public interface IForumModerationService
{
    Task<Result<ForumReportDto>> ReportAsync(CreateForumReportRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<ForumReportDto>> GetPendingReportsAsync(CancellationToken ct = default);
    Task<Result<ForumReportDto>> ResolveReportAsync(ResolveForumReportRequest request, CancellationToken ct = default);
}
