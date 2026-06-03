using DormitoryManagement.Application.Abstractions.Auth;
using DormitoryManagement.Application.Abstractions.Data;
using DormitoryManagement.Application.Abstractions.Services;
using DormitoryManagement.Application.DTOs.Payments;
using DormitoryManagement.Application.Validation;
using DormitoryManagement.Domain.Constants;
using DormitoryManagement.Domain.Entities;
using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Application.Services.Payments;

public sealed class PaymentExtensionService : IPaymentExtensionService
{
    private readonly IPermissionService _permissions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogService _auditLog;
    private readonly ICurrentUserService _currentUser;

    public PaymentExtensionService(
        IPermissionService permissions,
        IUnitOfWork unitOfWork,
        IAuditLogService auditLog,
        ICurrentUserService currentUser)
    {
        _permissions = permissions;
        _unitOfWork = unitOfWork;
        _auditLog = auditLog;
        _currentUser = currentUser;
    }

    public async Task<PaymentExtensionDto> RequestExtensionAsync(CreatePaymentExtensionRequest request, CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.PaymentsCreate, ct);
        RequestValidator.ValidateAndThrow(request);
        var invoice = GetInvoice(request.InvoiceId);
        var studentId = ResolveStudentId();
        if (invoice.StudentId != studentId)
        {
            throw new InvalidOperationException("Students can request extensions only for their own invoices.");
        }

        EnsureMonthlyUtilityInvoice(invoice);
        EnsureRequestIsWithinPolicy(invoice, request.RequestedDueDate.Date);
        if (_unitOfWork.Repository<PaymentExtension>().Query().Any(extension =>
            extension.InvoiceId == invoice.Id && extension.Status == PaymentExtensionStatus.Pending))
        {
            throw new InvalidOperationException("Invoice already has a pending extension request.");
        }

        var extension = new PaymentExtension
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoice.Id,
            StudentId = studentId,
            RequestedDueDate = request.RequestedDueDate.Date,
            Reason = request.Reason.Trim(),
            Status = PaymentExtensionStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        await _unitOfWork.Repository<PaymentExtension>().AddAsync(extension, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        await _auditLog.WriteAsync("PaymentExtension.Requested", "PaymentExtension", extension.Id, invoice.InvoiceNumber, ct);
        return MapExtensions(new[] { extension }).Single();
    }

    public async Task<IReadOnlyList<PaymentExtensionDto>> GetPendingExtensionsAsync(CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.BillingWrite, ct);
        var extensions = _unitOfWork.Repository<PaymentExtension>().Query()
            .Where(extension => extension.Status == PaymentExtensionStatus.Pending)
            .OrderBy(extension => extension.CreatedAt)
            .ToList();
        if (_currentUser.IsInRole(RoleNames.Manager) && _currentUser.CurrentUser?.BuildingId is { })
        {
            var buildingId = _currentUser.CurrentUser?.BuildingId
                ?? throw new InvalidOperationException("Manager is not assigned to a building.");
            var roomIds = _unitOfWork.Repository<Room>().Query()
                .Where(room => room.BuildingId == buildingId)
                .Select(room => room.Id)
                .ToHashSet();
            var invoiceIds = _unitOfWork.Repository<Invoice>().Query()
                .Where(invoice => roomIds.Contains(invoice.RoomId))
                .Select(invoice => invoice.Id)
                .ToHashSet();
            extensions = extensions.Where(extension => invoiceIds.Contains(extension.InvoiceId)).ToList();
        }

