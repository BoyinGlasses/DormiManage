using DormitoryManagement.Application.Common;
using DormitoryManagement.Application.DTOs.Users;

namespace DormitoryManagement.Application.Services.Users;

public interface IUserManagementService
{
    Task<PagedResult<UserListItemDto>> GetUsersAsync(int pageNumber = 1, int pageSize = 20, CancellationToken ct = default);
}
