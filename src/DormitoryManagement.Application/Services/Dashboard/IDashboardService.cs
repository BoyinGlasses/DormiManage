using DormitoryManagement.Application.DTOs.Dashboard;

namespace DormitoryManagement.Application.Services.Dashboard;

public interface IDashboardService
{
    Task<AdminDashboardDto> GetAdminDashboardAsync(CancellationToken ct = default);
    Task<StudentDashboardDto> GetStudentDashboardAsync(Guid studentId, CancellationToken ct = default);
    Task<IReadOnlyList<ChartPointDto>> GetRevenueByMonthAsync(int year, CancellationToken ct = default);
    Task<IReadOnlyList<RoomOccupancyDto>> GetRoomOccupancyAsync(CancellationToken ct = default);
    Task<DebtSummaryDto> GetDebtSummaryAsync(CancellationToken ct = default);
    Task<IReadOnlyList<DashboardRegistrationDto>> GetPendingRegistrationsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<DashboardInvoiceDto>> GetOverdueInvoicesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<DashboardTicketDto>> GetOpenSupportTicketsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<DashboardActivityDto>> GetRecentActivitiesAsync(CancellationToken ct = default);
}
