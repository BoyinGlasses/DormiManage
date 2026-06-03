using System.ComponentModel.DataAnnotations;

namespace DormitoryManagement.Application.DTOs.Students;

public sealed class CreateStudentRequest
{
    [Required, StringLength(30)]
    public string StudentCode { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string FullName { get; set; } = string.Empty;

    [EmailAddress, StringLength(256)]
    public string? Email { get; set; }

    [Phone, StringLength(30)]
    public string? PhoneNumber { get; set; }

    [StringLength(100)]
    public string? Department { get; set; }

    [StringLength(50)]
    public string? ClassName { get; set; }
}
