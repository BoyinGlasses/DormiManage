using DormitoryManagement.Domain.Common;
using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Domain.Entities;

public class ForumPost : SoftDeleteEntity
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public ForumCategory? Category { get; set; }
    public Guid CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }
    public ForumVisibilityScope VisibilityScope { get; set; } = ForumVisibilityScope.Campus;
    public Guid? BuildingId { get; set; }
    public Building? Building { get; set; }
    public Guid? RoomId { get; set; }
    public Room? Room { get; set; }
    public string? TargetRoleName { get; set; }
    public ForumPostStatus Status { get; set; } = ForumPostStatus.Published;
    public DateTime? PublishedAt { get; set; } = DateTime.UtcNow;
    public ICollection<ForumComment> Comments { get; set; } = new List<ForumComment>();
    public ICollection<ForumPostTag> PostTags { get; set; } = new List<ForumPostTag>();
}
