using DormitoryManagement.Domain.Common;
using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Domain.Entities;

public class ForumReport : AuditableEntity
{
    public Guid ReporterUserId { get; set; }
    public ForumReportTargetType TargetType { get; set; }
    public Guid? ForumPostId { get; set; }
    public Guid? ForumCommentId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Details { get; set; }
    public ForumReportStatus Status { get; set; } = ForumReportStatus.Pending;
    public Guid? ReviewedByUserId { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public ForumModerationAction? ResolutionAction { get; set; }
    public string? ResolutionNote { get; set; }
    public User? ReporterUser { get; set; }
    public User? ReviewedByUser { get; set; }
    public ForumPost? ForumPost { get; set; }
    public ForumComment? ForumComment { get; set; }
}
