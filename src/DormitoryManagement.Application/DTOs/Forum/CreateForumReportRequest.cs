namespace DormitoryManagement.Application.DTOs.Forum;

public sealed class CreateForumReportRequest
{
    public Guid? ForumPostId { get; set; }
    public Guid? ForumCommentId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Details { get; set; }
}
