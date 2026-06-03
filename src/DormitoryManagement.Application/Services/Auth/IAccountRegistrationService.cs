using DormitoryManagement.Application.DTOs.Auth;

namespace DormitoryManagement.Application.Services.Auth;

public interface IAccountRegistrationService
{
    Task<StartAccountRegistrationResult> StartStudentAccountRegistrationAsync(RegisterAccountRequest request, CancellationToken ct = default);
    Task<RegisterAccountResult> VerifyStudentAccountOtpAsync(Guid pendingRegistrationId, string otpCode, CancellationToken ct = default);
    Task<StartAccountRegistrationResult> ResendStudentAccountOtpAsync(Guid pendingRegistrationId, CancellationToken ct = default);
}
