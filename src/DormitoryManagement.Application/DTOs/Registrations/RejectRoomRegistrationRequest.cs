using System.ComponentModel.DataAnnotations;

namespace DormitoryManagement.Application.DTOs.Registrations;

public sealed class RejectRoomRegistrationRequest
{
    [Required]
    public Guid RegistrationId { get; set; }

    [Required, StringLength(500)]
    public string Reason { get; set; } = string.Empty;
}
