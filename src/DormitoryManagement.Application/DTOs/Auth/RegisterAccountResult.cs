namespace DormitoryManagement.Application.DTOs.Auth;

public sealed class RegisterAccountResult
{
    public bool Succeeded { get; init; }
    public string? ErrorMessage { get; init; }
    public Guid? UserId { get; init; }
    public Guid? StudentId { get; init; }

    public static RegisterAccountResult Success(Guid userId, Guid studentId) =>
        new() { Succeeded = true, UserId = userId, StudentId = studentId };

    public static RegisterAccountResult Failed(string message) =>
        new() { Succeeded = false, ErrorMessage = message };
}
