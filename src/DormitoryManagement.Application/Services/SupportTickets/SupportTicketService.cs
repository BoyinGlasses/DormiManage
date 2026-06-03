using DormitoryManagement.Application.Abstractions.Auth;
using DormitoryManagement.Application.Abstractions.Data;
using DormitoryManagement.Application.Abstractions.Services;
using DormitoryManagement.Application.DTOs.SupportTickets;
using DormitoryManagement.Application.Validation;
using DormitoryManagement.Domain.Constants;
using DormitoryManagement.Domain.Entities;
using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Application.Services.SupportTickets;

public sealed class SupportTicketService : ISupportTicketService
{
    private readonly IPermissionService _permissions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogService _auditLog;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService? _notifications;

    public SupportTicketService(
        IPermissionService permissions,
        IUnitOfWork unitOfWork,
        IAuditLogService auditLog,
        ICurrentUserService currentUser,
        INotificationService? notifications = null)
    {
        _permissions = permissions;
        _unitOfWork = unitOfWork;
        _auditLog = auditLog;
        _currentUser = currentUser;
        _notifications = notifications;
    }

    public async Task<IReadOnlyList<SupportTicketDto>> GetTicketsAsync(CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.TicketsRead, ct);
        var query = _unitOfWork.Repository<SupportTicket>().Query();
        if (_currentUser.IsInRole(RoleNames.Student))
        {
            var studentId = ResolveCurrentStudentId();
            query = query.Where(ticket => ticket.StudentId == studentId);
        }

        var tickets = query
            .OrderByDescending(ticket => ticket.CreatedAt)
            .Take(300)
            .ToList();

