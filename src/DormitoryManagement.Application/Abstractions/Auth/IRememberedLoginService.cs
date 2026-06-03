using DormitoryManagement.Application.DTOs.Auth;

namespace DormitoryManagement.Application.Abstractions.Auth;

public interface IRememberedLoginService
{
    RememberedLoginState Load();
    void SaveFullLogin(string emailOrStudentCode, string password);
    void SaveEmailOnly(string emailOrStudentCode);
    void DowngradeToEmailOnly(string emailOrStudentCode);
    void Clear();
}
