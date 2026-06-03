using DormitoryManagement.Application.Abstractions.Auth;
using DormitoryManagement.Application.Abstractions.Data;
using DormitoryManagement.Domain.Constants;

namespace DormitoryManagement.Application.Services.Contracts;

public sealed class ContractService : IContractService
{
    private readonly IPermissionService _permissions;
    private readonly IUnitOfWork _unitOfWork;

    public ContractService(IPermissionService permissions, IUnitOfWork unitOfWork)
    {
        _permissions = permissions;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> CreateContractAsync(Guid studentId, Guid roomId, DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.ContractsWrite, ct);
        // TODO: Create contract after active assignment exists and no overlapping active contract.
        return Guid.NewGuid();
    }

    public Task<Guid?> GetActiveContractByStudentAsync(Guid studentId, CancellationToken ct = default)
    {
        // TODO: Students may only read their own contract; managers are scoped by building.
        return Task.FromResult<Guid?>(null);
    }

    public async Task TerminateContractAsync(Guid contractId, string reason, CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.ContractsWrite, ct);
        await using var tx = await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            // TODO: Terminate contract, end assignment, adjust room occupancy, audit transfer/checkout.
            await _unitOfWork.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }
}
