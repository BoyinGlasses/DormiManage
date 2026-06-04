using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Application.DTOs.Forum;

public sealed class ResolveForumReportRequest
{
    public Guid ReportId { get; set; }
    public ForumModerationAction Action { get; set; } = ForumModerationAction.Resolve;
    public string? ResolutionNote { get; set; }
}
