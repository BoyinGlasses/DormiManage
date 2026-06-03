using DormitoryManagement.Application.Abstractions.Auth;
using DormitoryManagement.Application.Common;
using DormitoryManagement.Application.DTOs.Settings;
using DormitoryManagement.Domain.Constants;

namespace DormitoryManagement.Application.Services.Settings;

public sealed class FeeTypeService : IFeeTypeService
{
    private readonly IPermissionService _permissions;

    public FeeTypeService(IPermissionService permissions)
    {
        _permissions = permissions;
    }

    public async Task<PagedResult<FeeTypeDto>> GetFeeTypesAsync(int pageNumber = 1, int pageSize = 20, CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.BillingWrite, ct);
        return PagedResult<FeeTypeDto>.Empty(pageNumber, pageSize);
    }
}
