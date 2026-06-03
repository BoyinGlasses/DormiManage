using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Application.DTOs.Forum;

public sealed class ForumPostFilterRequest
{
    public Guid? CategoryId { get; set; }
    public string? TagSlug { get; set; }
    public ForumVisibilityScope? VisibilityScope { get; set; }
    public Guid? BuildingId { get; set; }
    public Guid? RoomId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
