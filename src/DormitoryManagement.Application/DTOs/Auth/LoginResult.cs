namespace DormitoryManagement.Application.DTOs.Auth;

public sealed class LoginResult
{
    public bool Succeeded { get; init; }
    public string? ErrorMessage { get; init; }
    public LoginFailureReason FailureReason { get; init; } = LoginFailureReason.None;
    public CurrentUserDto? User { get; init; }

    public static LoginResult Success(CurrentUserDto user) => new() { Succeeded = true, User = user };

    public static LoginResult Failed(string message, LoginFailureReason failureReason = LoginFailureReason.General) =>
        new() { Succeeded = false, ErrorMessage = message, FailureReason = failureReason };
}

public enum LoginFailureReason
{
    None,
    AccountNotFound,
    InvalidPassword,
    Disabled,
    Locked,
    General
}
