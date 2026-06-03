namespace DormitoryManagement.Application.DTOs.Forum;

public sealed class ForumAuthorDto
{
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string? StudentCode { get; set; }
    public string? StaffCode { get; set; }
    public Guid? BuildingId { get; set; }
}
