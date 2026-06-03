namespace DormitoryManagement.Application.DTOs.Auth;

public sealed class StartAccountRegistrationResult
{
    public bool Succeeded { get; init; }
    public string? ErrorMessage { get; init; }
    public string? DiagnosticMessage { get; init; }
    public Guid? PendingRegistrationId { get; init; }
    public string? MaskedEmail { get; init; }
    public DateTime? ExpiresAtUtc { get; init; }
    public DateTime? ResendAvailableAtUtc { get; init; }

    public static StartAccountRegistrationResult Success(Guid pendingRegistrationId, string maskedEmail, DateTime expiresAtUtc, DateTime resendAvailableAtUtc) =>
        new()
        {
            Succeeded = true,
            PendingRegistrationId = pendingRegistrationId,
            MaskedEmail = maskedEmail,
            ExpiresAtUtc = expiresAtUtc,
            ResendAvailableAtUtc = resendAvailableAtUtc
        };

    public static StartAccountRegistrationResult Failed(string message, string? diagnosticMessage = null) =>
        new() { Succeeded = false, ErrorMessage = message, DiagnosticMessage = diagnosticMessage };
}
