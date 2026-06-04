using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Application.DTOs.Forum;

public sealed class UpdateForumPostRequest
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Excerpt { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? Area { get; set; }
    public IReadOnlyCollection<string> Tags { get; set; } = Array.Empty<string>();
    public ForumVisibilityScope VisibilityScope { get; set; } = ForumVisibilityScope.Dormitory;
    public Guid? VisibilityBuildingId { get; set; }
    public Guid? VisibilityRoomId { get; set; }
    public string? VisibilityRoleName { get; set; }
    public bool IsPinned { get; set; }
    public bool IsImportant { get; set; }
}
