namespace DormitoryManagement.Application.DTOs.Auth;

public sealed record RememberedLoginState(string EmailOrStudentCode, string Password, bool HasPassword)
{
    public static RememberedLoginState Empty { get; } = new(string.Empty, string.Empty, false);
    public bool HasEmailOrStudentCode => !string.IsNullOrWhiteSpace(EmailOrStudentCode);
}
