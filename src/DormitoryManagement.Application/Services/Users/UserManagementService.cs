using DormitoryManagement.Application.Abstractions.Auth;
using DormitoryManagement.Application.Common;
using DormitoryManagement.Application.DTOs.Users;
using DormitoryManagement.Domain.Constants;

namespace DormitoryManagement.Application.Services.Users;

public sealed class UserManagementService : IUserManagementService
{
    private readonly IPermissionService _permissions;

    public UserManagementService(IPermissionService permissions)
    {
        _permissions = permissions;
    }

    public async Task<PagedResult<UserListItemDto>> GetUsersAsync(int pageNumber = 1, int pageSize = 20, CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.UsersManage, ct);
        return PagedResult<UserListItemDto>.Empty(pageNumber, pageSize);
    }
}
