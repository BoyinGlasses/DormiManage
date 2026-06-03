using DormitoryManagement.Application.Abstractions.Auth;
using DormitoryManagement.Application.Abstractions.Data;
using DormitoryManagement.Application.Abstractions.Services;
using DormitoryManagement.Application.DTOs.Billing;
using DormitoryManagement.Application.Validation;
using DormitoryManagement.Domain.Constants;
using DormitoryManagement.Domain.Entities;
using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Application.Services.Billing;

public sealed class BillingService : IBillingService
{
    private const decimal ElectricityUnitPrice = 3500m;
    private const decimal WaterUnitPrice = 19500m;
    private const decimal InternetUnitPrice = 50000m;
    private readonly IPermissionService _permissions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogService _auditLog;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationService? _notifications;

    public BillingService(
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

    public Task<UtilityBillingPreviewDto> PreviewUtilityBillingAsync(UtilityBillingPreviewRequest request, CancellationToken ct = default)
    {
        return PreviewUtilityBillingCoreAsync(request, ct);
    }

    private async Task<UtilityBillingPreviewDto> PreviewUtilityBillingCoreAsync(UtilityBillingPreviewRequest request, CancellationToken ct)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.BillingRead, ct);
        RequestValidator.ValidateAndThrow(request);
        var room = await _unitOfWork.Repository<Room>().GetByIdAsync(request.RoomId, ct)
            ?? throw new InvalidOperationException("Room was not found.");
        EnsureCanManageRoom(room);

        var periodStart = ParseBillingPeriod(request.BillingPeriod);
        var periodEnd = periodStart.AddMonths(1).AddDays(-1);
        var previousPeriod = periodStart.AddMonths(-1).ToString("yyyy-MM");
        var previousReading = _unitOfWork.Repository<UtilityReading>().Query()
            .FirstOrDefault(reading => reading.RoomId == request.RoomId && reading.BillingPeriod == previousPeriod);
        var electricityPrevious = previousReading?.ElectricityCurrent ?? request.ElectricityPrevious ?? 0m;
        var waterPrevious = previousReading?.WaterCurrent ?? request.WaterPrevious ?? 0m;

        if (request.ElectricityCurrent < electricityPrevious)
        {
            throw new InvalidOperationException("Current electricity reading cannot be lower than previous reading.");
        }

        if (request.WaterCurrent < waterPrevious)
        {
            throw new InvalidOperationException("Current water reading cannot be lower than previous reading.");
        }

        var feeTypes = _unitOfWork.Repository<FeeType>().Query().Where(feeType => !feeType.IsDeleted).ToList();
        var rates = _unitOfWork.Repository<FeeRate>().Query().ToList();
        var electricityRate = TryGetRate("ELECTRICITY", periodStart, feeTypes, rates, out _, out var configuredElectricityRate)
            ? configuredElectricityRate
            : ElectricityUnitPrice;
        var waterRate = TryGetRate("WATER", periodStart, feeTypes, rates, out _, out var configuredWaterRate)
            ? configuredWaterRate
            : WaterUnitPrice;
        var internetRate = TryGetRate("INTERNET", periodStart, feeTypes, rates, out _, out var configuredInternetRate)
            ? configuredInternetRate
            : InternetUnitPrice;
        var activeContracts = _unitOfWork.Repository<Contract>().Query()
            .Where(contract => contract.RoomId == request.RoomId
                && contract.Status == ContractStatus.Active
                && contract.StartDate <= periodEnd
                && contract.EndDate >= periodStart)
            .ToList();
        var activeMemberCount = activeContracts.Count;
        var electricityConsumption = request.ElectricityCurrent - electricityPrevious;
        var waterConsumption = request.WaterCurrent - waterPrevious;
        var electricityAmount = Math.Round(electricityConsumption * electricityRate, 2);
        var waterAmount = Math.Round(waterConsumption * waterRate, 2);
        var internetAmount = activeMemberCount * internetRate;
        var totalAmount = electricityAmount + waterAmount + internetAmount;

