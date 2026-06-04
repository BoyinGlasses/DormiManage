using DormitoryManagement.Application.Abstractions.Auth;
using DormitoryManagement.Application.Abstractions.Data;
using DormitoryManagement.Application.Abstractions.Payments;
using DormitoryManagement.Application.Abstractions.Services;
using DormitoryManagement.Application.DTOs.Billing;
using DormitoryManagement.Application.DTOs.Payments;
using DormitoryManagement.Application.Validation;
using DormitoryManagement.Domain.Constants;
using DormitoryManagement.Domain.Entities;
using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Application.Services.Payments;

public sealed class PaymentService : IPaymentService
{
    private readonly IPermissionService _permissions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogService _auditLog;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService? _notifications;
    private readonly IPayOsService? _payOsService;
    private static readonly InvoiceStatus[] PayableStatuses = [InvoiceStatus.Unpaid, InvoiceStatus.Partial, InvoiceStatus.Overdue];

    public PaymentService(
        IPermissionService permissions,
        IUnitOfWork unitOfWork,
        IAuditLogService auditLog,
        ICurrentUserService currentUser,
        INotificationService? notifications = null,
        IPayOsService? payOsService = null)
    {
        _permissions = permissions;
        _unitOfWork = unitOfWork;
        _auditLog = auditLog;
        _currentUser = currentUser;
        _notifications = notifications;
        _payOsService = payOsService;
    }

    public async Task<IReadOnlyList<OutstandingInvoiceDto>> GetOutstandingInvoicesAsync(CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.BillingRead, ct);
        var studentId = _currentUser.IsInRole(RoleNames.Student) ? ResolveCurrentStudentId() : (Guid?)null;
        var query = _unitOfWork.Repository<Invoice>().Query()
            .Where(invoice => PayableStatuses.Contains(invoice.Status) && invoice.TotalAmount > invoice.PaidAmount);

        if (studentId.HasValue)
        {
            query = query.Where(invoice => invoice.StudentId == studentId.Value);
        }

