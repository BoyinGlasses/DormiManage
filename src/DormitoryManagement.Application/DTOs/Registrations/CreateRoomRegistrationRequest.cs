using System.ComponentModel.DataAnnotations;

namespace DormitoryManagement.Application.DTOs.Registrations;

public sealed class CreateRoomRegistrationRequest
{
    [Required]
    public Guid StudentId { get; set; }

    [Required]
    public Guid RoomId { get; set; }

    public int ContractTermMonths { get; set; } = 12;

    public bool IncludesInternet { get; set; }

    [StringLength(500)]
    public string? Note { get; set; }
}
