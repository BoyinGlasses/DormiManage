using DormitoryManagement.Application.Abstractions.Auth;
using DormitoryManagement.Application.Abstractions.Data;
using DormitoryManagement.Application.Abstractions.Repositories;
using DormitoryManagement.Application.DTOs.Dashboard;
using DormitoryManagement.Domain.Constants;
using DormitoryManagement.Domain.Entities;
using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Application.Services.Dashboard;

public sealed class DashboardService : IDashboardService
{
    private readonly IPermissionService _permissions;
    private readonly ICurrentUserService _currentUser;
    private readonly IStudentRepository _students;
    private readonly IRoomRepository _rooms;
    private readonly IInvoiceRepository _invoices;
    private readonly IPaymentRepository _payments;
    private readonly ISupportTicketRepository _tickets;
    private readonly IUnitOfWork _unitOfWork;

    public DashboardService(
        IPermissionService permissions,
        ICurrentUserService currentUser,
        IStudentRepository students,
        IRoomRepository rooms,
        IInvoiceRepository invoices,
        IPaymentRepository payments,
        ISupportTicketRepository tickets,
        IUnitOfWork unitOfWork)
    {
        _permissions = permissions;
        _currentUser = currentUser;
        _students = students;
        _rooms = rooms;
        _invoices = invoices;
        _payments = payments;
        _tickets = tickets;
        _unitOfWork = unitOfWork;
    }

    public async Task<AdminDashboardDto> GetAdminDashboardAsync(CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.DashboardAdmin, ct);
        var today = DateTime.Today;
        var firstDay = new DateTime(today.Year, today.Month, 1);
        var openTicketStatuses = new[] { SupportTicketStatus.New, SupportTicketStatus.Assigned, SupportTicketStatus.InProgress };
        var activeAssignments = _unitOfWork.Repository<RoomAssignment>().Query().Count(assignment => assignment.IsActive);

