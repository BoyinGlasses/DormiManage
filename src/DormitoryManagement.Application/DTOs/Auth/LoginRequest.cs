using System.ComponentModel.DataAnnotations;

namespace DormitoryManagement.Application.DTOs.Auth;

public sealed class LoginRequest
{
    [Required, StringLength(256)]
    public string EmailOrUsernameOrStudentCode { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;
}
