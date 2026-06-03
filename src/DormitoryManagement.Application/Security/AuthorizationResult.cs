namespace DormitoryManagement.Application.Security;

public sealed record AuthorizationResult(bool Succeeded, string? ErrorMessage = null)
{
    public static AuthorizationResult Success() => new(true);
    public static AuthorizationResult Denied(string message) => new(false, message);
}