        return query
            .OrderBy(invoice => invoice.DueDate)
            .ThenBy(invoice => invoice.IssueDate)
            .ToList()
            .Select(invoice => new OutstandingInvoiceDto
            {
                Id = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                BillingPeriod = invoice.BillingPeriod,
                InvoiceKind = invoice.InvoiceKind,
                DueDate = invoice.DueDate,
                TotalAmount = invoice.TotalAmount,
                PaidAmount = invoice.PaidAmount,
                RemainingAmount = invoice.TotalAmount - invoice.PaidAmount,
                Status = invoice.Status
            })
            .ToArray();
    }

    public async Task<IReadOnlyList<PaymentDto>> GetPendingPaymentsAsync(CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.PaymentsConfirm, ct);
        var payments = _unitOfWork.Repository<Payment>().Query()
            .Where(payment => payment.Status == PaymentStatus.Pending)
            .OrderBy(payment => payment.CreatedAt)
            .ToList();

        return MapPayments(payments);
    }

    public async Task<PaymentDto> CreateMockPaymentAsync(CreatePaymentRequest request, CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.PaymentsCreate, ct);
        RequestValidator.ValidateAndThrow(request);
        var studentId = _currentUser.IsInRole(RoleNames.Student)
            ? ResolveCurrentStudentId()
            : request.StudentId;
        if (studentId == Guid.Empty)
        {
            throw new InvalidOperationException("Student is required.");
        }

        var targetInvoice = request.InvoiceId.HasValue
            ? GetPayableInvoice(request.InvoiceId.Value, studentId)
            : null;
        if (targetInvoice is not null)
        {
            var remaining = targetInvoice.TotalAmount - targetInvoice.PaidAmount;
            if (request.Amount > remaining)
            {
                throw new InvalidOperationException("Payment amount exceeds selected invoice balance.");
            }

            if (targetInvoice.InvoiceKind == InvoiceKind.ContractPrepayment && request.Amount != remaining)
            {
                throw new InvalidOperationException("Contract prepayment must be paid in full.");
            }
        }

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            StudentId = studentId,
            TargetInvoiceId = targetInvoice?.Id,
            Amount = request.Amount,
            Method = request.Method,
            Status = PaymentStatus.Pending,
            PaymentCode = GeneratePaymentCode()
        };
        await _unitOfWork.Repository<Payment>().AddAsync(payment, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        await _auditLog.WriteAsync("Payment.Created", "Payment", payment.Id, payment.PaymentCode, ct);
        await NotifyRoleAsync(RoleNames.Admin, "Payment pending", $"Payment {payment.PaymentCode} is waiting for confirmation.", ct);
        return MapPayments(new[] { payment }).Single();
    }

    public async Task<InvoicePaymentQrDto> GenerateInvoiceQrAsync(Guid invoiceId, CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.BillingWrite, ct);
        var invoice = GetPayableInvoiceForQr(invoiceId);
        if (HasActivePayOsQr(invoice))
        {
            return MapInvoicePaymentQr(invoice);
        }

        if (_payOsService is null)
        {
            throw new InvalidOperationException("PayOS service is not configured.");
        }

        var transferContent = GenerateTransferContent(invoice);
        var amount = invoice.TotalAmount - invoice.PaidAmount;
        var student = GetStudent(invoice.StudentId);
        var paymentLink = await _payOsService.CreatePaymentLinkAsync(new PayOsCreatePaymentRequest
        {
            OrderCode = GeneratePayOsOrderCode(invoice.Id),
            Amount = amount,
            Description = transferContent,
            ItemName = string.IsNullOrWhiteSpace(invoice.InvoiceNumber) ? "Dormitory invoice" : invoice.InvoiceNumber.Trim(),
            BuyerName = student?.FullName ?? string.Empty
        }, ct);
        if (string.IsNullOrWhiteSpace(paymentLink.QrDataUrl))
        {
            throw new InvalidOperationException("PayOS did not return QR data.");
        }

        invoice.TransferContent = transferContent;
        invoice.QrDataUrl = paymentLink.QrDataUrl.Trim();
        _unitOfWork.Repository<Invoice>().Update(invoice);
        await _unitOfWork.SaveChangesAsync(ct);
        await _auditLog.WriteAsync("Payment.QrGenerated", "Invoice", invoice.Id, transferContent, ct);
        return MapInvoicePaymentQr(invoice, paymentLink);
    }

    public async Task<InvoicePaymentQrDto> GetInvoicePaymentQrAsync(Guid invoiceId, CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.BillingRead, ct);
        var invoice = _unitOfWork.Repository<Invoice>().GetByIdAsync(invoiceId, ct).GetAwaiter().GetResult()
            ?? throw new InvalidOperationException("Invoice was not found.");
        EnsureInvoiceAccess(invoice);
        return MapInvoicePaymentQr(invoice);
    }

    public async Task<BankTransferProcessResultDto> ProcessBankTransferAsync(BankTransferNotificationDto notification, CancellationToken ct = default)
    {
        RequestValidator.ValidateAndThrow(notification);
        var transactionId = notification.TransactionId.Trim();
        var description = notification.Description.Trim();
        var duplicatePayment = _unitOfWork.Repository<Payment>().Query()
            .Where(payment => payment.Status == PaymentStatus.Success && payment.TransactionRef != null)
            .ToList()
            .FirstOrDefault(payment => payment.TransactionRef!.Equals(transactionId, StringComparison.OrdinalIgnoreCase));
        if (duplicatePayment is not null)
        {
            await _auditLog.WriteAsync("Payment.BankTransferDuplicate", "Payment", duplicatePayment.Id, transactionId, ct);
            return new BankTransferProcessResultDto
            {
                Matched = false,
                Duplicate = true,
                InvoiceId = duplicatePayment.TargetInvoiceId,
                PaymentId = duplicatePayment.Id,
                Status = "Duplicate",
                Message = "Bank transaction was already processed."
            };
        }

        var matches = _unitOfWork.Repository<Invoice>().Query()
            .Where(invoice => invoice.TransferContent != null
                && invoice.TransferContent != string.Empty
                && invoice.TotalAmount - invoice.PaidAmount == notification.Amount
                && (invoice.Status == InvoiceStatus.Unpaid || invoice.Status == InvoiceStatus.Partial || invoice.Status == InvoiceStatus.Overdue))
            .ToList()
            .Where(invoice => description.Contains(invoice.TransferContent!, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (matches.Count != 1)
        {
            var status = matches.Count == 0 ? "Unmatched" : "MultipleMatches";
            await _auditLog.WriteAsync("Payment.BankTransferUnmatched", "Payment", null, $"{transactionId}:{status}", ct);
            return new BankTransferProcessResultDto
            {
                Matched = false,
                Duplicate = false,
                Status = status,
                Message = matches.Count == 0
                    ? "No payable invoice matched the bank transfer."
                    : "More than one payable invoice matched the bank transfer."
            };
        }

        var invoice = matches.Single();
        await using var tx = await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var paidAt = notification.TransactionDate == default ? DateTime.UtcNow : notification.TransactionDate;
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                StudentId = invoice.StudentId,
                TargetInvoiceId = invoice.Id,
                Amount = notification.Amount,
                Method = PaymentMethod.QrBanking,
                Status = PaymentStatus.Success,
                PaymentCode = GeneratePaymentCode(),
                TransactionRef = transactionId,
                PaidAt = paidAt
            };
            var allocation = new PaymentAllocation
            {
                Id = Guid.NewGuid(),
                PaymentId = payment.Id,
                InvoiceId = invoice.Id,
                Amount = notification.Amount
            };

            await _unitOfWork.Repository<Payment>().AddAsync(payment, ct);
            await _unitOfWork.Repository<PaymentAllocation>().AddAsync(allocation, ct);
            invoice.PaidAmount += notification.Amount;
            invoice.Status = invoice.PaidAmount >= invoice.TotalAmount ? InvoiceStatus.Paid : InvoiceStatus.Partial;
            invoice.BankTransactionId = transactionId;
            _unitOfWork.Repository<Invoice>().Update(invoice);
            if (invoice.Status == InvoiceStatus.Paid)
            {
                MarkVehicleRegistrationPaid(invoice, paidAt);
            }

            if (invoice.Status == InvoiceStatus.Paid && invoice.InvoiceKind == InvoiceKind.ContractPrepayment)
            {
                await ActivateContractPrepaymentAsync(invoice, ct);
            }

            await _unitOfWork.SaveChangesAsync(ct);
            await _auditLog.WriteAsync("Payment.BankTransferMatched", "Payment", payment.Id, transactionId, ct);
            await NotifyStudentAsync(payment.StudentId, "Payment confirmed", $"Invoice {invoice.InvoiceNumber} was paid.", ct);
            await tx.CommitAsync(ct);
            return new BankTransferProcessResultDto
            {
                Matched = true,
                Duplicate = false,
                InvoiceId = invoice.Id,
                PaymentId = payment.Id,
                Status = "Paid",
                Message = "Bank transfer matched one invoice."
            };
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<PaymentDto> ConfirmPaymentAsync(ConfirmPaymentRequest request, CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.PaymentsConfirm, ct);
        RequestValidator.ValidateAndThrow(request);
        await using var tx = await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var payment = _unitOfWork.Repository<Payment>().GetByIdAsync(request.PaymentId, ct).GetAwaiter().GetResult()
                ?? throw new InvalidOperationException("Payment was not found.");
            if (payment.Status != PaymentStatus.Pending)
            {
                throw new InvalidOperationException("Only pending payments can be confirmed.");
            }

            var invoices = GetInvoicesForPayment(payment);
            var outstandingTotal = invoices.Sum(invoice => invoice.TotalAmount - invoice.PaidAmount);
            if (outstandingTotal <= 0m)
            {
                throw new InvalidOperationException("Student has no outstanding invoice balance.");
            }

            if (payment.Amount > outstandingTotal)
            {
                throw new InvalidOperationException("Payment amount exceeds outstanding invoice balance.");
            }

            var paidAt = DateTime.UtcNow;
            var remaining = payment.Amount;
            foreach (var invoice in invoices)
            {
                if (remaining <= 0m)
                {
                    break;
                }

                var invoiceRemaining = invoice.TotalAmount - invoice.PaidAmount;
                var allocationAmount = Math.Min(invoiceRemaining, remaining);
                if (invoice.InvoiceKind == InvoiceKind.ContractPrepayment && allocationAmount != invoiceRemaining)
                {
                    throw new InvalidOperationException("Contract prepayment must be paid in full.");
                }

                var allocation = new PaymentAllocation
                {
                    Id = Guid.NewGuid(),
                    PaymentId = payment.Id,
                    InvoiceId = invoice.Id,
                    Amount = allocationAmount
                };
                await _unitOfWork.Repository<PaymentAllocation>().AddAsync(allocation, ct);
                invoice.PaidAmount += allocationAmount;
                invoice.Status = invoice.PaidAmount >= invoice.TotalAmount ? InvoiceStatus.Paid : InvoiceStatus.Partial;
                _unitOfWork.Repository<Invoice>().Update(invoice);
                if (invoice.Status == InvoiceStatus.Paid)
                {
                    MarkVehicleRegistrationPaid(invoice, paidAt);
                }

                if (invoice.Status == InvoiceStatus.Paid && invoice.InvoiceKind == InvoiceKind.ContractPrepayment)
                {
                    await ActivateContractPrepaymentAsync(invoice, ct);
                }

                remaining -= allocationAmount;
            }

            payment.Status = PaymentStatus.Success;
            payment.TransactionRef = string.IsNullOrWhiteSpace(request.TransactionRef)
                ? $"MOCK-TXN-{DateTime.UtcNow:yyyyMMddHHmmss}"
                : request.TransactionRef.Trim();
            payment.PaidAt = paidAt;
            _unitOfWork.Repository<Payment>().Update(payment);
            await _unitOfWork.SaveChangesAsync(ct);
            await _auditLog.WriteAsync("Payment.Confirmed", "Payment", request.PaymentId, payment.TransactionRef, ct);
            await NotifyStudentAsync(payment.StudentId, "Payment confirmed", $"Payment {payment.PaymentCode} was confirmed.", ct);
            await tx.CommitAsync(ct);
            return MapPayments(new[] { payment }).Single();
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    public async Task AllocatePaymentAsync(Guid paymentId, Guid invoiceId, decimal amount, CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.PaymentsConfirm, ct);
        if (amount <= 0m)
        {
            throw new InvalidOperationException("Allocation amount must be greater than zero.");
        }

        var payment = _unitOfWork.Repository<Payment>().GetByIdAsync(paymentId, ct).GetAwaiter().GetResult()
            ?? throw new InvalidOperationException("Payment was not found.");
        var invoice = _unitOfWork.Repository<Invoice>().GetByIdAsync(invoiceId, ct).GetAwaiter().GetResult()
            ?? throw new InvalidOperationException("Invoice was not found.");
        var allocatedPaymentAmount = _unitOfWork.Repository<PaymentAllocation>().Query()
            .Where(allocation => allocation.PaymentId == paymentId)
            .Sum(allocation => allocation.Amount);
        var paymentRemaining = payment.Amount - allocatedPaymentAmount;
        var invoiceRemaining = invoice.TotalAmount - invoice.PaidAmount;
        if (amount > paymentRemaining || amount > invoiceRemaining)
        {
            throw new InvalidOperationException("Allocation exceeds remaining payment or invoice balance.");
        }

        await _unitOfWork.Repository<PaymentAllocation>().AddAsync(new PaymentAllocation
        {
            Id = Guid.NewGuid(),
            PaymentId = paymentId,
            InvoiceId = invoiceId,
            Amount = amount
        }, ct);
        invoice.PaidAmount += amount;
        invoice.Status = invoice.PaidAmount >= invoice.TotalAmount ? InvoiceStatus.Paid : InvoiceStatus.Partial;
        _unitOfWork.Repository<Invoice>().Update(invoice);
        if (invoice.Status == InvoiceStatus.Paid)
        {
            MarkVehicleRegistrationPaid(invoice, DateTime.UtcNow);
        }

        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task CancelPaymentAsync(Guid paymentId, string reason, CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.PaymentsConfirm, ct);
        var payment = _unitOfWork.Repository<Payment>().GetByIdAsync(paymentId, ct).GetAwaiter().GetResult()
            ?? throw new InvalidOperationException("Payment was not found.");
        if (payment.Status != PaymentStatus.Pending)
        {
            throw new InvalidOperationException("Only pending payments can be cancelled.");
        }

        payment.Status = PaymentStatus.Cancelled;
        _unitOfWork.Repository<Payment>().Update(payment);
        await _unitOfWork.SaveChangesAsync(ct);
        await _auditLog.WriteAsync("Payment.Cancelled", "Payment", paymentId, reason, ct);
    }

    private Invoice GetPayableInvoice(Guid invoiceId, Guid studentId)
    {
        var invoice = _unitOfWork.Repository<Invoice>().GetByIdAsync(invoiceId).GetAwaiter().GetResult()
            ?? throw new InvalidOperationException("Invoice was not found.");
        if (invoice.StudentId != studentId)
        {
            throw new InvalidOperationException("Students can pay only their own invoices.");
        }

        if (!IsPayableInvoice(invoice))
        {
            throw new InvalidOperationException("Invoice has no outstanding balance.");
        }

        return invoice;
    }

    private Invoice GetPayableInvoiceForQr(Guid invoiceId)
    {
        var invoice = _unitOfWork.Repository<Invoice>().GetByIdAsync(invoiceId).GetAwaiter().GetResult()
            ?? throw new InvalidOperationException("Invoice was not found.");
        if (!IsPayableInvoice(invoice))
        {
            throw new InvalidOperationException("Invoice has no outstanding balance.");
        }

        return invoice;
    }

    private void EnsureInvoiceAccess(Invoice invoice)
    {
        if (!_currentUser.IsInRole(RoleNames.Student))
        {
            return;
        }

        var studentId = ResolveCurrentStudentId();
        if (invoice.StudentId != studentId)
        {
            throw new InvalidOperationException("Students can access only their own invoices.");
        }
    }

    private string GenerateTransferContent(Invoice invoice)
    {
        var primary = "K" + invoice.Id.ToString("N")[..8].ToUpperInvariant();
        var duplicateExists = _unitOfWork.Repository<Invoice>().Query()
            .Any(candidate => candidate.Id != invoice.Id && candidate.TransferContent == primary);
        if (!duplicateExists)
        {
            return primary;
        }

        return "P" + invoice.Id.ToString("N")[8..16].ToUpperInvariant();
    }

    private static long GeneratePayOsOrderCode(Guid invoiceId)
    {
        var raw = BitConverter.ToInt64(invoiceId.ToByteArray(), 0) & long.MaxValue;
        var normalized = raw % 9000000000000000L;
        return normalized == 0 ? 1 : normalized;
    }

    private static bool HasActivePayOsQr(Invoice invoice)
    {
        if (string.IsNullOrWhiteSpace(invoice.TransferContent) || string.IsNullOrWhiteSpace(invoice.QrDataUrl))
        {
            return false;
        }

        if (invoice.QrDataUrl.Contains("vietqr.io", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return invoice.TransferContent.Trim().Length <= 9;
    }

    private static bool IsPayableInvoice(Invoice invoice) =>
        PayableStatuses.Contains(invoice.Status) && invoice.TotalAmount > invoice.PaidAmount;

    private Student? GetStudent(Guid studentId) =>
        _unitOfWork.Repository<Student>().Query().FirstOrDefault(student => student.Id == studentId);

    private InvoicePaymentQrDto MapInvoicePaymentQr(Invoice invoice, PayOsPaymentLinkDto? paymentLink = null)
    {
        var paidAt = _unitOfWork.Repository<Payment>().Query()
            .Where(payment => payment.TargetInvoiceId == invoice.Id && payment.Status == PaymentStatus.Success)
            .OrderByDescending(payment => payment.PaidAt)
            .Select(payment => payment.PaidAt)
            .FirstOrDefault();
        return new InvoicePaymentQrDto
        {
            InvoiceId = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            StudentId = invoice.StudentId,
            Amount = Math.Max(0m, invoice.TotalAmount - invoice.PaidAmount),
            TransferContent = invoice.TransferContent ?? string.Empty,
            QrDataUrl = paymentLink?.QrDataUrl ?? invoice.QrDataUrl ?? string.Empty,
            CheckoutUrl = paymentLink?.CheckoutUrl ?? string.Empty,
            PaymentLinkId = paymentLink?.PaymentLinkId ?? string.Empty,
            ProviderOrderCode = paymentLink?.OrderCode,
            Status = invoice.Status.ToString(),
            DueDate = invoice.DueDate,
            PaidAt = paidAt
        };
    }

    private List<Invoice> GetInvoicesForPayment(Payment payment)
    {
        if (payment.TargetInvoiceId.HasValue)
        {
            var invoice = GetPayableInvoice(payment.TargetInvoiceId.Value, payment.StudentId);
            return new List<Invoice> { invoice };
        }

        return _unitOfWork.Repository<Invoice>().Query()
            .Where(invoice => invoice.StudentId == payment.StudentId
                && PayableStatuses.Contains(invoice.Status)
                && invoice.TotalAmount > invoice.PaidAmount)
            .OrderBy(invoice => invoice.DueDate)
            .ThenBy(invoice => invoice.IssueDate)
            .ToList();
    }

    private void MarkVehicleRegistrationPaid(Invoice invoice, DateTime paidAt)
    {
        if (invoice.InvoiceKind != InvoiceKind.VehicleParking)
        {
            return;
        }

        var registration = _unitOfWork.Repository<VehicleRegistration>().Query()
            .FirstOrDefault(candidate => candidate.InvoiceId == invoice.Id);
        if (registration is null)
        {
            return;
        }

        registration.PaymentDate = paidAt.Date;
        registration.UpdatedAt = paidAt;
        _unitOfWork.Repository<VehicleRegistration>().Update(registration);
    }

    private async Task ActivateContractPrepaymentAsync(Invoice invoice, CancellationToken ct)
    {
        var contract = invoice.ContractId.HasValue
            ? _unitOfWork.Repository<Contract>().GetByIdAsync(invoice.ContractId.Value, ct).GetAwaiter().GetResult()
            : _unitOfWork.Repository<Contract>().Query().FirstOrDefault(candidate => candidate.UpfrontInvoiceId == invoice.Id);
        if (contract is null || contract.Status == ContractStatus.Active)
        {
            return;
        }

        if (contract.Status != ContractStatus.PendingPayment)
        {
            throw new InvalidOperationException("Only pending-payment contracts can be activated.");
        }

        var registration = contract.RoomRegistrationId.HasValue
            ? _unitOfWork.Repository<RoomRegistration>().GetByIdAsync(contract.RoomRegistrationId.Value, ct).GetAwaiter().GetResult()
            : null;
        var student = _unitOfWork.Repository<Student>().GetByIdAsync(contract.StudentId, ct).GetAwaiter().GetResult()
            ?? throw new InvalidOperationException("Student was not found.");
        var room = _unitOfWork.Repository<Room>().GetByIdAsync(contract.RoomId, ct).GetAwaiter().GetResult()
            ?? throw new InvalidOperationException("Room was not found.");
        if (_unitOfWork.Repository<RoomAssignment>().Query().Any(assignment => assignment.StudentId == student.Id && assignment.IsActive))
        {
            throw new InvalidOperationException("Student already has an active room assignment.");
        }

        var assignment = new RoomAssignment
        {
            Id = Guid.NewGuid(),
            StudentId = student.Id,
            RoomId = room.Id,
            StartDate = contract.StartDate,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        contract.Status = ContractStatus.Active;
        if (registration is not null)
        {
            registration.Status = RegistrationStatus.Approved;
            registration.ReviewedAt ??= DateTime.UtcNow;
            registration.ReviewedByUserId ??= _currentUser.UserId;
            _unitOfWork.Repository<RoomRegistration>().Update(registration);
        }

        room.CurrentOccupancy = Math.Max(room.CurrentOccupancy, CountActiveAssignments(room.Id)) + 1;
        room.Status = room.CurrentOccupancy >= room.Capacity ? RoomStatus.Full : RoomStatus.Available;
        student.CurrentRoomId = room.Id;
        student.Status = StudentStatus.Staying;
        await _unitOfWork.Repository<RoomAssignment>().AddAsync(assignment, ct);
        _unitOfWork.Repository<Contract>().Update(contract);
        _unitOfWork.Repository<Room>().Update(room);
        _unitOfWork.Repository<Student>().Update(student);
        await _auditLog.WriteAsync("Contract.Activated", "Contract", contract.Id, contract.ContractNumber, ct);
        await _auditLog.WriteAsync("RoomAssignment.Created", "RoomAssignment", assignment.Id, room.RoomNumber, ct);
        await _auditLog.WriteAsync("RoomRegistration.Activated", "RoomRegistration", registration?.Id, invoice.InvoiceNumber, ct);
    }

    private int CountActiveAssignments(Guid roomId) =>
        _unitOfWork.Repository<RoomAssignment>().Query()
            .Count(assignment => assignment.RoomId == roomId && assignment.IsActive);

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

    private IReadOnlyList<PaymentDto> MapPayments(IReadOnlyList<Payment> payments)
    {
        if (payments.Count == 0)
        {
            return Array.Empty<PaymentDto>();
        }

        var studentIds = payments.Select(payment => payment.StudentId).ToHashSet();
        var students = _unitOfWork.Repository<Student>().Query()
            .Where(student => studentIds.Contains(student.Id))
            .ToDictionary(student => student.Id);

        return payments.Select(payment =>
        {
            students.TryGetValue(payment.StudentId, out var student);
            var targetInvoice = payment.TargetInvoiceId.HasValue
                ? _unitOfWork.Repository<Invoice>().Query().FirstOrDefault(invoice => invoice.Id == payment.TargetInvoiceId.Value)
                : null;
            return new PaymentDto
            {
                Id = payment.Id,
                PaymentCode = payment.PaymentCode,
                StudentId = payment.StudentId,
                StudentCode = student?.StudentCode ?? string.Empty,
                StudentName = student?.FullName ?? string.Empty,
                TargetInvoiceId = payment.TargetInvoiceId,
                TargetInvoiceNumber = targetInvoice?.InvoiceNumber,
                Amount = payment.Amount,
                Method = payment.Method,
                Status = payment.Status,
                TransactionRef = payment.TransactionRef,
                CreatedAt = payment.CreatedAt,
                PaidAt = payment.PaidAt
            };
        }).ToArray();
    }

    private static string GeneratePaymentCode() => $"PAY-{DateTime.UtcNow:yyyyMMddHHmmssfff}";

    private async Task NotifyStudentAsync(Guid studentId, string title, string message, CancellationToken ct)
    {
        if (_notifications is null)
        {
            return;
        }

        var student = _unitOfWork.Repository<Student>().Query().FirstOrDefault(candidate => candidate.Id == studentId);
        if (student?.UserId is { } userId)
        {
            await _notifications.NotifyUserAsync(userId, title, message, ct);
        }
    }

    private async Task NotifyRoleAsync(string roleName, string title, string message, CancellationToken ct)
    {
        if (_notifications is not null)
        {
            await _notifications.NotifyRoleAsync(roleName, title, message, ct);
        }
    }
}








