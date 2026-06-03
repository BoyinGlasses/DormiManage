namespace DormitoryManagement.Application.DTOs.Auth;

public sealed class CurrentUserDto
{
    public Guid UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string RoleName { get; init; } = string.Empty;
    public Guid? StudentId { get; init; }
    public Guid? ManagerId { get; init; }
    public Guid? BuildingId { get; init; }
}
