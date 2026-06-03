namespace DormitoryManagement.Application.DTOs.Dashboard;

public sealed class StudentDashboardDto
{
    public string? CurrentRoom { get; set; }
    public string? RequestedRoom { get; set; }
    public string RoomCardDisplayMode { get; set; } = "Empty";
    public string RoomCardStatusText { get; set; } = "Chưa phân phòng";
    public bool CanOpenRoomRegistrationPopup { get; set; } = true;
    public string? RoomCardLockReason { get; set; }
    public decimal OutstandingDebt { get; set; }
    public int OpenTickets { get; set; }
    public int UnreadNotifications { get; set; }
}