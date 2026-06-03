using System.ComponentModel.DataAnnotations;

namespace DormitoryManagement.Application.DTOs.Students;

public sealed class UpdateStudentRequest
{
    [Required]
    public Guid StudentId { get; set; }

    [Required, StringLength(200)]
    public string FullName { get; set; } = string.Empty;

    [EmailAddress, StringLength(256)]
    public string? Email { get; set; }

    [Phone, StringLength(30)]
    public string? PhoneNumber { get; set; }
}
