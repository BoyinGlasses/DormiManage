namespace DormitoryManagement.Application.Services.Contracts;

public interface IContractService
{
    Task<Guid> CreateContractAsync(Guid studentId, Guid roomId, DateTime startDate, DateTime endDate, CancellationToken ct = default);
    Task<Guid?> GetActiveContractByStudentAsync(Guid studentId, CancellationToken ct = default);
    Task TerminateContractAsync(Guid contractId, string reason, CancellationToken ct = default);
}
