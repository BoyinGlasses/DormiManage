namespace DormitoryManagement.Application.DTOs.Dashboard;

public sealed class DashboardRegistrationDto
{
    public Guid RegistrationId { get; set; }
    public string Student { get; set; } = string.Empty;
    public string PreferredRoom { get; set; } = string.Empty;
    public string SubmittedAt { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