        return new AdminDashboardDto
        {
            TotalStudents = _students.Query().Count(student => !student.IsDeleted),
            OccupiedRooms = activeAssignments > 0 ? activeAssignments : _rooms.Query().Sum(room => room.CurrentOccupancy),
            MonthlyRevenue = _payments.Query().Where(payment =>
                payment.Status == PaymentStatus.Success &&
                payment.PaidAt.HasValue &&
                payment.PaidAt.Value >= firstDay &&
                payment.PaidAt.Value < firstDay.AddMonths(1)).Sum(payment => payment.Amount),
            OpenTickets = _tickets.Query().Count(ticket => openTicketStatuses.Contains(ticket.Status))
        };
    }

    public Task<StudentDashboardDto> GetStudentDashboardAsync(Guid studentId, CancellationToken ct = default)
    {
        studentId = ResolveStudentId(studentId);
        var student = _unitOfWork.Repository<Student>().Query().FirstOrDefault(candidate => candidate.Id == studentId)
            ?? throw new InvalidOperationException("Student was not found.");

        var currentAssignment = _unitOfWork.Repository<RoomAssignment>().Query()
            .Where(assignment => assignment.StudentId == studentId && assignment.IsActive)
            .OrderByDescending(assignment => assignment.StartDate)
            .FirstOrDefault();

        var currentRoom = currentAssignment is not null
            ? ResolveRoomLabel(currentAssignment.RoomId)
            : null;

        if (string.IsNullOrWhiteSpace(currentRoom) && student.CurrentRoomId.HasValue)
        {
            currentRoom = ResolveRoomLabel(student.CurrentRoomId.Value);
        }

        var pendingStatuses = new[] { RegistrationStatus.Pending, RegistrationStatus.PaymentPending };
        var pendingRegistration = string.IsNullOrWhiteSpace(currentRoom)
            ? _unitOfWork.Repository<RoomRegistration>().Query()
                .Where(registration => registration.StudentId == studentId && pendingStatuses.Contains(registration.Status))
                .OrderByDescending(registration => registration.RequestedAt)
                .FirstOrDefault()
            : null;

        if (string.IsNullOrWhiteSpace(currentRoom))
        {
            var approvedRegistration = _unitOfWork.Repository<RoomRegistration>().Query()
                .Where(registration => registration.StudentId == studentId && registration.Status == RegistrationStatus.Approved)
                .OrderByDescending(registration => registration.ReviewedAt ?? registration.RequestedAt)
                .FirstOrDefault();
            if (approvedRegistration is not null)
            {
                currentRoom = ResolveRoomLabel(approvedRegistration.RoomId);
            }
        }

        var roomCardDisplayMode = "Empty";
        var roomCardStatusText = "Chưa phân phòng";
        var canOpenRoomRegistrationPopup = true;
        string? requestedRoom = null;
        string? roomCardLockReason = null;

        if (!string.IsNullOrWhiteSpace(currentRoom))
        {
            roomCardDisplayMode = "Assigned";
            roomCardStatusText = "Đã phân phòng";
            canOpenRoomRegistrationPopup = false;
            roomCardLockReason = "Sinh viên đã có phòng được duyệt.";
        }
        else if (pendingRegistration is not null)
        {
            roomCardDisplayMode = "Pending";
            requestedRoom = ResolveRoomLabel(pendingRegistration.RoomId);
            roomCardStatusText = "(đang chờ xử lí)";
            canOpenRoomRegistrationPopup = false;
            roomCardLockReason = pendingRegistration.Status == RegistrationStatus.PaymentPending
                ? "Yêu cầu đăng ký đang chờ thanh toán hợp đồng."
                : "Yêu cầu đăng ký đang chờ xử lí.";
        }

        var dueStatuses = new[] { InvoiceStatus.Unpaid, InvoiceStatus.Overdue };
        var openTicketStatuses = new[] { SupportTicketStatus.New, SupportTicketStatus.Assigned, SupportTicketStatus.InProgress };
        var unreadNotifications = student.UserId.HasValue
            ? _unitOfWork.Repository<UserNotification>().Query().Count(row => row.UserId == student.UserId.Value && !row.IsRead)
            : 0;

        return Task.FromResult(new StudentDashboardDto
        {
            CurrentRoom = currentRoom,
            RequestedRoom = requestedRoom,
            RoomCardDisplayMode = roomCardDisplayMode,
            RoomCardStatusText = roomCardStatusText,
            CanOpenRoomRegistrationPopup = canOpenRoomRegistrationPopup,
            RoomCardLockReason = roomCardLockReason,
            OutstandingDebt = _unitOfWork.Repository<Invoice>().Query()
                .Where(invoice => invoice.StudentId == studentId
                    && dueStatuses.Contains(invoice.Status)
                    && invoice.TotalAmount > invoice.PaidAmount)
                .Sum(invoice => invoice.TotalAmount - invoice.PaidAmount),
            OpenTickets = _unitOfWork.Repository<SupportTicket>().Query()
                .Count(ticket => ticket.StudentId == studentId && openTicketStatuses.Contains(ticket.Status)),
            UnreadNotifications = unreadNotifications
        });
    }

    private Guid ResolveStudentId(Guid requestedStudentId)
    {
        if (requestedStudentId != Guid.Empty)
        {
            return requestedStudentId;
        }

        if (_currentUser.CurrentUser?.StudentId is { } currentStudentId)
        {
            return currentStudentId;
        }

        if (_currentUser.UserId is { } userId)
        {
            var student = _unitOfWork.Repository<Student>().Query().FirstOrDefault(candidate => candidate.UserId == userId);
            if (student is not null)
            {
                return student.Id;
            }
        }

        throw new InvalidOperationException("Current user is not linked to a student profile.");
    }

    private string? ResolveRoomLabel(Guid roomId)
    {
        var room = _unitOfWork.Repository<Room>().Query().FirstOrDefault(candidate => candidate.Id == roomId);
        if (room is null)
        {
            return null;
        }

        var building = _unitOfWork.Repository<Building>().Query().FirstOrDefault(candidate => candidate.Id == room.BuildingId);
        return string.IsNullOrWhiteSpace(building?.Code) ? room.RoomNumber : $"{building.Code}-{room.RoomNumber}";
    }

    public Task<IReadOnlyList<ChartPointDto>> GetRevenueByMonthAsync(int year, CancellationToken ct = default)
    {
        var paidByMonth = _payments.Query()
            .Where(payment => payment.Status == PaymentStatus.Success && payment.PaidAt.HasValue && payment.PaidAt.Value.Year == year)
            .GroupBy(payment => payment.PaidAt!.Value.Month)
            .Select(group => new { Month = group.Key, Amount = group.Sum(payment => payment.Amount) })
            .ToDictionary(point => point.Month, point => point.Amount);

        IReadOnlyList<ChartPointDto> result = Enumerable.Range(1, 12)
            .Select(month => new ChartPointDto
            {
                Label = new DateTime(year, month, 1).ToString("MMM"),
                Value = paidByMonth.TryGetValue(month, out var amount) ? amount : 0m
            })
            .ToList();

        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<RoomOccupancyDto>> GetRoomOccupancyAsync(CancellationToken ct = default)
    {
        var activeAssignments = _unitOfWork.Repository<RoomAssignment>().Query()
            .Where(assignment => assignment.IsActive)
            .GroupBy(assignment => assignment.RoomId)
            .ToDictionary(group => group.Key, group => group.Count());
        var rooms = _rooms.Query().ToList();
        var buildingIds = rooms.Select(room => room.BuildingId).ToHashSet();
        var buildings = _unitOfWork.Repository<Building>().Query()
            .Where(building => buildingIds.Contains(building.Id))
            .ToDictionary(building => building.Id);

        IReadOnlyList<RoomOccupancyDto> result = rooms
            .GroupBy(room => buildings.TryGetValue(room.BuildingId, out var building) ? building.Code : string.Empty)
            .Select(group => new RoomOccupancyDto
            {
                BuildingCode = group.Key,
                Capacity = group.Sum(room => room.Capacity),
                Occupied = group.Sum(room =>
                    Math.Max(room.CurrentOccupancy, activeAssignments.TryGetValue(room.Id, out var assigned) ? assigned : 0))
            })
            .OrderBy(point => point.BuildingCode)
            .ToList();

        return Task.FromResult(result);
    }

    public Task<DebtSummaryDto> GetDebtSummaryAsync(CancellationToken ct = default)
    {
        var today = DateTime.Today;
        var dueStatuses = new[] { InvoiceStatus.Unpaid, InvoiceStatus.Overdue };
        var invoices = _invoices.Query()
            .Where(invoice => dueStatuses.Contains(invoice.Status) && invoice.TotalAmount > invoice.PaidAmount)
            .Select(invoice => new
            {
                invoice.StudentId,
                Remaining = invoice.TotalAmount - invoice.PaidAmount,
                IsOverdue = invoice.Status == InvoiceStatus.Overdue || invoice.DueDate < today
            })
            .ToList();

        return Task.FromResult(new DebtSummaryDto
        {
            TotalDebt = invoices.Sum(invoice => invoice.Remaining),
            StudentCount = invoices.Select(invoice => invoice.StudentId).Distinct().Count(),
            UnpaidInvoiceCount = invoices.Count,
            OverdueInvoiceCount = invoices.Count(invoice => invoice.IsOverdue)
        });
    }

    public Task<IReadOnlyList<DashboardRegistrationDto>> GetPendingRegistrationsAsync(CancellationToken ct = default)
    {
        var rows = _unitOfWork.Repository<RoomRegistration>().Query()
            .Where(registration => registration.Status == RegistrationStatus.Pending)
            .OrderByDescending(registration => registration.RequestedAt)
            .Take(6)
            .Select(registration => new
            {
                registration.Id,
                Student = registration.Student!.FullName,
                PreferredRoom = registration.Room!.Building!.Code + "-" + registration.Room.RoomNumber,
                registration.RequestedAt,
                registration.Status
            })
            .ToList();

        IReadOnlyList<DashboardRegistrationDto> result = rows.Select(registration => new DashboardRegistrationDto
        {
            RegistrationId = registration.Id,
            Student = registration.Student,
            PreferredRoom = registration.PreferredRoom,
            SubmittedAt = registration.RequestedAt.ToString("dd MMM"),
            Status = registration.Status.ToString()
        }).ToList();

        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<DashboardInvoiceDto>> GetOverdueInvoicesAsync(CancellationToken ct = default)
    {
        var today = DateTime.Today;
        var rows = _invoices.Query()
            .Where(invoice => invoice.TotalAmount > invoice.PaidAmount &&
                (invoice.Status == InvoiceStatus.Overdue || invoice.DueDate < today))
            .OrderBy(invoice => invoice.DueDate)
            .Take(6)
            .Select(invoice => new
            {
                invoice.InvoiceNumber,
                Student = invoice.Student!.FullName,
                invoice.DueDate,
                Remaining = invoice.TotalAmount - invoice.PaidAmount,
                invoice.Status
            })
            .ToList();

        IReadOnlyList<DashboardInvoiceDto> result = rows.Select(invoice => new DashboardInvoiceDto
        {
            InvoiceNumber = invoice.InvoiceNumber,
            Student = invoice.Student,
            DueDate = invoice.DueDate.ToString("dd MMM yyyy"),
            Remaining = invoice.Remaining,
            Status = invoice.Status == InvoiceStatus.Overdue ? "Overdue" : "Unpaid"
        }).ToList();

        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<DashboardTicketDto>> GetOpenSupportTicketsAsync(CancellationToken ct = default)
    {
        var openStatuses = new[] { SupportTicketStatus.New, SupportTicketStatus.Assigned, SupportTicketStatus.InProgress };
        var rows = _tickets.Query()
            .Where(ticket => openStatuses.Contains(ticket.Status))
            .OrderByDescending(ticket => ticket.Priority)
            .ThenByDescending(ticket => ticket.CreatedAt)
            .Take(6)
            .Select(ticket => new
            {
                ticket.Title,
                ticket.Priority,
                ticket.Status,
                AssignedStaff = ticket.AssignedToManager != null ? ticket.AssignedToManager.FullName : "Unassigned"
            })
            .ToList();

        IReadOnlyList<DashboardTicketDto> result = rows.Select(ticket => new DashboardTicketDto
        {
            Ticket = ticket.Title,
            Priority = ticket.Priority.ToString(),
            Status = ticket.Status == SupportTicketStatus.New ? "Open" : ticket.Status.ToString(),
            AssignedStaff = ticket.AssignedStaff
        }).ToList();

        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<DashboardActivityDto>> GetRecentActivitiesAsync(CancellationToken ct = default)
    {
        var rows = _unitOfWork.Repository<AuditLog>().Query()
            .OrderByDescending(log => log.CreatedAt)
            .Take(8)
            .Select(ticket => new
            {
                ticket.CreatedAt,
                ticket.Action,
                ticket.EntityName,
                ticket.Details
            })
            .ToList()
            .Select(log => new DashboardActivityDto
            {
                Time = log.CreatedAt.ToString("dd MMM, HH:mm"),
                Title = log.Action.Replace('.', ' '),
                Description = string.IsNullOrWhiteSpace(log.Details)
                    ? log.EntityName
                    : $"{log.EntityName}: {log.Details}"
            })
            .ToList();

        return Task.FromResult<IReadOnlyList<DashboardActivityDto>>(rows);
    }
}