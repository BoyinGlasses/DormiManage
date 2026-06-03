using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Application.DTOs.Users;

public sealed class UserListItemDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public UserStatus Status { get; set; }
}
