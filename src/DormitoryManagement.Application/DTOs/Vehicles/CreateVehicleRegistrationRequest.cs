using System.ComponentModel.DataAnnotations;

namespace DormitoryManagement.Application.DTOs.Vehicles;

public sealed class CreateVehicleRegistrationRequest
{
    [Required, StringLength(20)]
    public string LicensePlate { get; set; } = string.Empty;

    [Range(1, 12)]
    public int MonthCount { get; set; }
}
