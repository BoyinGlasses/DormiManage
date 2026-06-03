namespace DormitoryManagement.Application.DTOs.Students;

public sealed class StudentProfileDto
{
    public Guid StudentId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string StudentCode { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public DateTime? DateOfBirth { get; init; }
    public string Gender { get; init; } = string.Empty;
    public string AvatarAssetPath { get; init; } = string.Empty;
    public string ProfileStatusMessage { get; init; } = string.Empty;
    public string BuildingName { get; init; } = string.Empty;
    public string RoomLabel { get; init; } = string.Empty;
    public string AssignmentStatus { get; init; } = string.Empty;
    public string DormitorySupportMessage { get; init; } = string.Empty;
    public bool HasActiveAssignment { get; init; }
}
