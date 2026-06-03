using DormitoryManagement.Application.DTOs.Auth;

namespace DormitoryManagement.Application.Abstractions.Auth;

public interface ICurrentUserService
{
    CurrentUserDto? CurrentUser { get; }
    Guid? UserId { get; }
    string? UserName { get; }
    string? Email { get; }
    string? FullName { get; }
    IReadOnlyCollection<string> Roles { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string roleName);
}
