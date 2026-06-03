using DormitoryManagement.Domain.Common;
using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Domain.Entities;

public class ForumComment : AuditableEntity
{
    public Guid ForumPostId { get; set; }
    public Guid AuthorUserId { get; set; }
    public Guid? ParentCommentId { get; set; }
    public string Content { get; set; } = string.Empty;
    public ForumCommentStatus Status { get; set; } = ForumCommentStatus.Published;
    public ForumPost? ForumPost { get; set; }
    public User? AuthorUser { get; set; }
    public ForumComment? ParentComment { get; set; }
    public ICollection<ForumComment> Replies { get; set; } = new List<ForumComment>();
    public ICollection<ForumReaction> Reactions { get; set; } = new List<ForumReaction>();
}
