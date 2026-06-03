using System.ComponentModel.DataAnnotations;

namespace DormitoryManagement.Application.DTOs.Auth;

public sealed class ChangePasswordRequest
{
    [Required, MinLength(6)]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string NewPassword { get; set; } = string.Empty;
}