        return new UtilityBillingPreviewDto
        {
            RoomId = room.Id,
            RoomNumber = room.RoomNumber,
            BillingPeriod = request.BillingPeriod,
            ElectricityPrevious = electricityPrevious,
            ElectricityCurrent = request.ElectricityCurrent,
            ElectricityConsumption = electricityConsumption,
            ElectricityAmount = electricityAmount,
            WaterPrevious = waterPrevious,
            WaterCurrent = request.WaterCurrent,
            WaterConsumption = waterConsumption,
            WaterAmount = waterAmount,
            ActiveMemberCount = activeMemberCount,
            InternetSubscriberCount = activeMemberCount,
            InternetUnitPrice = internetRate,
            InternetAmount = internetAmount,
            TotalAmount = totalAmount,
            PerMemberAmount = activeMemberCount == 0 ? 0m : Math.Round(totalAmount / activeMemberCount, 2),
            DueDate = CalculateMonthlyUtilityDueDate(periodStart),
            ValidationMessages = Array.Empty<string>()
        };
    }

    public async Task UpsertUtilityReadingAsync(UtilityReadingRequest request, CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.BillingWrite, ct);
        RequestValidator.ValidateAndThrow(request);
        var room = _unitOfWork.Repository<Room>().GetByIdAsync(request.RoomId, ct).GetAwaiter().GetResult()
            ?? throw new InvalidOperationException("Room was not found.");
        EnsureCanManageRoom(room);
        var periodStart = ParseBillingPeriod(request.BillingPeriod);
        var previousPeriod = periodStart.AddMonths(-1).ToString("yyyy-MM");
        var previousReading = _unitOfWork.Repository<UtilityReading>().Query()
            .FirstOrDefault(reading => reading.RoomId == request.RoomId && reading.BillingPeriod == previousPeriod);
        var electricityPrevious = previousReading?.ElectricityCurrent ?? request.ElectricityPrevious ?? 0m;
        var waterPrevious = previousReading?.WaterCurrent ?? request.WaterPrevious ?? 0m;

        if (request.ElectricityCurrent < electricityPrevious || request.WaterCurrent < waterPrevious)
        {
            throw new InvalidOperationException("Current utility readings cannot be lower than previous readings.");
        }

        var reading = _unitOfWork.Repository<UtilityReading>().Query()
            .FirstOrDefault(candidate => candidate.RoomId == request.RoomId && candidate.BillingPeriod == request.BillingPeriod);
        if (reading is null)
        {
            reading = new UtilityReading
            {
                Id = Guid.NewGuid(),
                RoomId = request.RoomId,
                BillingPeriod = request.BillingPeriod,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Repository<UtilityReading>().AddAsync(reading, ct);
        }

        reading.ElectricityPrevious = electricityPrevious;
        reading.ElectricityCurrent = request.ElectricityCurrent;
        reading.WaterPrevious = waterPrevious;
        reading.WaterCurrent = request.WaterCurrent;
        reading.RecordedAt = DateTime.UtcNow;
        _unitOfWork.Repository<UtilityReading>().Update(reading);
        await _unitOfWork.SaveChangesAsync(ct);
        await _auditLog.WriteAsync("UtilityReading.Upserted", "UtilityReading", reading.Id, request.BillingPeriod, ct);
    }

    public async Task<GenerateMonthlyInvoiceResult> GenerateMonthlyInvoicesAsync(GenerateMonthlyInvoiceRequest request, CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.BillingWrite, ct);
        RequestValidator.ValidateAndThrow(request);
        var periodStart = ParseBillingPeriod(request.BillingPeriod);
        var periodEnd = periodStart.AddMonths(1).AddDays(-1);
        if (request.RoomId.HasValue)
        {
            var requestedRoom = await _unitOfWork.Repository<Room>().GetByIdAsync(request.RoomId.Value, ct)
                ?? throw new InvalidOperationException("Room was not found.");
            EnsureCanManageRoom(requestedRoom);
        }

        await using var tx = await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var result = new GenerateMonthlyInvoiceResult();
            var warnings = new List<string>();
            var createdInvoices = new List<Invoice>();
            var contracts = _unitOfWork.Repository<Contract>().Query()
                .Where(contract => contract.Status == ContractStatus.Active
                    && contract.StartDate <= periodEnd
                    && contract.EndDate >= periodStart)
                .OrderBy(contract => contract.StudentId)
                .ToList();
            if (request.RoomId.HasValue)
            {
                contracts = contracts.Where(contract => contract.RoomId == request.RoomId.Value).ToList();
            }

            if (_currentUser.IsInRole(RoleNames.BuildingManager) && _currentUser.CurrentUser?.BuildingId is { } currentBuildingId)
            {
                var allowedRoomIds = _unitOfWork.Repository<Room>().Query()
                    .Where(room => room.BuildingId == currentBuildingId)
                    .Select(room => room.Id)
                    .ToHashSet();
                contracts = contracts.Where(contract => allowedRoomIds.Contains(contract.RoomId)).ToList();
            }

            var existingKeys = _unitOfWork.Repository<Invoice>().Query()
                .Where(invoice => invoice.BillingPeriod == request.BillingPeriod && invoice.InvoiceKind == InvoiceKind.MonthlyUtility)
                .Select(invoice => new { invoice.StudentId, invoice.RoomId, invoice.BillingPeriod, invoice.InvoiceKind })
                .ToList()
                .Select(invoice => (invoice.StudentId, invoice.RoomId, invoice.BillingPeriod, invoice.InvoiceKind))
                .ToHashSet();
            var feeTypes = _unitOfWork.Repository<FeeType>().Query().Where(feeType => !feeType.IsDeleted).ToList();
            var rates = _unitOfWork.Repository<FeeRate>().Query().ToList();
            var readings = _unitOfWork.Repository<UtilityReading>().Query()
                .Where(reading => reading.BillingPeriod == request.BillingPeriod)
                .ToList()
                .ToDictionary(reading => reading.RoomId);
            var rooms = _unitOfWork.Repository<Room>().Query()
                .Where(room => contracts.Select(contract => contract.RoomId).Contains(room.Id))
                .ToDictionary(room => room.Id);
            var dueDate = CalculateMonthlyUtilityDueDate(periodStart);

            foreach (var roomContracts in contracts.GroupBy(contract => contract.RoomId))
            {
                if (!readings.TryGetValue(roomContracts.Key, out var reading))
                {
                    var roomNumber = rooms.TryGetValue(roomContracts.Key, out var room) ? room.RoomNumber : roomContracts.Key.ToString();
                    warnings.Add($"Missing utility reading for room {roomNumber} in {request.BillingPeriod}.");
                    continue;
                }

                var activeOccupants = roomContracts.Count();
                foreach (var contract in roomContracts)
                {
                    var key = (contract.StudentId, contract.RoomId, request.BillingPeriod, InvoiceKind.MonthlyUtility);
                    if (existingKeys.Contains(key))
                    {
                        result.SkippedCount++;
                        continue;
                    }

                    var items = BuildInvoiceItems(contract, request.BillingPeriod, periodStart, feeTypes, rates, reading, activeOccupants);
                    var total = items.Sum(item => item.Amount);
                    var invoiceId = Guid.NewGuid();
                    var invoice = new Invoice
                    {
                        Id = invoiceId,
                        InvoiceNumber = GenerateInvoiceNumber(request.BillingPeriod, invoiceId),
                        StudentId = contract.StudentId,
                        RoomId = contract.RoomId,
                        BillingPeriod = request.BillingPeriod,
                        InvoiceKind = InvoiceKind.MonthlyUtility,
                        IssueDate = DateTime.UtcNow,
                        DueDate = dueDate,
                        TotalAmount = total,
                        PaidAmount = 0m,
                        Status = InvoiceStatus.Unpaid,
                        Items = items
                    };

                    foreach (var item in invoice.Items)
                    {
                        item.InvoiceId = invoice.Id;
                    }

                    await _unitOfWork.Repository<Invoice>().AddAsync(invoice, ct);
                    existingKeys.Add(key);
                    createdInvoices.Add(invoice);
                    result.CreatedCount++;
                }
            }

            await _unitOfWork.SaveChangesAsync(ct);
            await _auditLog.WriteAsync("Invoice.GeneratedMonthly", "Invoice", null, request.BillingPeriod, ct);
            foreach (var invoice in createdInvoices)
            {
                await NotifyStudentAsync(invoice.StudentId, "Invoice generated", $"Invoice {invoice.InvoiceNumber} is ready for {request.BillingPeriod}.", ct);
            }
            await tx.CommitAsync(ct);
            result.MissingUtilityReadingCount = warnings.Count;
            result.Warnings = warnings;
            return result;
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<IReadOnlyList<InvoiceDto>> GetInvoicesAsync(string? billingPeriod = null, CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.BillingRead, ct);
        var query = _unitOfWork.Repository<Invoice>().Query();
        if (!string.IsNullOrWhiteSpace(billingPeriod))
        {
            query = query.Where(invoice => invoice.BillingPeriod == billingPeriod.Trim());
        }

        if (_currentUser.IsInRole(RoleNames.Student))
        {
            var studentId = ResolveCurrentStudentId();
            query = query.Where(invoice => invoice.StudentId == studentId);
        }
        else if (_currentUser.IsInRole(RoleNames.BuildingManager) && _currentUser.CurrentUser?.BuildingId is { } buildingId)
        {
            var allowedRoomIds = _unitOfWork.Repository<Room>().Query()
                .Where(room => room.BuildingId == buildingId)
                .Select(room => room.Id)
                .ToHashSet();
            query = query.Where(invoice => allowedRoomIds.Contains(invoice.RoomId));
        }

        var overdueInvoices = query
            .Where(invoice => invoice.DueDate.Date < DateTime.UtcNow.Date
                && (invoice.Status == InvoiceStatus.Unpaid || invoice.Status == InvoiceStatus.Partial))
            .ToList();
        foreach (var invoice in overdueInvoices)
        {
            invoice.Status = InvoiceStatus.Overdue;
            _unitOfWork.Repository<Invoice>().Update(invoice);
        }

        if (overdueInvoices.Count > 0)
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }

        var invoices = query
            .OrderByDescending(invoice => invoice.BillingPeriod)
            .ThenBy(invoice => invoice.InvoiceNumber)
            .ToList();
        return MapInvoices(invoices);
    }

    public async Task<IReadOnlyList<StudentBillingRowDto>> GetStudentBillingRowsAsync(string? billingPeriod = null, CancellationToken ct = default)
    {
        var invoices = await GetInvoicesAsync(billingPeriod, ct);
        return invoices.Select(MapStudentBillingRow).ToArray();
    }

    public async Task<InvoiceDto?> GetInvoiceAsync(Guid invoiceId, CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.BillingRead, ct);
        var invoice = _unitOfWork.Repository<Invoice>().GetByIdAsync(invoiceId, ct).GetAwaiter().GetResult();
        if (invoice is null)
        {
            return null;
        }

        if (_currentUser.IsInRole(RoleNames.Student) && invoice.StudentId != ResolveCurrentStudentId())
        {
            throw new InvalidOperationException("Students can view only their own invoices.");
        }
        EnsureCanViewInvoice(invoice);

        return MapInvoices(new[] { invoice }).Single();
    }

    public async Task<InvoiceDto> CreateInvoiceAsync(Guid studentId, Guid roomId, string billingPeriod, CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.BillingWrite, ct);
        if (_unitOfWork.Repository<Invoice>().Query().Any(invoice => invoice.StudentId == studentId
            && invoice.RoomId == roomId
            && invoice.BillingPeriod == billingPeriod
            && invoice.InvoiceKind == InvoiceKind.MonthlyUtility))
        {
            throw new InvalidOperationException("Invoice already exists for this student, room, and billing period.");
        }

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            StudentId = studentId,
            RoomId = roomId,
            BillingPeriod = billingPeriod,
            InvoiceKind = InvoiceKind.MonthlyUtility,
            IssueDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.Date.AddDays(14),
            Status = InvoiceStatus.Unpaid
        };
        invoice.InvoiceNumber = GenerateInvoiceNumber(billingPeriod, invoice.Id);
        await _unitOfWork.Repository<Invoice>().AddAsync(invoice, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return MapInvoices(new[] { invoice }).Single();
    }

    public async Task AddInvoiceItemAsync(Guid invoiceId, InvoiceItemDto item, CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.BillingWrite, ct);
        RequestValidator.ValidateAndThrow(item);
        var invoice = _unitOfWork.Repository<Invoice>().GetByIdAsync(invoiceId, ct).GetAwaiter().GetResult()
            ?? throw new InvalidOperationException("Invoice was not found.");
        var entity = new InvoiceItem
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoiceId,
            Description = item.Description,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            Amount = item.Quantity * item.UnitPrice
        };
        invoice.Items.Add(entity);
        invoice.TotalAmount += entity.Amount;
        _unitOfWork.Repository<Invoice>().Update(invoice);
        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<int> MarkOverdueInvoicesAsync(DateTime asOfDate, CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.BillingWrite, ct);
        var invoices = _unitOfWork.Repository<Invoice>().Query()
            .Where(invoice => invoice.DueDate.Date < asOfDate.Date
                && (invoice.Status == InvoiceStatus.Unpaid || invoice.Status == InvoiceStatus.Partial))
            .ToList();
        foreach (var invoice in invoices)
        {
            invoice.Status = InvoiceStatus.Overdue;
            _unitOfWork.Repository<Invoice>().Update(invoice);
        }

        await _unitOfWork.SaveChangesAsync(ct);
        return invoices.Count;
    }

    public async Task AdjustInvoiceAsync(Guid invoiceId, decimal amount, string reason, CancellationToken ct = default)
    {
        await _permissions.EnsurePermissionAsync(PermissionNames.BillingWrite, ct);
        await using var tx = await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            // TODO: Add adjustment, recalculate total, audit create/edit/cancel invoice action.
            await _unitOfWork.SaveChangesAsync(ct);
            await _auditLog.WriteAsync("Invoice.Adjusted", "Invoice", invoiceId, reason, ct);
            await tx.CommitAsync(ct);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    private List<InvoiceItem> BuildInvoiceItems(
        Contract contract,
        string billingPeriod,
        DateTime periodStart,
        IReadOnlyList<FeeType> feeTypes,
        IReadOnlyList<FeeRate> rates,
        UtilityReading reading,
        int activeOccupants)
    {
        var items = new List<InvoiceItem>();
        var occupantCount = Math.Max(1, activeOccupants);

        if (TryGetRate("ELECTRICITY", periodStart, feeTypes, rates, out var electricityFeeTypeId, out var electricityRate))
        {
            var quantity = Math.Max(0m, reading.ElectricityCurrent - reading.ElectricityPrevious) / occupantCount;
            items.Add(CreateItem(electricityFeeTypeId, $"Electricity {billingPeriod}", quantity, electricityRate));
        }

        if (TryGetRate("WATER", periodStart, feeTypes, rates, out var waterFeeTypeId, out var waterRate))
        {
            var quantity = Math.Max(0m, reading.WaterCurrent - reading.WaterPrevious) / occupantCount;
            items.Add(CreateItem(waterFeeTypeId, $"Water {billingPeriod}", quantity, waterRate));
        }

        if (TryGetRate("INTERNET", periodStart, feeTypes, rates, out var internetFeeTypeId, out var internetRate))
        {
            items.Add(CreateItem(internetFeeTypeId, $"Internet {billingPeriod}", 1m, internetRate));
        }

        return items;
    }

    private static InvoiceItem CreateItem(Guid? feeTypeId, string description, decimal quantity, decimal unitPrice) =>
        new()
        {
            Id = Guid.NewGuid(),
            FeeTypeId = feeTypeId,
            Description = description,
            Quantity = quantity,
            UnitPrice = unitPrice,
            Amount = Math.Round(quantity * unitPrice, 2),
            CreatedAt = DateTime.UtcNow
        };

    private static Guid? GetFeeTypeId(IEnumerable<FeeType> feeTypes, string code) =>
        feeTypes.FirstOrDefault(feeType => string.Equals(feeType.Code, code, StringComparison.OrdinalIgnoreCase))?.Id;

    private static bool TryGetRate(string code, DateTime asOfDate, IReadOnlyList<FeeType> feeTypes, IReadOnlyList<FeeRate> rates, out Guid? feeTypeId, out decimal amount)
    {
        feeTypeId = GetFeeTypeId(feeTypes, code);
        if (feeTypeId is null)
        {
            amount = 0m;
            return false;
        }

        var id = feeTypeId.Value;
        var rate = rates
            .Where(candidate => candidate.FeeTypeId == id
                && candidate.EffectiveFrom.Date <= asOfDate.Date
                && (!candidate.EffectiveTo.HasValue || candidate.EffectiveTo.Value.Date >= asOfDate.Date))
            .OrderByDescending(candidate => candidate.EffectiveFrom)
            .FirstOrDefault();
        amount = rate?.Amount ?? 0m;
        return rate is not null;
    }

    private IReadOnlyList<InvoiceDto> MapInvoices(IReadOnlyList<Invoice> invoices)
    {
        if (invoices.Count == 0)
        {
            return Array.Empty<InvoiceDto>();
        }

        var invoiceIds = invoices.Select(invoice => invoice.Id).ToHashSet();
        var studentIds = invoices.Select(invoice => invoice.StudentId).ToHashSet();
        var roomIds = invoices.Select(invoice => invoice.RoomId).ToHashSet();
        var items = _unitOfWork.Repository<InvoiceItem>().Query()
            .Where(item => invoiceIds.Contains(item.InvoiceId))
            .ToList()
            .GroupBy(item => item.InvoiceId)
            .ToDictionary(group => group.Key, group => group.ToList());
        var students = _unitOfWork.Repository<Student>().Query()
            .Where(student => studentIds.Contains(student.Id))
            .ToDictionary(student => student.Id);
        var rooms = _unitOfWork.Repository<Room>().Query()
            .Where(room => roomIds.Contains(room.Id))
            .ToDictionary(room => room.Id);

        return invoices.Select(invoice =>
        {
            items.TryGetValue(invoice.Id, out var invoiceItems);
            students.TryGetValue(invoice.StudentId, out var student);
            rooms.TryGetValue(invoice.RoomId, out var room);
            return new InvoiceDto
            {
                Id = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                StudentId = invoice.StudentId,
                StudentCode = student?.StudentCode ?? string.Empty,
                StudentName = student?.FullName ?? string.Empty,
                RoomId = invoice.RoomId,
                RoomNumber = room?.RoomNumber ?? string.Empty,
                BillingPeriod = invoice.BillingPeriod,
                InvoiceKind = invoice.InvoiceKind,
                ContractId = invoice.ContractId,
                IssueDate = invoice.IssueDate,
                DueDate = invoice.DueDate,
                TotalAmount = invoice.TotalAmount,
                PaidAmount = invoice.PaidAmount,
                Status = invoice.Status,
                Items = (invoiceItems ?? invoice.Items.ToList()).Select(item => new InvoiceItemDto
                {
                    Id = item.Id,
                    Description = item.Description,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Amount = item.Amount
                }).ToArray()
            };
        }).ToArray();
    }

    private static StudentBillingRowDto MapStudentBillingRow(InvoiceDto invoice)
    {
        var canShowExtendAction = invoice.InvoiceKind == InvoiceKind.MonthlyUtility;
        return new StudentBillingRowDto
        {
            InvoiceId = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            InvoiceKind = invoice.InvoiceKind,
            Name = invoice.StudentName,
            RoomNumber = invoice.RoomNumber,
            BillingPeriod = invoice.BillingPeriod,
            TotalAmount = invoice.TotalAmount,
            PaidAmount = invoice.PaidAmount,
            DueDate = invoice.DueDate,
            Status = invoice.Status,
            CanShowExtendAction = canShowExtendAction,
            CanExtend = canShowExtendAction && invoice.Status != InvoiceStatus.Paid,
            CanPay = true
        };
    }

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

    private void EnsureCanManageRoom(Room room)
    {
        if (_currentUser.IsInRole(RoleNames.BuildingManager))
        {
            var buildingId = _currentUser.CurrentUser?.BuildingId
                ?? throw new InvalidOperationException("Building manager is not assigned to a building.");
            if (room.BuildingId != buildingId)
            {
                throw new InvalidOperationException("Building managers can manage only assigned building rooms.");
            }
        }
    }

    private void EnsureCanViewInvoice(Invoice invoice)
    {
        if (!_currentUser.IsInRole(RoleNames.BuildingManager))
        {
            return;
        }

        var room = _unitOfWork.Repository<Room>().GetByIdAsync(invoice.RoomId).GetAwaiter().GetResult()
            ?? throw new InvalidOperationException("Room was not found.");
        EnsureCanManageRoom(room);
    }

    private static DateTime ParseBillingPeriod(string billingPeriod)
    {
        var parts = billingPeriod.Split('-');
        return new DateTime(int.Parse(parts[0]), int.Parse(parts[1]), 1, 0, 0, 0, DateTimeKind.Utc);
    }

    private static DateTime CalculateMonthlyUtilityDueDate(DateTime periodStart) =>
        new DateTime(periodStart.Year, periodStart.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1).AddDays(4);

    private static string GenerateInvoiceNumber(string billingPeriod, Guid invoiceId) =>
        $"INV-{billingPeriod}-{invoiceId.ToString("N")[..8]}";

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
}
