using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Application.DTOs.Forum;

public sealed class ForumPostFilterRequest
{
    public string? SearchText { get; set; }
    public string? Category { get; set; }
    public string? Tag { get; set; }
    public string? Area { get; set; }
    public ForumPostSortOption SortBy { get; set; } = ForumPostSortOption.Newest;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? CurrentUserRoleName { get; set; }
    public Guid? CurrentUserBuildingId { get; set; }
    public Guid? CurrentUserRoomId { get; set; }
    public bool CanViewAllScopes { get; set; }
}
