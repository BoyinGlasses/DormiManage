using DormitoryManagement.Domain.Common;
using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Domain.Entities;

public class ForumPost : AuditableEntity
{
    public Guid AuthorUserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Excerpt { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Area { get; set; }
    public ForumVisibilityScope VisibilityScope { get; set; } = ForumVisibilityScope.Dormitory;
    public Guid? VisibilityBuildingId { get; set; }
    public Guid? VisibilityRoomId { get; set; }
    public string? VisibilityRoleName { get; set; }
    public ForumPostStatus Status { get; set; } = ForumPostStatus.Published;
    public bool IsPinned { get; set; }
    public bool IsImportant { get; set; }
    public int ViewCount { get; set; }
    public User? AuthorUser { get; set; }
    public ICollection<ForumPostTag> Tags { get; set; } = new List<ForumPostTag>();
    public ICollection<ForumComment> Comments { get; set; } = new List<ForumComment>();
    public ICollection<ForumReaction> Reactions { get; set; } = new List<ForumReaction>();
}