        return MapTickets(tickets);
    }

    public async Task<SupportTicketDto?> GetTicketAsync(Guid ticketId, CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.TicketsRead, ct);
        var ticket = await _unitOfWork.Repository<SupportTicket>().GetByIdAsync(ticketId, ct);
        if (ticket is null)
        {
            return null;
        }

        if (_currentUser.IsInRole(RoleNames.Student) && ticket.StudentId != ResolveCurrentStudentId())
        {
            throw new InvalidOperationException("Students can view only their own tickets.");
        }

        return MapTickets(new[] { ticket }).Single();
    }

    public async Task<SupportTicketDto> CreateTicketAsync(CreateSupportTicketRequest request, CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.TicketsCreate, ct);
        RequestValidator.ValidateAndThrow(request);

        var studentId = _currentUser.IsInRole(RoleNames.Student)
            ? ResolveCurrentStudentId()
            : request.StudentId;
        var ticket = new SupportTicket
        {
            Id = Guid.NewGuid(),
            StudentId = studentId,
            CreatedByUserId = _currentUser.UserId ?? throw new InvalidOperationException("No active user."),
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Category = request.Category,
            Priority = request.Priority,
            Status = SupportTicketStatus.New,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<SupportTicket>().AddAsync(ticket, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        await _auditLog.WriteAsync("SupportTicket.Created", "SupportTicket", ticket.Id, ticket.Title, ct);
        await NotifyStaffAsync("New support ticket", ticket.Title, ct);
        return MapTickets(new[] { ticket }).Single();
    }

    public async Task AssignTicketAsync(Guid ticketId, Guid managerId, CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.TicketsAssign, ct);
        var ticket = await GetTicketEntityAsync(ticketId, ct);
        ticket.AssignedToManagerId = managerId;
        ticket.Status = SupportTicketStatus.Assigned;
        _unitOfWork.Repository<SupportTicket>().Update(ticket);
        await _unitOfWork.SaveChangesAsync(ct);
        await _auditLog.WriteAsync("SupportTicket.Assigned", "SupportTicket", ticketId, managerId.ToString(), ct);
    }

    public async Task AddResponseAsync(Guid ticketId, string message, CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.TicketsUpdate, ct);
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new InvalidOperationException("Response message is required.");
        }

        var ticket = await GetTicketEntityAsync(ticketId, ct);
        var response = new SupportTicketResponse
        {
            Id = Guid.NewGuid(),
            SupportTicketId = ticket.Id,
            UserId = _currentUser.UserId ?? throw new InvalidOperationException("No active user."),
            Message = message.Trim(),
            RespondedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.Repository<SupportTicketResponse>().AddAsync(response, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        await _auditLog.WriteAsync("SupportTicket.ResponseAdded", "SupportTicket", ticketId, null, ct);
        await NotifyTicketOwnerAsync(ticket, "Ticket updated", $"A response was added to {ticket.Title}.", ct);
    }

    public async Task UpdateStatusAsync(UpdateSupportTicketStatusRequest request, CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.TicketsUpdate, ct);
        RequestValidator.ValidateAndThrow(request);
        var ticket = await GetTicketEntityAsync(request.TicketId, ct);
        ticket.Status = request.Status;
        ticket.ResolvedAt = request.Status is SupportTicketStatus.Resolved or SupportTicketStatus.Closed
            ? DateTime.UtcNow
            : null;
        _unitOfWork.Repository<SupportTicket>().Update(ticket);

        if (!string.IsNullOrWhiteSpace(request.Note))
        {
            await _unitOfWork.Repository<SupportTicketResponse>().AddAsync(new SupportTicketResponse
            {
                Id = Guid.NewGuid(),
                SupportTicketId = ticket.Id,
                UserId = _currentUser.UserId ?? throw new InvalidOperationException("No active user."),
                Message = request.Note.Trim(),
                RespondedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            }, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);
        await _auditLog.WriteAsync("SupportTicket.StatusUpdated", "SupportTicket", request.TicketId, request.Status.ToString(), ct);
        await NotifyTicketOwnerAsync(ticket, "Ticket status updated", $"{ticket.Title}: {request.Status}", ct);
    }

    public async Task CloseTicketAsync(Guid ticketId, CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.TicketsUpdate, ct);
        var ticket = await GetTicketEntityAsync(ticketId, ct);
        ticket.Status = SupportTicketStatus.Closed;
        ticket.ResolvedAt ??= DateTime.UtcNow;
        _unitOfWork.Repository<SupportTicket>().Update(ticket);
        await _unitOfWork.SaveChangesAsync(ct);
        await _auditLog.WriteAsync("SupportTicket.Closed", "SupportTicket", ticketId, null, ct);
    }

    private async Task<SupportTicket> GetTicketEntityAsync(Guid ticketId, CancellationToken ct) =>
        await _unitOfWork.Repository<SupportTicket>().GetByIdAsync(ticketId, ct)
        ?? throw new InvalidOperationException("Support ticket was not found.");

    private Guid ResolveCurrentStudentId()
    {
        if (_currentUser.CurrentUser?.StudentId is { } studentId)
        {
            return studentId;
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

    private IReadOnlyList<SupportTicketDto> MapTickets(IReadOnlyList<SupportTicket> tickets)
    {
        if (tickets.Count == 0)
        {
            return Array.Empty<SupportTicketDto>();
        }

        var ticketIds = tickets.Select(ticket => ticket.Id).ToHashSet();
        var studentIds = tickets.Where(ticket => ticket.StudentId.HasValue).Select(ticket => ticket.StudentId!.Value).ToHashSet();
        var userIds = tickets.Select(ticket => ticket.CreatedByUserId).ToHashSet();
        var managerIds = tickets.Where(ticket => ticket.AssignedToManagerId.HasValue).Select(ticket => ticket.AssignedToManagerId!.Value).ToHashSet();
        var responses = _unitOfWork.Repository<SupportTicketResponse>().Query()
            .Where(response => ticketIds.Contains(response.SupportTicketId))
            .OrderBy(response => response.RespondedAt)
            .ToList()
            .GroupBy(response => response.SupportTicketId)
            .ToDictionary(group => group.Key, group => group.ToList());
        foreach (var response in responses.Values.SelectMany(row => row))
        {
            userIds.Add(response.UserId);
        }

        var students = _unitOfWork.Repository<Student>().Query()
            .Where(student => studentIds.Contains(student.Id))
            .ToDictionary(student => student.Id);
        var users = _unitOfWork.Repository<User>().Query()
            .Where(user => userIds.Contains(user.Id))
            .ToDictionary(user => user.Id);
        var managers = _unitOfWork.Repository<Manager>().Query()
            .Where(manager => managerIds.Contains(manager.Id))
            .ToDictionary(manager => manager.Id);

        return tickets.Select(ticket =>
        {
            students.TryGetValue(ticket.StudentId ?? Guid.Empty, out var student);
            users.TryGetValue(ticket.CreatedByUserId, out var createdBy);
            managers.TryGetValue(ticket.AssignedToManagerId ?? Guid.Empty, out var manager);
            responses.TryGetValue(ticket.Id, out var ticketResponses);

            return new SupportTicketDto
            {
                Id = ticket.Id,
                StudentId = ticket.StudentId,
                StudentCode = student?.StudentCode ?? string.Empty,
                StudentName = student?.FullName ?? string.Empty,
                CreatedBy = createdBy?.FullName ?? createdBy?.Username ?? string.Empty,
                AssignedTo = manager?.FullName ?? string.Empty,
                Title = ticket.Title,
                Description = ticket.Description,
                Category = ticket.Category,
                Status = ticket.Status,
                Priority = ticket.Priority,
                CreatedAt = ticket.CreatedAt,
                ResolvedAt = ticket.ResolvedAt,
                Responses = (ticketResponses ?? new List<SupportTicketResponse>()).Select(response =>
                {
                    users.TryGetValue(response.UserId, out var author);
                    return new SupportTicketResponseDto
                    {
                        Id = response.Id,
                        Author = author?.FullName ?? author?.Username ?? "User",
                        Message = response.Message,
                        CreatedAt = response.RespondedAt
                    };
                }).ToArray()
            };
        }).ToArray();
    }

    private async Task NotifyStaffAsync(string title, string message, CancellationToken ct)
    {
        if (_notifications is null)
        {
            return;
        }

        await _notifications.NotifyRoleAsync(RoleNames.Admin, title, message, ct);
        await _notifications.NotifyRoleAsync(RoleNames.Manager, title, message, ct);
        await _notifications.NotifyRoleAsync(RoleNames.Staff, title, message, ct);
    }

    private async Task NotifyTicketOwnerAsync(SupportTicket ticket, string title, string message, CancellationToken ct)
    {
        if (_notifications is null || ticket.CreatedByUserId == Guid.Empty)
        {
            return;
        }

        await _notifications.NotifyUserAsync(ticket.CreatedByUserId, title, message, ct);
    }
}
