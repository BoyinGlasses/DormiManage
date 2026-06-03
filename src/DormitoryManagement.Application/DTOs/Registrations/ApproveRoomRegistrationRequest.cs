using System.ComponentModel.DataAnnotations;

namespace DormitoryManagement.Application.DTOs.Registrations;

public sealed class ApproveRoomRegistrationRequest
{
    [Required]
    public Guid RegistrationId { get; set; }

    [Required]
    public DateTime StartDate { get; set; }
}
