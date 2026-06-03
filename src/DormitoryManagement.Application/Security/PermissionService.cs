using DormitoryManagement.Application.Abstractions.Auth;
using DormitoryManagement.Domain.Constants;

namespace DormitoryManagement.Application.Security;

public sealed class PermissionService : IPermissionService
{
    private readonly ICurrentUserService _currentUser;

    public PermissionService(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    public Task<bool> HasPermissionAsync(string permissionName, CancellationToken ct = default)
    {
        if (!_currentUser.IsAuthenticated)
        {
            return Task.FromResult(false);
        }

        if (_currentUser.IsInRole(RoleNames.Admin))
        {
            return Task.FromResult(true);
        }

        var allowed = permissionName switch
        {
            PermissionNames.DashboardAdmin
                or PermissionNames.StudentsRead
                or PermissionNames.RoomsRead
                or PermissionNames.RoomRegistrationApprove
                or PermissionNames.BillingRead
                or PermissionNames.BillingWrite
                or PermissionNames.PaymentsConfirm
                or PermissionNames.TicketsRead
                or PermissionNames.TicketsCreate
                or PermissionNames.TicketsAssign
                or PermissionNames.TicketsUpdate
                or PermissionNames.ForumPostsRead
                or PermissionNames.ForumPostsCreate
                or PermissionNames.ForumPostsUpdateOwn
                or PermissionNames.ForumPostsDeleteOwn
                or PermissionNames.ForumPostsModerate when _currentUser.IsInRole(RoleNames.Manager)
                    || _currentUser.IsInRole(RoleNames.BuildingManager) => true,
            PermissionNames.TicketsRead
                or PermissionNames.TicketsCreate
                or PermissionNames.TicketsAssign
                or PermissionNames.TicketsUpdate
                or PermissionNames.ForumPostsRead
                or PermissionNames.ForumPostsCreate
                or PermissionNames.ForumPostsUpdateOwn
                or PermissionNames.ForumPostsDeleteOwn
                or PermissionNames.ForumPostsModerate when _currentUser.IsInRole(RoleNames.Staff) => true,
            PermissionNames.RoomsRead
                or PermissionNames.RoomRegistrationCreate
                or PermissionNames.BillingRead
                or PermissionNames.PaymentsCreate
                or PermissionNames.TicketsRead
                or PermissionNames.TicketsCreate
                or PermissionNames.ForumPostsRead
                or PermissionNames.ForumPostsCreate
                or PermissionNames.ForumPostsUpdateOwn
                or PermissionNames.ForumPostsDeleteOwn when _currentUser.IsInRole(RoleNames.Student) => true,
            _ => false
        };

        // TODO: Replace role-name shortcut checks with persisted role-permission mapping.
        return Task.FromResult(allowed);
    }

    public async Task<AuthorizationResult> AuthorizeAsync(string permissionName, CancellationToken ct = default)
    {
        return await HasPermissionAsync(permissionName, ct)
            ? AuthorizationResult.Success()
            : AuthorizationResult.Denied($"Access denied for permission '{permissionName}'.");
    }

    public async Task EnsurePermissionAsync(string permissionName, CancellationToken ct = default)
    {
        var result = await AuthorizeAsync(permissionName, ct);
        if (!result.Succeeded)
        {
            throw new AccessDeniedException(result.ErrorMessage ?? "Access denied.");
        }
    }
}
