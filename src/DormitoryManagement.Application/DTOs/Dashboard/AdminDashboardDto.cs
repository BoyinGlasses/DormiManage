namespace DormitoryManagement.Application.DTOs.Dashboard;

public sealed class AdminDashboardDto
{
    public int TotalStudents { get; set; }
    public int OccupiedRooms { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public int OpenTickets { get; set; }
}
