using System.ComponentModel.DataAnnotations;
using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Application.DTOs.Forum;

public sealed class CreateForumPostRequest
{
    [Required, StringLength(160)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(8000)]
    public string Content { get; set; } = string.Empty;

    [Required]
    public Guid CategoryId { get; set; }

    public IReadOnlyList<Guid> TagIds { get; set; } = Array.Empty<Guid>();
    public ForumVisibilityScope VisibilityScope { get; set; } = ForumVisibilityScope.Campus;
    public Guid? BuildingId { get; set; }
    public Guid? RoomId { get; set; }
    [StringLength(100)]
    public string? TargetRoleName { get; set; }
}
