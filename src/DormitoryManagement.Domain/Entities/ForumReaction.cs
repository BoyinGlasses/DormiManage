using DormitoryManagement.Domain.Common;

namespace DormitoryManagement.Domain.Entities;

public class ForumReaction : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid? ForumPostId { get; set; }
    public Guid? ForumCommentId { get; set; }
    public string ReactionType { get; set; } = "Like";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public User? User { get; set; }
    public ForumPost? ForumPost { get; set; }
    public ForumComment? ForumComment { get; set; }
}
