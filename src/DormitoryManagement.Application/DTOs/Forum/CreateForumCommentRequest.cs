namespace DormitoryManagement.Application.DTOs.Forum;

public sealed class CreateForumCommentRequest
{
    public Guid ForumPostId { get; set; }
    public Guid? ParentCommentId { get; set; }
    public string Content { get; set; } = string.Empty;
}
