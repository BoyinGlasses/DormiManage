using DormitoryManagement.Application.Abstractions.Auth;
using DormitoryManagement.Application.DTOs.Auth;

namespace DormitoryManagement.Infrastructure.Services;

public sealed class SessionService : ISessionService, ICurrentUserService
{
    public CurrentUserDto? CurrentUser { get; private set; }

    public Guid? UserId => CurrentUser?.UserId;
    public string? UserName => CurrentUser?.Username;
    public string? Email => CurrentUser?.Email;
    public string? FullName => CurrentUser?.FullName;
    public IReadOnlyCollection<string> Roles => CurrentUser?.RoleName is { Length: > 0 } roleName ? new[] { roleName } : Array.Empty<string>();
    public bool IsAuthenticated => CurrentUser is not null;

    public void SetCurrentUser(CurrentUserDto user)
    {
        ArgumentNullException.ThrowIfNull(user);
        CurrentUser = user;
    }

    public void Clear()
    {
        CurrentUser = null;
    }

    public bool IsInRole(string roleName) => Roles.Contains(roleName, StringComparer.OrdinalIgnoreCase);
}
