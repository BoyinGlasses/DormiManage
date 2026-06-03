using DormitoryManagement.Application.Abstractions.Auth;
using DormitoryManagement.Application.Abstractions.Data;
using DormitoryManagement.Application.Abstractions.Services;
using DormitoryManagement.Application.DTOs.Registrations;
using DormitoryManagement.Application.Validation;
using DormitoryManagement.Domain.Constants;
using DormitoryManagement.Domain.Entities;
using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Application.Services.Registrations;

public sealed class RoomRegistrationService : IRoomRegistrationService
{
    private readonly IPermissionService _permissions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogService _auditLog;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService? _notifications;

    public RoomRegistrationService(
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

    public async Task<Guid> CreateRegistrationAsync(CreateRoomRegistrationRequest request, CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.RoomRegistrationCreate, ct);
        RequestValidator.ValidateAndThrow(request);
        EnsureSupportedContractOptions(request.ContractTermMonths);
        var studentId = ResolveStudentId(request.StudentId);
        var student = GetStudent(studentId);
        var room = GetRoom(request.RoomId);

        EnsureStudentCanRegister(student, room, null, allowOtherPendingRegistrations: false);

        var registration = new RoomRegistration
        {
            Id = Guid.NewGuid(),
            StudentId = student.Id,
            RoomId = room.Id,
            Status = RegistrationStatus.Pending,
            ContractTermMonths = request.ContractTermMonths,
            IncludesInternet = request.IncludesInternet,
            RequestedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<RoomRegistration>().AddAsync(registration, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        await _auditLog.WriteAsync("RoomRegistration.Submitted", "RoomRegistration", registration.Id, room.RoomNumber, ct);
        await NotifyStudentAsync(student, "Registration submitted", $"Your room registration for {room.RoomNumber} is pending review.", ct);
        await NotifyManagersAsync("New pending registration", $"{student.FullName} submitted a room registration for {room.RoomNumber}.", ct);
        return registration.Id;
    }

    public async Task<IReadOnlyList<RoomRegistrationDto>> GetPendingRegistrationsAsync(CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.RoomRegistrationApprove, ct);
        var activeStudentIds = _unitOfWork.Repository<RoomAssignment>().Query()
            .Where(assignment => assignment.IsActive)
            .Select(assignment => assignment.StudentId)
            .ToHashSet();
        var approvedLikeStatuses = new[] { RegistrationStatus.PaymentPending, RegistrationStatus.Approved };
        var approvedStudentIds = _unitOfWork.Repository<RoomRegistration>().Query()
            .Where(registration => approvedLikeStatuses.Contains(registration.Status))
            .Select(registration => registration.StudentId)
            .ToHashSet();
        var registrations = _unitOfWork.Repository<RoomRegistration>().Query()
            .Where(registration => registration.Status == RegistrationStatus.Pending
                && !activeStudentIds.Contains(registration.StudentId)
                && !approvedStudentIds.Contains(registration.StudentId))
            .OrderBy(registration => registration.RequestedAt)
            .ToList();

        if (_currentUser.CurrentUser?.BuildingId is { } buildingId)
        {
            var allowedRoomIds = _unitOfWork.Repository<Room>().Query()
                .Where(room => room.BuildingId == buildingId)
                .Select(room => room.Id)
                .ToHashSet();
            registrations = registrations.Where(registration => allowedRoomIds.Contains(registration.RoomId)).ToList();
        }

        return MapRegistrations(registrations);
    }

    public async Task<IReadOnlyList<RoomRegistrationDto>> GetCurrentStudentRegistrationsAsync(CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.RoomRegistrationCreate, ct);
        var studentId = ResolveStudentId(Guid.Empty);
        var registrations = _unitOfWork.Repository<RoomRegistration>().Query()
            .Where(registration => registration.StudentId == studentId)
            .OrderByDescending(registration => registration.RequestedAt)
            .ToList();

        return MapRegistrations(registrations);
    }

    public async Task ApproveRegistrationAsync(ApproveRoomRegistrationRequest request, CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.RoomRegistrationApprove, ct);
        RequestValidator.ValidateAndThrow(request);
        await using var tx = await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var registration = GetRegistration(request.RegistrationId);
            if (registration.Status != RegistrationStatus.Pending)
            {
                throw new InvalidOperationException("Only pending registrations can be approved.");
            }

            var student = GetStudent(registration.StudentId);
            var room = GetRoom(registration.RoomId);
            EnsureCanManageRoom(room);
            EnsureStudentCanRegister(student, room, registration.Id, allowOtherPendingRegistrations: true);

