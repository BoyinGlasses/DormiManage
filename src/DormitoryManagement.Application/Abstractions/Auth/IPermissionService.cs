using DormitoryManagement.Application.Security;

namespace DormitoryManagement.Application.Abstractions.Auth;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(string permissionName, CancellationToken ct = default);
    Task<AuthorizationResult> AuthorizeAsync(string permissionName, CancellationToken ct = default);
    Task EnsurePermissionAsync(string permissionName, CancellationToken ct = default);
}
