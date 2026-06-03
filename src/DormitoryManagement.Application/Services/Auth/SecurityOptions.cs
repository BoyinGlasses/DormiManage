namespace DormitoryManagement.Application.Services.Auth;

public sealed class SecurityOptions
{
    public const string SectionName = "Security";

    public int MaxFailedLoginAttempts { get; set; } = 5;
    public int LockoutMinutes { get; set; } = 5;
    public int SessionTimeoutMinutes { get; set; } = 60;
}
