using DormitoryManagement.Domain.Common;
using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Domain.Entities;

public class ForumComment : SoftDeleteEntity
{
    public Guid PostId { get; set; }
    public ForumPost? Post { get; set; }
    public Guid? ParentCommentId { get; set; }
    public ForumComment? ParentComment { get; set; }
    public Guid CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }
    public string Content { get; set; } = string.Empty;
    public ForumCommentStatus Status { get; set; } = ForumCommentStatus.Published;
    public ICollection<ForumComment> Replies { get; set; } = new List<ForumComment>();
}
