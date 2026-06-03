using System.Text.RegularExpressions;
using DormitoryManagement.Application.Abstractions.Auth;
using DormitoryManagement.Application.Abstractions.Data;
using DormitoryManagement.Application.Abstractions.Services;
using DormitoryManagement.Application.DTOs.Vehicles;
using DormitoryManagement.Application.Validation;
using DormitoryManagement.Domain.Constants;
using DormitoryManagement.Domain.Entities;
using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Application.Services.Vehicles;

public sealed partial class VehicleService : IVehicleService
{
    private const decimal MonthlyParkingFee = 40000m;
    private static readonly int[] AllowedMonthCounts = { 1, 2, 3, 6, 9, 12 };
    private readonly IPermissionService _permissions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogService _auditLog;
    private readonly ICurrentUserService _currentUser;

    public VehicleService(
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

    public Task<IReadOnlyList<VehicleRegistrationDto>> GetCurrentStudentVehicleRegistrationsAsync(DateTime? asOfDate = null, CancellationToken ct = default)
    {
        var studentId = ResolveCurrentStudentId();
        var invoiceIds = _unitOfWork.Repository<VehicleRegistration>().Query()
            .Where(registration => registration.StudentId == studentId && registration.InvoiceId.HasValue)
            .Select(registration => registration.InvoiceId!.Value)
            .ToHashSet();
        var invoices = _unitOfWork.Repository<Invoice>().Query()
            .Where(invoice => invoiceIds.Contains(invoice.Id))
            .ToDictionary(invoice => invoice.Id);
        var registrations = _unitOfWork.Repository<VehicleRegistration>().Query()
            .Where(registration => registration.StudentId == studentId)
            .OrderBy(registration => registration.RegisteredAt)
            .ToList();

        return Task.FromResult<IReadOnlyList<VehicleRegistrationDto>>(MapRegistrations(registrations, invoices, asOfDate ?? DateTime.UtcNow));
    }

    public async Task<VehicleRegistrationDto> RegisterVehicleAsync(CreateVehicleRegistrationRequest request, CancellationToken ct = default)
    {
        RequestValidator.ValidateAndThrow(request);
        var normalizedPlate = NormalizeLicensePlate(request.LicensePlate);
        if (!AllowedMonthCounts.Contains(request.MonthCount))
        {
            throw new InvalidOperationException("Số tháng đăng ký giữ xe không hợp lệ.");
        }

        var studentId = ResolveCurrentStudentId();
        var student = _unitOfWork.Repository<Student>().GetByIdAsync(studentId, ct).GetAwaiter().GetResult()
            ?? throw new InvalidOperationException("Student was not found.");
        if (student.CurrentRoomId is null)
        {
            throw new InvalidOperationException("Sinh viên cần được duyệt vào phòng trước khi đăng ký giữ xe.");
        }

        var room = _unitOfWork.Repository<Room>().GetByIdAsync(student.CurrentRoomId.Value, ct).GetAwaiter().GetResult()
            ?? throw new InvalidOperationException("Current room was not found.");
        var now = DateTime.UtcNow;
        var activeCandidates = _unitOfWork.Repository<VehicleRegistration>().Query()
            .ToList()
            .Where(registration => TryNormalizeLicensePlate(registration.LicensePlate, out var existingPlate) && existingPlate == normalizedPlate)
            .ToList();
        var activeInvoiceIds = activeCandidates
            .Where(registration => registration.InvoiceId.HasValue)
            .Select(registration => registration.InvoiceId!.Value)
            .ToHashSet();
        var activeInvoices = _unitOfWork.Repository<Invoice>().Query()
            .Where(invoice => activeInvoiceIds.Contains(invoice.Id))
            .ToDictionary(invoice => invoice.Id);
        if (activeCandidates.Any(registration => IsActiveRegistration(registration, activeInvoices, now)))
        {
            throw new InvalidOperationException("Biển số xe đang có đăng ký giữ xe còn hiệu lực.");
        }

        var amount = request.MonthCount * MonthlyParkingFee;
        await using var tx = await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var registration = new VehicleRegistration
            {
                Id = Guid.NewGuid(),
                StudentId = student.Id,
                LicensePlate = normalizedPlate,
                VehicleType = "Motorbike",
                MonthCount = request.MonthCount,
                Amount = amount,
                Status = VehicleStatus.Pending,
                RegisteredAt = now,
                CreatedAt = now
            };
            var invoice = new Invoice
            {
                Id = Guid.NewGuid(),
                StudentId = student.Id,
                RoomId = room.Id,
                BillingPeriod = now.ToString("yyyy-MM"),
                InvoiceKind = InvoiceKind.VehicleParking,
                IssueDate = now,
                DueDate = now.Date.AddDays(2),
                TotalAmount = amount,
                PaidAmount = 0m,
                Status = InvoiceStatus.Unpaid,
                CreatedAt = now
            };
            invoice.InvoiceNumber = GenerateParkingInvoiceNumber(now, invoice.Id);
            var item = new InvoiceItem
            {
                Id = Guid.NewGuid(),
                InvoiceId = invoice.Id,
                FeeTypeId = GetParkingFeeTypeId(),
                Description = $"Giữ xe máy {normalizedPlate} ({request.MonthCount} tháng)",
                Quantity = request.MonthCount,
                UnitPrice = MonthlyParkingFee,
                Amount = amount,
                CreatedAt = now
            };
            registration.InvoiceId = invoice.Id;
            invoice.Items.Add(item);

            await _unitOfWork.Repository<VehicleRegistration>().AddAsync(registration, ct);
            await _unitOfWork.Repository<Invoice>().AddAsync(invoice, ct);
            await _unitOfWork.Repository<InvoiceItem>().AddAsync(item, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            await _auditLog.WriteAsync("Invoice.Created", "Invoice", invoice.Id, invoice.InvoiceNumber, ct);
            await tx.CommitAsync(ct);

            return MapRegistrations(new[] { registration }, new Dictionary<Guid, Invoice> { [invoice.Id] = invoice }, now).Single();
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    public async Task ApproveVehicleAsync(Guid registrationId, CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.VehiclesApprove, ct);
    }

    public async Task RejectVehicleAsync(Guid registrationId, string reason, CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.VehiclesApprove, ct);
    }

    public Task CancelVehicleAsync(Guid registrationId, CancellationToken ct = default) => Task.CompletedTask;

    private Guid ResolveCurrentStudentId()
    {
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

    private Guid? GetParkingFeeTypeId() =>
        _unitOfWork.Repository<FeeType>().Query()
            .ToList()
            .FirstOrDefault(feeType => feeType.Code.Equals("PARKING", StringComparison.OrdinalIgnoreCase))?.Id;

    private static IReadOnlyList<VehicleRegistrationDto> MapRegistrations(IReadOnlyList<VehicleRegistration> registrations, IReadOnlyDictionary<Guid, Invoice> invoices, DateTime asOfDate)
    {
        var row = 0;
        return registrations.Select(registration =>
        {
            row++;
            var invoice = registration.InvoiceId.HasValue && invoices.TryGetValue(registration.InvoiceId.Value, out var found)
                ? found
                : null;
            var paymentDate = registration.PaymentDate ?? (invoice?.Status == InvoiceStatus.Paid ? invoice.UpdatedAt ?? invoice.IssueDate : null);
            var applicationDate = paymentDate?.Date.AddDays(1);
            var expiryDate = applicationDate?.AddMonths(registration.MonthCount).AddDays(-1);
            var statusText = ResolveStatusText(paymentDate, applicationDate, expiryDate, asOfDate);
            var normalizedPlate = TryNormalizeLicensePlate(registration.LicensePlate, out var parsedPlate)
                ? parsedPlate
                : registration.LicensePlate;
            return new VehicleRegistrationDto
            {
                Id = registration.Id,
                RowNumber = row,
                StudentId = registration.StudentId,
                LicensePlate = normalizedPlate,
                NormalizedPlate = normalizedPlate,
                VehicleType = registration.VehicleType,
                RegisteredAt = registration.RegisteredAt,
                PaymentDate = paymentDate?.Date,
                Amount = registration.Amount,
                MonthCount = registration.MonthCount,
                Status = registration.Status,
                StatusText = statusText,
                ExpiryDate = expiryDate,
                InvoiceStatus = invoice?.Status ?? InvoiceStatus.Unpaid
            };
        }).ToArray();
    }

    private static string NormalizeLicensePlate(string value)
    {
        var compact = value.Trim().ToUpperInvariant().Replace("-", string.Empty);
        if (!CompactPlateRegex().IsMatch(compact))
        {
            throw new InvalidOperationException("Nhập sai định dạng biển số.");
        }

        return $"{compact[..4]}-{compact[4..]}";
    }

    private static bool TryNormalizeLicensePlate(string value, out string normalizedPlate)
    {
        normalizedPlate = string.Empty;
        try
        {
            normalizedPlate = NormalizeLicensePlate(value);
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    private static string GenerateParkingInvoiceNumber(DateTime issueDate, Guid invoiceId) =>
        $"INV-PARK-{issueDate:yyyyMM}-{invoiceId.ToString("N")[..8]}";

    private static string ResolveStatusText(DateTime? paymentDate, DateTime? applicationDate, DateTime? expiryDate, DateTime asOfDate)
    {
        if (paymentDate is null)
        {
            return "Chưa thanh toán";
        }

        if (expiryDate.HasValue && asOfDate.Date > expiryDate.Value.Date)
        {
            return "Hết hạn";
        }

        return asOfDate.Date < applicationDate!.Value.Date ? "Đã thanh toán" : "Đã thanh toán và áp dụng";
    }

    private static bool IsActiveRegistration(
        VehicleRegistration registration,
        IReadOnlyDictionary<Guid, Invoice> invoices,
        DateTime asOfDate)
    {
        if (registration.Status is VehicleStatus.Rejected or VehicleStatus.Cancelled or VehicleStatus.Expired)
        {
            return false;
        }

        var paymentDate = registration.PaymentDate;
        if (paymentDate is null
            && registration.InvoiceId.HasValue
            && invoices.TryGetValue(registration.InvoiceId.Value, out var invoice)
            && invoice.Status == InvoiceStatus.Paid)
        {
            paymentDate = invoice.UpdatedAt ?? invoice.IssueDate;
        }

        if (paymentDate is null)
        {
            return true;
        }

        var expiryDate = paymentDate.Value.Date.AddDays(1).AddMonths(registration.MonthCount).AddDays(-1);
        return expiryDate.Date >= asOfDate.Date;
    }

    [GeneratedRegex(@"^\d{2}[A-Z]\d{5,6}$")]
    private static partial Regex CompactPlateRegex();
}
