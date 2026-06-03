using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Application.DTOs.Registrations;

public sealed class RoomRegistrationDto
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public Guid RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string BuildingName { get; set; } = string.Empty;
    public RegistrationStatus Status { get; set; }
    public int ContractTermMonths { get; set; }
    public bool IncludesInternet { get; set; }
    public DateTime RequestedAt { get; set; }
    public string? RejectionReason { get; set; }
}