        return MapExtensions(extensions);
    }

    public async Task ApproveExtensionAsync(Guid extensionId, CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.BillingWrite, ct);
        var extension = GetExtension(extensionId);
        if (extension.Status != PaymentExtensionStatus.Pending)
        {
            throw new InvalidOperationException("Only pending extension requests can be approved.");
        }

        var invoice = GetInvoice(extension.InvoiceId);
        EnsureMonthlyUtilityInvoice(invoice);
        EnsureCanManageInvoice(invoice);
        var maxDueDate = CalculateMaxExtensionDueDate(invoice);
        var requestedDueDate = extension.RequestedDueDate.Date > maxDueDate ? maxDueDate : extension.RequestedDueDate.Date;
        invoice.DueDate = requestedDueDate > invoice.DueDate.Date ? requestedDueDate : invoice.DueDate.Date;
        extension.Status = PaymentExtensionStatus.Approved;
        extension.ReviewedAt = DateTime.UtcNow;
        extension.ReviewedByUserId = _currentUser.UserId;
        _unitOfWork.Repository<Invoice>().Update(invoice);
        _unitOfWork.Repository<PaymentExtension>().Update(extension);
        await _unitOfWork.SaveChangesAsync(ct);
        await _auditLog.WriteAsync("PaymentExtension.Approved", "PaymentExtension", extension.Id, invoice.InvoiceNumber, ct);
    }

    public async Task RejectExtensionAsync(Guid extensionId, string reason, CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.BillingWrite, ct);
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new InvalidOperationException("Rejection reason is required.");
        }

        var extension = GetExtension(extensionId);
        if (extension.Status != PaymentExtensionStatus.Pending)
        {
            throw new InvalidOperationException("Only pending extension requests can be rejected.");
        }

        EnsureCanManageInvoice(GetInvoice(extension.InvoiceId));
        extension.Status = PaymentExtensionStatus.Rejected;
        extension.ReviewedAt = DateTime.UtcNow;
        extension.ReviewedByUserId = _currentUser.UserId;
        extension.RejectionReason = reason.Trim();
        _unitOfWork.Repository<PaymentExtension>().Update(extension);
        await _unitOfWork.SaveChangesAsync(ct);
        await _auditLog.WriteAsync("PaymentExtension.Rejected", "PaymentExtension", extension.Id, extension.RejectionReason, ct);
    }

    private Invoice GetInvoice(Guid invoiceId) =>
        _unitOfWork.Repository<Invoice>().GetByIdAsync(invoiceId).GetAwaiter().GetResult()
        ?? throw new InvalidOperationException("Invoice was not found.");

    private PaymentExtension GetExtension(Guid extensionId) =>
        _unitOfWork.Repository<PaymentExtension>().GetByIdAsync(extensionId).GetAwaiter().GetResult()
        ?? throw new InvalidOperationException("Payment extension was not found.");

    private Guid ResolveStudentId()
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

    private static void EnsureMonthlyUtilityInvoice(Invoice invoice)
    {
        if (invoice.InvoiceKind != InvoiceKind.MonthlyUtility)
        {
            throw new InvalidOperationException("Extensions are available only for monthly utility invoices.");
        }
    }

    private static void EnsureRequestIsWithinPolicy(Invoice invoice, DateTime requestedDueDate)
    {
        if (invoice.Status == InvoiceStatus.Paid || invoice.PaidAmount >= invoice.TotalAmount)
        {
            throw new InvalidOperationException("Paid invoices cannot be extended.");
        }

        var maxDueDate = CalculateMaxExtensionDueDate(invoice);
        if (requestedDueDate > maxDueDate)
        {
            throw new InvalidOperationException($"Extension due date cannot be later than {maxDueDate:d}.");
        }
    }

    private void EnsureCanManageInvoice(Invoice invoice)
    {
        if (!(_currentUser.IsInRole(RoleNames.Manager) && _currentUser.CurrentUser?.BuildingId is { }))
        {
            return;
        }

        var buildingId = _currentUser.CurrentUser?.BuildingId
            ?? throw new InvalidOperationException("Manager is not assigned to a building.");
        var room = _unitOfWork.Repository<Room>().GetByIdAsync(invoice.RoomId).GetAwaiter().GetResult()
            ?? throw new InvalidOperationException("Room was not found.");
        if (room.BuildingId != buildingId)
        {
            throw new InvalidOperationException("Managers can review only assigned building invoices.");
        }
    }

    private IReadOnlyList<PaymentExtensionDto> MapExtensions(IReadOnlyList<PaymentExtension> extensions)
    {
        if (extensions.Count == 0)
        {
            return Array.Empty<PaymentExtensionDto>();
        }

        var invoiceIds = extensions.Select(extension => extension.InvoiceId).ToHashSet();
        var studentIds = extensions.Select(extension => extension.StudentId).ToHashSet();
        var invoices = _unitOfWork.Repository<Invoice>().Query()
            .Where(invoice => invoiceIds.Contains(invoice.Id))
            .ToDictionary(invoice => invoice.Id);
        var students = _unitOfWork.Repository<Student>().Query()
            .Where(student => studentIds.Contains(student.Id))
            .ToDictionary(student => student.Id);
        return extensions.Select(extension =>
        {
            invoices.TryGetValue(extension.InvoiceId, out var invoice);
            students.TryGetValue(extension.StudentId, out var student);
            return new PaymentExtensionDto
            {
                Id = extension.Id,
                InvoiceId = extension.InvoiceId,
                InvoiceNumber = invoice?.InvoiceNumber ?? string.Empty,
                StudentId = extension.StudentId,
                StudentCode = student?.StudentCode ?? string.Empty,
                StudentName = student?.FullName ?? string.Empty,
                RequestedDueDate = extension.RequestedDueDate,
                Reason = extension.Reason,
                Status = extension.Status,
                ReviewedAt = extension.ReviewedAt,
                RejectionReason = extension.RejectionReason
            };
        }).ToArray();
    }

    private static DateTime CalculateMaxExtensionDueDate(Invoice invoice)
    {
        var plusFive = invoice.DueDate.Date.AddDays(5);
        var day15 = new DateTime(invoice.DueDate.Year, invoice.DueDate.Month, 15);
        return plusFive <= day15 ? plusFive : day15;
    }
}

