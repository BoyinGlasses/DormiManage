using DormitoryManagement.Application.DTOs.Auth;

namespace DormitoryManagement.Application.Abstractions.Auth;

public interface ISessionService
{
    CurrentUserDto? CurrentUser { get; }
    void SetCurrentUser(CurrentUserDto user);
    void Clear();
}
