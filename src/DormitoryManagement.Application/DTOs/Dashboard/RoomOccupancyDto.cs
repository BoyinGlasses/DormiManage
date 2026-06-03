namespace DormitoryManagement.Application.DTOs.Dashboard;

public sealed class RoomOccupancyDto
{
    public string BuildingCode { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int Occupied { get; set; }
}
