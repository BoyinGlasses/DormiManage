using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Application.DTOs.Forum;

public sealed class ForumCommentDto
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public Guid? ParentCommentId { get; set; }
    public string Content { get; set; } = string.Empty;
    public ForumCommentStatus Status { get; set; }
    public ForumAuthorDto Author { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
}
