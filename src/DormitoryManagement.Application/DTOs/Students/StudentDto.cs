using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Application.DTOs.Students;

public sealed class StudentDto
{
    public Guid Id { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public StudentStatus Status { get; set; }
    public Guid? CurrentRoomId { get; set; }
}
