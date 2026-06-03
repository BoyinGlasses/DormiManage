using DormitoryManagement.Application.Common;
using DormitoryManagement.Application.DTOs.Settings;

namespace DormitoryManagement.Application.Services.Settings;

public interface IFeeTypeService
{
    Task<PagedResult<FeeTypeDto>> GetFeeTypesAsync(int pageNumber = 1, int pageSize = 20, CancellationToken ct = default);
}
