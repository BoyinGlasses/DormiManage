using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Application.DTOs.Forum;

public sealed class ForumPostDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public ForumCategoryDto Category { get; set; } = new();
    public IReadOnlyList<ForumTagDto> Tags { get; set; } = Array.Empty<ForumTagDto>();
    public IReadOnlyList<ForumCommentDto> Comments { get; set; } = Array.Empty<ForumCommentDto>();
    public ForumAuthorDto Author { get; set; } = new();
    public ForumVisibilityScope VisibilityScope { get; set; }
    public Guid? BuildingId { get; set; }
    public Guid? RoomId { get; set; }
    public string? TargetRoleName { get; set; }
    public ForumPostStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public bool CanModerate { get; set; }
}
