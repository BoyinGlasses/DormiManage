namespace DormitoryManagement.Application.DTOs.Forum;

public sealed class ForumCommentDto
{
    public Guid Id { get; set; }
    public Guid ForumPostId { get; set; }
    public Guid AuthorUserId { get; set; }
    public Guid? ParentCommentId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string AuthorRole { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int LikeCount { get; set; }
    public bool IsLikedByCurrentUser { get; set; }
    public IReadOnlyList<ForumCommentDto> Replies { get; set; } = Array.Empty<ForumCommentDto>();
}