            var startDate = request.StartDate.Date;
            EnsureSupportedContractOptions(registration.ContractTermMonths);
            var totalAmount = room.MonthlyPrice * registration.ContractTermMonths;
            var contract = new Contract
            {
                Id = Guid.NewGuid(),
                ContractNumber = GenerateContractNumber(registration.Id, startDate),
                StudentId = student.Id,
                RoomId = room.Id,
                StartDate = startDate,
                EndDate = startDate.AddMonths(registration.ContractTermMonths).AddDays(-1),
                MonthlyFee = room.MonthlyPrice,
                DepositAmount = 0m,
                TermMonths = registration.ContractTermMonths,
                TotalAmount = totalAmount,
                IncludesInternet = registration.IncludesInternet,
                RoomRegistrationId = registration.Id,
                Status = ContractStatus.PendingPayment
            };
            var invoiceId = Guid.NewGuid();
            var invoice = new Invoice
            {
                Id = invoiceId,
                InvoiceNumber = GenerateContractInvoiceNumber(registration.Id, startDate),
                StudentId = student.Id,
                RoomId = room.Id,
                BillingPeriod = startDate.ToString("yyyy-MM"),
                InvoiceKind = InvoiceKind.ContractPrepayment,
                ContractId = contract.Id,
                IssueDate = DateTime.UtcNow,
                DueDate = startDate,
                TotalAmount = totalAmount,
                PaidAmount = 0m,
                Status = InvoiceStatus.Unpaid,
                Items =
                {
                    new InvoiceItem
                    {
                        Id = Guid.NewGuid(),
                        InvoiceId = invoiceId,
                        FeeTypeId = GetFeeTypeId("ROOM_FEE"),
                        Description = $"Contract prepayment {registration.ContractTermMonths} months",
                        Quantity = registration.ContractTermMonths,
                        UnitPrice = room.MonthlyPrice,
                        Amount = totalAmount,
                        CreatedAt = DateTime.UtcNow
                    }
                }
            };

            registration.Status = RegistrationStatus.PaymentPending;
            registration.ReviewedAt = DateTime.UtcNow;
            registration.ReviewedByUserId = _currentUser.UserId;
            var otherPendingRegistrations = _unitOfWork.Repository<RoomRegistration>().Query()
                .Where(candidate => candidate.StudentId == student.Id
                    && candidate.Id != registration.Id
                    && candidate.Status == RegistrationStatus.Pending)
                .ToList();
            foreach (var pendingRegistration in otherPendingRegistrations)
            {
                pendingRegistration.Status = RegistrationStatus.Cancelled;
                pendingRegistration.ReviewedAt = DateTime.UtcNow;
                pendingRegistration.ReviewedByUserId = _currentUser.UserId;
                pendingRegistration.RejectionReason = "Automatically cancelled after another room registration was approved.";
                _unitOfWork.Repository<RoomRegistration>().Update(pendingRegistration);
            }

            await _unitOfWork.Repository<Contract>().AddAsync(contract, ct);
            await _unitOfWork.Repository<Invoice>().AddAsync(invoice, ct);
            _unitOfWork.Repository<RoomRegistration>().Update(registration);
            await _unitOfWork.SaveChangesAsync(ct);
            contract.UpfrontInvoiceId = invoice.Id;
            _unitOfWork.Repository<Contract>().Update(contract);
            await _unitOfWork.SaveChangesAsync(ct);
            await _auditLog.WriteAsync("RoomRegistration.ApprovedPendingPayment", "RoomRegistration", request.RegistrationId, null, ct);
            await _auditLog.WriteAsync("Contract.CreatedPendingPayment", "Contract", contract.Id, contract.ContractNumber, ct);
            await _auditLog.WriteAsync("Contract.PrepaymentInvoiceCreated", "Invoice", invoice.Id, invoice.InvoiceNumber, ct);
            await NotifyStudentAsync(student, "Registration awaiting payment", $"Pay invoice {invoice.InvoiceNumber} to activate room {room.RoomNumber}.", ct);
            await tx.CommitAsync(ct);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    public async Task RejectRegistrationAsync(RejectRoomRegistrationRequest request, CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.RoomRegistrationApprove, ct);
        RequestValidator.ValidateAndThrow(request);
        await using var tx = await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var registration = GetRegistration(request.RegistrationId);
            if (registration.Status != RegistrationStatus.Pending)
            {
                throw new InvalidOperationException("Only pending registrations can be rejected.");
            }
            EnsureCanManageRoom(GetRoom(registration.RoomId));

            registration.Status = RegistrationStatus.Rejected;
            registration.RejectionReason = request.Reason.Trim();
            registration.ReviewedAt = DateTime.UtcNow;
            registration.ReviewedByUserId = _currentUser.UserId;
            _unitOfWork.Repository<RoomRegistration>().Update(registration);
            await _unitOfWork.SaveChangesAsync(ct);
            await _auditLog.WriteAsync("RoomRegistration.Rejected", "RoomRegistration", request.RegistrationId, request.Reason.Trim(), ct);
            var student = GetStudent(registration.StudentId);
            await NotifyStudentAsync(student, "Registration rejected", request.Reason.Trim(), ct);
            await tx.CommitAsync(ct);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    public async Task CancelRegistrationAsync(Guid registrationId, CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.RoomRegistrationCreate, ct);
        var registration = GetRegistration(registrationId);
        if (registration.Status != RegistrationStatus.Pending)
        {
            throw new InvalidOperationException("Only pending registrations can be cancelled.");
        }

