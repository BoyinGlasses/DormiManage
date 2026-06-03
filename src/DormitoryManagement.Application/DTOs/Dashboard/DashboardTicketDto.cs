namespace DormitoryManagement.Application.DTOs.Dashboard;

public sealed class DashboardTicketDto
{
    public string Ticket { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string AssignedStaff { get; set; } = string.Empty;
}