        if (_currentUser.IsInRole(RoleNames.Student) && registration.StudentId != ResolveStudentId(Guid.Empty))
        {
            throw new InvalidOperationException("Students can cancel only their own registrations.");
        }

        registration.Status = RegistrationStatus.Cancelled;
        _unitOfWork.Repository<RoomRegistration>().Update(registration);
        await _unitOfWork.SaveChangesAsync(ct);
        await _auditLog.WriteAsync("RoomRegistration.Cancelled", "RoomRegistration", registrationId, null, ct);
    }

    private Guid ResolveStudentId(Guid requestedStudentId)
    {
        if (_currentUser.IsInRole(RoleNames.Student))
        {
            if (_currentUser.CurrentUser?.StudentId is { } currentStudentId)
            {
                return currentStudentId;
            }

            if (_currentUser.UserId is { } userId)
            {
                var studentByUser = _unitOfWork.Repository<Student>().Query().FirstOrDefault(student => student.UserId == userId);
                if (studentByUser is not null)
                {
                    return studentByUser.Id;
                }
            }

            throw new InvalidOperationException("Current user is not linked to a student profile.");
        }

        if (requestedStudentId == Guid.Empty)
        {
            throw new InvalidOperationException("Student is required.");
        }

        return requestedStudentId;
    }

    private Student GetStudent(Guid studentId) =>
        _unitOfWork.Repository<Student>().GetByIdAsync(studentId).GetAwaiter().GetResult()
        ?? throw new InvalidOperationException("Student was not found.");

    private Room GetRoom(Guid roomId) =>
        _unitOfWork.Repository<Room>().GetByIdAsync(roomId).GetAwaiter().GetResult()
        ?? throw new InvalidOperationException("Room was not found.");

    private RoomRegistration GetRegistration(Guid registrationId) =>
        _unitOfWork.Repository<RoomRegistration>().GetByIdAsync(registrationId).GetAwaiter().GetResult()
        ?? throw new InvalidOperationException("Registration was not found.");

    private void EnsureStudentCanRegister(Student student, Room room, Guid? currentRegistrationId, bool allowOtherPendingRegistrations)
    {
        if (room.IsDeleted || room.Status is RoomStatus.Maintenance or RoomStatus.Inactive)
        {
            throw new InvalidOperationException("Room is not available for registration.");
        }

        if (!IsGenderCompatible(student, room))
        {
            throw new InvalidOperationException("Student gender is not compatible with the room policy.");
        }

        var approvedLikeStatuses = new[] { RegistrationStatus.PaymentPending, RegistrationStatus.Approved };
        var hasApprovedRegistration = _unitOfWork.Repository<RoomRegistration>().Query()
            .Any(registration => registration.StudentId == student.Id
                && approvedLikeStatuses.Contains(registration.Status)
                && registration.Id != currentRegistrationId);
        if (hasApprovedRegistration)
        {
            throw new InvalidOperationException("Student already has an approved room registration.");
        }

        var duplicatePendingStatuses = new[] { RegistrationStatus.Pending, RegistrationStatus.PaymentPending };
        var hasDuplicatePending = _unitOfWork.Repository<RoomRegistration>().Query()
            .Any(registration => registration.StudentId == student.Id
                && duplicatePendingStatuses.Contains(registration.Status)
                && registration.Id != currentRegistrationId);
        if (!allowOtherPendingRegistrations && hasDuplicatePending)
        {
            throw new InvalidOperationException("Student already has a pending room registration.");
        }

        var hasActiveAssignment = _unitOfWork.Repository<RoomAssignment>().Query()
            .Any(assignment => assignment.StudentId == student.Id && assignment.IsActive);
        if (hasActiveAssignment)
        {
            throw new InvalidOperationException("Student already has an active room assignment.");
        }

        var activeAssignments = CountActiveAssignments(room.Id);
        var holdStatuses = new[] { RegistrationStatus.Pending, RegistrationStatus.PaymentPending };
        var heldByOthers = _unitOfWork.Repository<RoomRegistration>().Query()
            .Count(registration => registration.RoomId == room.Id
                && holdStatuses.Contains(registration.Status)
                && registration.Id != currentRegistrationId
                && (!allowOtherPendingRegistrations || registration.StudentId != student.Id));
        var occupiedSlots = Math.Max(room.CurrentOccupancy, activeAssignments);
        if (occupiedSlots + heldByOthers >= room.Capacity)
        {
            throw new InvalidOperationException("Room has no available slots.");
        }
    }

    private int CountActiveAssignments(Guid roomId) =>
        _unitOfWork.Repository<RoomAssignment>().Query()
            .Count(assignment => assignment.RoomId == roomId && assignment.IsActive);

    private static bool IsGenderCompatible(Student student, Room room)
    {
        if (room.GenderType == RoomGenderType.Mixed)
        {
            return false;
        }

        var gender = student.Gender?.Trim();
        return room.GenderType switch
        {
            RoomGenderType.Male => string.Equals(gender, "Male", StringComparison.OrdinalIgnoreCase),
            RoomGenderType.Female => string.Equals(gender, "Female", StringComparison.OrdinalIgnoreCase),
            _ => true
        };
    }

    private IReadOnlyList<RoomRegistrationDto> MapRegistrations(IReadOnlyList<RoomRegistration> registrations)
    {
        if (registrations.Count == 0)
        {
            return Array.Empty<RoomRegistrationDto>();
        }

        var studentIds = registrations.Select(registration => registration.StudentId).ToHashSet();
        var roomIds = registrations.Select(registration => registration.RoomId).ToHashSet();
        var students = _unitOfWork.Repository<Student>().Query()
            .Where(student => studentIds.Contains(student.Id))
            .ToDictionary(student => student.Id);
        var rooms = _unitOfWork.Repository<Room>().Query()
            .Where(room => roomIds.Contains(room.Id))
            .ToDictionary(room => room.Id);
        var buildingIds = rooms.Values.Select(room => room.BuildingId).ToHashSet();
        var buildings = _unitOfWork.Repository<Building>().Query()
            .Where(building => buildingIds.Contains(building.Id))
            .ToDictionary(building => building.Id);

        return registrations.Select(registration =>
        {
            students.TryGetValue(registration.StudentId, out var student);
            rooms.TryGetValue(registration.RoomId, out var room);
            var buildingName = room is not null && buildings.TryGetValue(room.BuildingId, out var building)
                ? building.Name
                : string.Empty;

            return new RoomRegistrationDto
            {
                Id = registration.Id,
                StudentId = registration.StudentId,
                StudentCode = student?.StudentCode ?? string.Empty,
                StudentName = student?.FullName ?? string.Empty,
                RoomId = registration.RoomId,
                RoomNumber = room is null ? string.Empty : $"{buildingName}-{room.RoomNumber}".Trim('-'),
                BuildingName = buildingName,
                Status = registration.Status,
                ContractTermMonths = registration.ContractTermMonths,
                IncludesInternet = registration.IncludesInternet,
                RequestedAt = registration.RequestedAt,
                RejectionReason = registration.RejectionReason
            };
        }).ToArray();
    }

    private static void EnsureSupportedContractOptions(int termMonths)
    {
        if (termMonths is not (6 or 12))
        {
            throw new InvalidOperationException("Contract term must be 6 or 12 months.");
        }
    }

    private void EnsureCanManageRoom(Room room)
    {
        if (_currentUser.IsInRole(RoleNames.BuildingManager)
            && _currentUser.CurrentUser?.BuildingId is { } buildingId
            && room.BuildingId != buildingId)
        {
            throw new InvalidOperationException("Building managers can manage only assigned building rooms.");
        }
    }

    private Guid? GetFeeTypeId(string code) =>
        _unitOfWork.Repository<FeeType>().Query()
            .Where(feeType => !feeType.IsDeleted)
            .ToList()
            .FirstOrDefault(feeType => string.Equals(feeType.Code, code, StringComparison.OrdinalIgnoreCase))
            ?.Id;

    private static string GenerateContractNumber(Guid registrationId, DateTime startDate) =>
        $"CTR-{startDate:yyyyMM}-{registrationId.ToString("N")[..8]}";

    private static string GenerateContractInvoiceNumber(Guid registrationId, DateTime startDate) =>
        $"INV-PREPAY-{startDate:yyyyMM}-{registrationId.ToString("N")[..8]}";

    private async Task NotifyStudentAsync(Student student, string title, string message, CancellationToken ct)
    {
        if (_notifications is not null && student.UserId.HasValue)
        {
            await _notifications.NotifyUserAsync(student.UserId.Value, title, message, ct);
        }
    }

    private async Task NotifyManagersAsync(string title, string message, CancellationToken ct)
    {
        if (_notifications is null)
        {
            return;
        }

        await _notifications.NotifyRoleAsync(RoleNames.Admin, title, message, ct);
        await _notifications.NotifyRoleAsync(RoleNames.Manager, title, message, ct);
        await _notifications.NotifyRoleAsync(RoleNames.BuildingManager, title, message, ct);
    }
}
