using DormitoryManagement.Application.DTOs.Billing;
using DormitoryManagement.Application.Services.Billing;
using DormitoryManagement.Domain.Constants;
using DormitoryManagement.Domain.Entities;
using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Application.Tests;

public sealed class BillingServiceTests
{
    [Fact]
    public async Task PreviewUtilityBillingAsync_calculates_room_total_from_readings_and_internet_subscriptions()
    {
        var subscribedStudent = new Student { Id = Guid.NewGuid(), StudentCode = "SV002", FullName = "Tran Thi Binh", UserId = Guid.NewGuid() };
        var unsubscribedStudent = new Student { Id = Guid.NewGuid(), StudentCode = "SV003", FullName = "Le Minh Chau", UserId = Guid.NewGuid() };
        var room = new Room { Id = Guid.NewGuid(), RoomNumber = "A-102", MonthlyPrice = 750000m };
        var unitOfWork = new InMemoryUnitOfWork();
        unitOfWork.Set<Student>().Items.Add(subscribedStudent);
        unitOfWork.Set<Student>().Items.Add(unsubscribedStudent);
        unitOfWork.Set<Room>().Items.Add(room);
        unitOfWork.Set<Contract>().Items.AddRange(new[]
        {
            new Contract
            {
                Id = Guid.NewGuid(),
                StudentId = subscribedStudent.Id,
                RoomId = room.Id,
                StartDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2026, 12, 31),
                IncludesInternet = true,
                Status = ContractStatus.Active
            },
            new Contract
            {
                Id = Guid.NewGuid(),
                StudentId = unsubscribedStudent.Id,
                RoomId = room.Id,
                StartDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2026, 12, 31),
                IncludesInternet = false,
                Status = ContractStatus.Active
            }
        });
        unitOfWork.Set<FeeType>().Items.AddRange(new[] { FeeType("ELECTRICITY"), FeeType("WATER"), FeeType("INTERNET") });
        unitOfWork.Set<FeeRate>().Items.AddRange(unitOfWork.Set<FeeType>().Items.Select(type => new FeeRate
        {
            Id = Guid.NewGuid(),
            FeeTypeId = type.Id,
            Amount = type.Code switch
            {
                "ELECTRICITY" => 3500m,
                "WATER" => 19500m,
                "INTERNET" => 50000m,
                _ => 0m
            },
            EffectiveFrom = new DateTime(2026, 1, 1)
        }));
        unitOfWork.Set<UtilityReading>().Items.Add(new UtilityReading
        {
            Id = Guid.NewGuid(),
            RoomId = room.Id,
            BillingPeriod = "2026-05",
            ElectricityCurrent = 100m,
            WaterCurrent = 10m
        });
        var service = new BillingService(
            new AllowAllPermissionService(),
            unitOfWork,
            new RecordingAuditLogService(),
            new TestCurrentUser(RoleNames.Manager));

        var preview = await InvokePreviewUtilityBillingAsync(service, room.Id, "2026-06", 120m, 13m);

        Assert.Equal(100m, GetPreviewValue<decimal>(preview, "ElectricityPrevious"));
        Assert.Equal(20m, GetPreviewValue<decimal>(preview, "ElectricityConsumption"));
        Assert.Equal(70000m, GetPreviewValue<decimal>(preview, "ElectricityAmount"));
        Assert.Equal(10m, GetPreviewValue<decimal>(preview, "WaterPrevious"));
        Assert.Equal(3m, GetPreviewValue<decimal>(preview, "WaterConsumption"));
        Assert.Equal(58500m, GetPreviewValue<decimal>(preview, "WaterAmount"));
        Assert.Equal(2, GetPreviewValue<int>(preview, "ActiveMemberCount"));
        Assert.Equal(2, GetPreviewValue<int>(preview, "InternetSubscriberCount"));
        Assert.Equal(100000m, GetPreviewValue<decimal>(preview, "InternetAmount"));
        Assert.Equal(228500m, GetPreviewValue<decimal>(preview, "TotalAmount"));
        Assert.Equal(114250m, GetPreviewValue<decimal>(preview, "PerMemberAmount"));
        Assert.Equal(new DateTime(2026, 7, 5, 0, 0, 0, DateTimeKind.Utc), GetPreviewValue<DateTime>(preview, "DueDate"));
    }

    [Fact]
    public async Task PreviewUtilityBillingAsync_rejects_current_readings_below_previous_values()
    {
        var room = new Room { Id = Guid.NewGuid(), RoomNumber = "A-102" };
        var unitOfWork = new InMemoryUnitOfWork();
        unitOfWork.Set<Room>().Items.Add(room);
        unitOfWork.Set<UtilityReading>().Items.Add(new UtilityReading
        {
            Id = Guid.NewGuid(),
            RoomId = room.Id,
            BillingPeriod = "2026-05",
            ElectricityCurrent = 100m,
            WaterCurrent = 10m
        });
        var service = new BillingService(
            new AllowAllPermissionService(),
            unitOfWork,
            new RecordingAuditLogService(),
            new TestCurrentUser(RoleNames.Manager));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            InvokePreviewUtilityBillingAsync(service, room.Id, "2026-06", 99m, 13m));
    }

    [Fact]
    public async Task PreviewUtilityBillingAsync_defaults_first_room_baselines_to_zero()
    {
        var room = new Room { Id = Guid.NewGuid(), RoomNumber = "A-102" };
        var unitOfWork = new InMemoryUnitOfWork();
        unitOfWork.Set<Room>().Items.Add(room);
        var service = new BillingService(
            new AllowAllPermissionService(),
            unitOfWork,
            new RecordingAuditLogService(),
            new TestCurrentUser(RoleNames.Manager));

        var preview = await InvokePreviewUtilityBillingAsync(service, room.Id, "2026-06", 100m, 12m);

        Assert.Equal(0m, GetPreviewValue<decimal>(preview, "ElectricityPrevious"));
        Assert.Equal(100m, GetPreviewValue<decimal>(preview, "ElectricityConsumption"));
        Assert.Equal(350000m, GetPreviewValue<decimal>(preview, "ElectricityAmount"));
        Assert.Equal(0m, GetPreviewValue<decimal>(preview, "WaterPrevious"));
        Assert.Equal(12m, GetPreviewValue<decimal>(preview, "WaterConsumption"));
        Assert.Equal(234000m, GetPreviewValue<decimal>(preview, "WaterAmount"));
        Assert.Equal(584000m, GetPreviewValue<decimal>(preview, "TotalAmount"));
    }

    [Fact]
    public async Task GenerateMonthlyInvoicesAsync_creates_utility_only_invoices_split_by_active_room_occupants()
    {
        var student = new Student { Id = Guid.NewGuid(), StudentCode = "SV002", FullName = "Tran Thi Binh", UserId = Guid.NewGuid() };
        var roommate = new Student { Id = Guid.NewGuid(), StudentCode = "SV003", FullName = "Le Minh Chau", UserId = Guid.NewGuid() };
        var room = new Room { Id = Guid.NewGuid(), RoomNumber = "A-102", MonthlyPrice = 750000m };
        var contract = new Contract
        {
            Id = Guid.NewGuid(),
            StudentId = student.Id,
            RoomId = room.Id,
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 12, 31),
            MonthlyFee = 750000m,
            IncludesInternet = true,
            Status = ContractStatus.Active
        };
        var roommateContract = new Contract
        {
            Id = Guid.NewGuid(),
            StudentId = roommate.Id,
            RoomId = room.Id,
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 12, 31),
            MonthlyFee = 750000m,
            IncludesInternet = false,
            Status = ContractStatus.Active
        };
        var unitOfWork = new InMemoryUnitOfWork();
        unitOfWork.Set<Student>().Items.Add(student);
        unitOfWork.Set<Student>().Items.Add(roommate);
        unitOfWork.Set<Room>().Items.Add(room);
        unitOfWork.Set<Contract>().Items.Add(contract);
        unitOfWork.Set<Contract>().Items.Add(roommateContract);
        unitOfWork.Set<FeeType>().Items.AddRange(new[]
        {
            FeeType("ROOM_FEE"),
            FeeType("INTERNET"),
            FeeType("ELECTRICITY"),
            FeeType("WATER"),
            FeeType("PARKING")
        });
        unitOfWork.Set<FeeRate>().Items.AddRange(unitOfWork.Set<FeeType>().Items.Select(type => new FeeRate
        {
            Id = Guid.NewGuid(),
            FeeTypeId = type.Id,
            Amount = type.Code switch
            {
                "ROOM_FEE" => 750000m,
                "INTERNET" => 50000m,
                "ELECTRICITY" => 3500m,
                "WATER" => 19500m,
                "PARKING" => 100000m,
                _ => 0m
            },
            EffectiveFrom = new DateTime(2026, 1, 1)
        }));
        unitOfWork.Set<UtilityReading>().Items.Add(new UtilityReading
        {
            Id = Guid.NewGuid(),
            RoomId = room.Id,
            BillingPeriod = "2026-06",
            ElectricityPrevious = 100m,
            ElectricityCurrent = 120m,
            WaterPrevious = 10m,
            WaterCurrent = 13m
        });
        var audit = new RecordingAuditLogService();
        var service = new BillingService(
            new AllowAllPermissionService(),
            unitOfWork,
            audit,
            new TestCurrentUser(RoleNames.Manager));

        var result = await service.GenerateMonthlyInvoicesAsync(new GenerateMonthlyInvoiceRequest
        {
            BillingPeriod = "2026-06",
            DueDate = new DateTime(2026, 6, 15)
        });

        Assert.Equal(2, result.CreatedCount);
        Assert.Equal(0, result.SkippedCount);
        Assert.Equal(0, result.MissingUtilityReadingCount);
        var invoice = Assert.Single(unitOfWork.Set<Invoice>().Items, candidate => candidate.StudentId == student.Id);
        var roommateInvoice = Assert.Single(unitOfWork.Set<Invoice>().Items, candidate => candidate.StudentId == roommate.Id);
        Assert.Equal(student.Id, invoice.StudentId);
        Assert.Equal(room.Id, invoice.RoomId);
        Assert.Equal("2026-06", invoice.BillingPeriod);
        Assert.Equal(InvoiceKind.MonthlyUtility, invoice.InvoiceKind);
        Assert.Equal(new DateTime(2026, 7, 5, 0, 0, 0, DateTimeKind.Utc), invoice.DueDate);
        Assert.Equal(114250m, invoice.TotalAmount);
        Assert.Equal(InvoiceStatus.Unpaid, invoice.Status);
        Assert.Equal(3, invoice.Items.Count);
        Assert.Contains(invoice.Items, item => item.Description == "Electricity 2026-06" && item.Quantity == 10m && item.Amount == 35000m);
        Assert.Contains(invoice.Items, item => item.Description == "Water 2026-06" && item.Quantity == 1.5m && item.Amount == 29250m);
        Assert.Contains(invoice.Items, item => item.Description == "Internet 2026-06" && item.Quantity == 1m && item.Amount == 50000m);
        Assert.Equal(114250m, roommateInvoice.TotalAmount);
        Assert.Contains(roommateInvoice.Items, item => item.Description == "Internet 2026-06" && item.Quantity == 1m && item.Amount == 50000m);
        Assert.DoesNotContain(invoice.Items, item => item.Description.StartsWith("Room fee", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(invoice.Items, item => item.Description.StartsWith("Parking", StringComparison.OrdinalIgnoreCase));
        Assert.True(unitOfWork.LastTransaction?.Committed);
        Assert.Contains(audit.Entries, entry => entry.Action == "Invoice.GeneratedMonthly");
    }

    [Fact]
    public async Task GenerateMonthlyInvoicesAsync_skips_existing_monthly_utility_invoice_for_same_student_room_period()
    {
        var student = new Student { Id = Guid.NewGuid(), StudentCode = "SV002", FullName = "Tran Thi Binh", UserId = Guid.NewGuid() };
        var room = new Room { Id = Guid.NewGuid(), RoomNumber = "A-102", MonthlyPrice = 750000m };
        var existingInvoice = new Invoice
        {
            Id = Guid.NewGuid(),
            InvoiceNumber = "INV-2026-06-EXIST",
            StudentId = student.Id,
            RoomId = room.Id,
            BillingPeriod = "2026-06",
            InvoiceKind = InvoiceKind.MonthlyUtility,
            IssueDate = new DateTime(2026, 6, 1),
            DueDate = new DateTime(2026, 6, 10),
            TotalAmount = 100000m,
            PaidAmount = 0m,
            Status = InvoiceStatus.Unpaid
        };
        var unitOfWork = new InMemoryUnitOfWork();
        unitOfWork.Set<Student>().Items.Add(student);
        unitOfWork.Set<Room>().Items.Add(room);
        unitOfWork.Set<Contract>().Items.Add(new Contract
        {
            Id = Guid.NewGuid(),
            StudentId = student.Id,
            RoomId = room.Id,
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 12, 31),
            IncludesInternet = true,
            Status = ContractStatus.Active
        });
        unitOfWork.Set<Invoice>().Items.Add(existingInvoice);
        unitOfWork.Set<FeeType>().Items.AddRange(new[] { FeeType("ELECTRICITY"), FeeType("WATER"), FeeType("INTERNET") });
        unitOfWork.Set<FeeRate>().Items.AddRange(unitOfWork.Set<FeeType>().Items.Select(type => new FeeRate
        {
            Id = Guid.NewGuid(),
            FeeTypeId = type.Id,
            Amount = type.Code switch
            {
                "ELECTRICITY" => 3500m,
                "WATER" => 19500m,
                "INTERNET" => 50000m,
                _ => 0m
            },
            EffectiveFrom = new DateTime(2026, 1, 1)
        }));
        unitOfWork.Set<UtilityReading>().Items.Add(new UtilityReading
        {
            Id = Guid.NewGuid(),
            RoomId = room.Id,
            BillingPeriod = "2026-06",
            ElectricityPrevious = 100m,
            ElectricityCurrent = 120m,
            WaterPrevious = 10m,
            WaterCurrent = 13m
        });
        var service = new BillingService(
            new AllowAllPermissionService(),
            unitOfWork,
            new RecordingAuditLogService(),
            new TestCurrentUser(RoleNames.Manager));

        var result = await service.GenerateMonthlyInvoicesAsync(new GenerateMonthlyInvoiceRequest
        {
            BillingPeriod = "2026-06",
            DueDate = new DateTime(2026, 6, 15)
        });

        Assert.Equal(0, result.CreatedCount);
        Assert.Equal(1, result.SkippedCount);
        Assert.Equal(0, result.MissingUtilityReadingCount);
        Assert.Single(unitOfWork.Set<Invoice>().Items);
        Assert.Same(existingInvoice, unitOfWork.Set<Invoice>().Items.Single());
    }

    [Fact]
    public async Task GenerateMonthlyInvoicesAsync_skips_room_when_utility_reading_is_missing()
    {
        var student = new Student { Id = Guid.NewGuid(), StudentCode = "SV002", FullName = "Tran Thi Binh" };
        var room = new Room { Id = Guid.NewGuid(), RoomNumber = "A-102", MonthlyPrice = 750000m };
        var unitOfWork = new InMemoryUnitOfWork();
        unitOfWork.Set<Student>().Items.Add(student);
        unitOfWork.Set<Room>().Items.Add(room);
        unitOfWork.Set<Contract>().Items.Add(new Contract
        {
            Id = Guid.NewGuid(),
            StudentId = student.Id,
            RoomId = room.Id,
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 12, 31),
            Status = ContractStatus.Active
        });
        unitOfWork.Set<FeeType>().Items.AddRange(new[] { FeeType("ELECTRICITY"), FeeType("WATER"), FeeType("INTERNET") });
        unitOfWork.Set<FeeRate>().Items.AddRange(unitOfWork.Set<FeeType>().Items.Select(type => new FeeRate
        {
            Id = Guid.NewGuid(),
            FeeTypeId = type.Id,
            Amount = type.Code == "WATER" ? 19500m : type.Code == "ELECTRICITY" ? 3500m : 50000m,
            EffectiveFrom = new DateTime(2026, 1, 1)
        }));
        var service = new BillingService(
            new AllowAllPermissionService(),
            unitOfWork,
            new RecordingAuditLogService(),
            new TestCurrentUser(RoleNames.Manager));

        var result = await service.GenerateMonthlyInvoicesAsync(new GenerateMonthlyInvoiceRequest
        {
            BillingPeriod = "2026-06",
            DueDate = new DateTime(2026, 6, 15)
        });

        Assert.Equal(0, result.CreatedCount);
        Assert.Equal(1, result.MissingUtilityReadingCount);
        Assert.Empty(unitOfWork.Set<Invoice>().Items);
        Assert.Contains(result.Warnings, warning => warning.Contains(room.RoomNumber, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task UpsertUtilityReadingAsync_uses_previous_period_current_values()
    {
        var room = new Room { Id = Guid.NewGuid(), RoomNumber = "A-102", BuildingId = Guid.NewGuid() };
        var unitOfWork = new InMemoryUnitOfWork();
        unitOfWork.Set<Room>().Items.Add(room);
        unitOfWork.Set<UtilityReading>().Items.Add(new UtilityReading
        {
            Id = Guid.NewGuid(),
            RoomId = room.Id,
            BillingPeriod = "2026-05",
            ElectricityCurrent = 120m,
            WaterCurrent = 13m
        });
        var service = new BillingService(
            new AllowAllPermissionService(),
            unitOfWork,
            new RecordingAuditLogService(),
            new TestCurrentUser(RoleNames.Manager, buildingId: room.BuildingId));

        await service.UpsertUtilityReadingAsync(new UtilityReadingRequest
        {
            RoomId = room.Id,
            BillingPeriod = "2026-06",
            ElectricityCurrent = 140m,
            WaterCurrent = 16m
        });

        var reading = Assert.Single(unitOfWork.Set<UtilityReading>().Items, item => item.BillingPeriod == "2026-06");
        Assert.Equal(120m, reading.ElectricityPrevious);
        Assert.Equal(13m, reading.WaterPrevious);
    }

    [Fact]
    public async Task UpsertUtilityReadingAsync_defaults_first_room_baselines_to_zero()
    {
        var room = new Room { Id = Guid.NewGuid(), RoomNumber = "A-102", BuildingId = Guid.NewGuid() };
        var unitOfWork = new InMemoryUnitOfWork();
        unitOfWork.Set<Room>().Items.Add(room);
        var service = new BillingService(
            new AllowAllPermissionService(),
            unitOfWork,
            new RecordingAuditLogService(),
            new TestCurrentUser(RoleNames.Manager, buildingId: room.BuildingId));

        await service.UpsertUtilityReadingAsync(new UtilityReadingRequest
        {
            RoomId = room.Id,
            BillingPeriod = "2026-06",
            ElectricityCurrent = 100m,
            WaterCurrent = 12m
        });

        var reading = Assert.Single(unitOfWork.Set<UtilityReading>().Items);
        Assert.Equal(0m, reading.ElectricityPrevious);
        Assert.Equal(100m, reading.ElectricityCurrent);
        Assert.Equal(0m, reading.WaterPrevious);
        Assert.Equal(12m, reading.WaterCurrent);
    }

    [Fact]
    public async Task UpsertUtilityReadingAsync_as_building_scoped_manager_rejects_room_outside_assigned_building()
    {
        var assignedBuildingId = Guid.NewGuid();
        var otherRoom = new Room { Id = Guid.NewGuid(), RoomNumber = "B-201", BuildingId = Guid.NewGuid() };
        var unitOfWork = new InMemoryUnitOfWork();
        unitOfWork.Set<Room>().Items.Add(otherRoom);
        var service = new BillingService(
            new AllowAllPermissionService(),
            unitOfWork,
            new RecordingAuditLogService(),
            new TestCurrentUser(RoleNames.Manager, buildingId: assignedBuildingId));

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpsertUtilityReadingAsync(new UtilityReadingRequest
        {
            RoomId = otherRoom.Id,
            BillingPeriod = "2026-06",
            ElectricityPrevious = 100m,
            ElectricityCurrent = 120m,
            WaterPrevious = 10m,
            WaterCurrent = 13m
        }));

        Assert.Empty(unitOfWork.Set<UtilityReading>().Items);
    }

    [Fact]
    public async Task GetInvoiceAsync_as_student_rejects_other_student_invoice()
    {
        var currentStudentId = Guid.NewGuid();
        var otherStudent = new Student { Id = Guid.NewGuid(), StudentCode = "SV009", FullName = "Other Student" };
        var room = new Room { Id = Guid.NewGuid(), RoomNumber = "A-102" };
        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            InvoiceNumber = "INV-2026-06-OTHER",
            StudentId = otherStudent.Id,
            RoomId = room.Id,
            BillingPeriod = "2026-06",
            InvoiceKind = InvoiceKind.MonthlyUtility,
            IssueDate = new DateTime(2026, 6, 1),
            DueDate = new DateTime(2026, 6, 10),
            TotalAmount = 100000m,
            PaidAmount = 0m,
            Status = InvoiceStatus.Unpaid
        };
        var unitOfWork = new InMemoryUnitOfWork();
        unitOfWork.Set<Student>().Items.Add(otherStudent);
        unitOfWork.Set<Room>().Items.Add(room);
        unitOfWork.Set<Invoice>().Items.Add(invoice);
        var service = new BillingService(
            new AllowAllPermissionService(),
            unitOfWork,
            new RecordingAuditLogService(),
            new TestCurrentUser(RoleNames.Student, studentId: currentStudentId));

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetInvoiceAsync(invoice.Id));
    }

    [Fact]
    public async Task GetInvoicesAsync_as_building_scoped_manager_returns_only_assigned_building_invoices()
    {
        var assignedBuildingId = Guid.NewGuid();
        var assignedStudent = new Student { Id = Guid.NewGuid(), StudentCode = "SV001", FullName = "Assigned Student" };
        var otherStudent = new Student { Id = Guid.NewGuid(), StudentCode = "SV002", FullName = "Other Student" };
        var assignedRoom = new Room { Id = Guid.NewGuid(), RoomNumber = "A-101", BuildingId = assignedBuildingId };
        var otherRoom = new Room { Id = Guid.NewGuid(), RoomNumber = "B-201", BuildingId = Guid.NewGuid() };
        var assignedInvoice = new Invoice
        {
            Id = Guid.NewGuid(),
            InvoiceNumber = "INV-ASSIGNED",
            StudentId = assignedStudent.Id,
            RoomId = assignedRoom.Id,
            BillingPeriod = "2026-06",
            InvoiceKind = InvoiceKind.MonthlyUtility,
            IssueDate = new DateTime(2026, 6, 1),
            DueDate = new DateTime(2026, 6, 10),
            TotalAmount = 100000m,
            PaidAmount = 0m,
            Status = InvoiceStatus.Unpaid
        };
        var otherInvoice = new Invoice
        {
            Id = Guid.NewGuid(),
            InvoiceNumber = "INV-OTHER",
            StudentId = otherStudent.Id,
            RoomId = otherRoom.Id,
            BillingPeriod = "2026-06",
            InvoiceKind = InvoiceKind.MonthlyUtility,
            IssueDate = new DateTime(2026, 6, 1),
            DueDate = new DateTime(2026, 6, 10),
            TotalAmount = 100000m,
            PaidAmount = 0m,
            Status = InvoiceStatus.Unpaid
        };
        var unitOfWork = new InMemoryUnitOfWork();
        unitOfWork.Set<Student>().Items.AddRange(new[] { assignedStudent, otherStudent });
        unitOfWork.Set<Room>().Items.AddRange(new[] { assignedRoom, otherRoom });
        unitOfWork.Set<Invoice>().Items.AddRange(new[] { assignedInvoice, otherInvoice });
        var service = new BillingService(
            new AllowAllPermissionService(),
            unitOfWork,
            new RecordingAuditLogService(),
            new TestCurrentUser(RoleNames.Manager, buildingId: assignedBuildingId));

        var invoices = await service.GetInvoicesAsync();

        var invoice = Assert.Single(invoices);
        Assert.Equal(assignedInvoice.Id, invoice.Id);
    }

    [Fact]
    public async Task GetStudentBillingRowsAsync_as_student_returns_own_rows_with_action_state()
    {
        var currentStudent = new Student { Id = Guid.NewGuid(), StudentCode = "SV001", FullName = "Nguyen Van An" };
        var otherStudent = new Student { Id = Guid.NewGuid(), StudentCode = "SV002", FullName = "Other Student" };
        var room = new Room { Id = Guid.NewGuid(), RoomNumber = "A-101" };
        var otherRoom = new Room { Id = Guid.NewGuid(), RoomNumber = "B-201" };
        var unpaidUtility = Invoice("INV-UTILITY-UNPAID", currentStudent.Id, room.Id, InvoiceKind.MonthlyUtility, InvoiceStatus.Unpaid, 178500m, 0m);
        var paidUtility = Invoice("INV-UTILITY-PAID", currentStudent.Id, room.Id, InvoiceKind.MonthlyUtility, InvoiceStatus.Paid, 178500m, 178500m);
        var vehicleParking = Invoice("INV-PARK", currentStudent.Id, room.Id, InvoiceKind.VehicleParking, InvoiceStatus.Unpaid, 50000m, 0m);
        var otherStudentInvoice = Invoice("INV-OTHER", otherStudent.Id, otherRoom.Id, InvoiceKind.MonthlyUtility, InvoiceStatus.Unpaid, 100000m, 0m);
        var unitOfWork = new InMemoryUnitOfWork();
        unitOfWork.Set<Student>().Items.AddRange(new[] { currentStudent, otherStudent });
        unitOfWork.Set<Room>().Items.AddRange(new[] { room, otherRoom });
        unitOfWork.Set<Invoice>().Items.AddRange(new[] { unpaidUtility, paidUtility, vehicleParking, otherStudentInvoice });
        var service = new BillingService(
            new AllowAllPermissionService(),
            unitOfWork,
            new RecordingAuditLogService(),
            new TestCurrentUser(RoleNames.Student, studentId: currentStudent.Id));

        var rows = await service.GetStudentBillingRowsAsync();

        Assert.Equal(3, rows.Count);
        Assert.DoesNotContain(rows, row => row.InvoiceNumber == otherStudentInvoice.InvoiceNumber);
        var unpaidUtilityRow = rows.Single(row => row.InvoiceNumber == unpaidUtility.InvoiceNumber);
        Assert.Equal(InvoiceKind.MonthlyUtility, unpaidUtilityRow.InvoiceKind);
        Assert.Equal(currentStudent.FullName, unpaidUtilityRow.Name);
        Assert.Equal(room.RoomNumber, unpaidUtilityRow.RoomNumber);
        Assert.Equal(unpaidUtility.BillingPeriod, unpaidUtilityRow.BillingPeriod);
        Assert.Equal(unpaidUtility.TotalAmount, unpaidUtilityRow.TotalAmount);
        Assert.Equal(unpaidUtility.PaidAmount, unpaidUtilityRow.PaidAmount);
        Assert.Equal(unpaidUtility.DueDate, unpaidUtilityRow.DueDate);
        Assert.Equal(unpaidUtility.Status, unpaidUtilityRow.Status);
        Assert.True(unpaidUtilityRow.CanShowExtendAction);
        Assert.True(unpaidUtilityRow.CanExtend);
        Assert.True(unpaidUtilityRow.CanPay);

        var paidUtilityRow = rows.Single(row => row.InvoiceNumber == paidUtility.InvoiceNumber);
        Assert.True(paidUtilityRow.CanShowExtendAction);
        Assert.False(paidUtilityRow.CanExtend);
        Assert.True(paidUtilityRow.CanPay);

        var vehicleParkingRow = rows.Single(row => row.InvoiceNumber == vehicleParking.InvoiceNumber);
        Assert.False(vehicleParkingRow.CanShowExtendAction);
        Assert.False(vehicleParkingRow.CanExtend);
        Assert.True(vehicleParkingRow.CanPay);
    }

    [Fact]
    public async Task GetInvoicesAsync_marks_unpaid_past_due_vehicle_invoice_overdue()
    {
        var student = new Student { Id = Guid.NewGuid(), StudentCode = "SV001", FullName = "Nguyen Van An" };
        var room = new Room { Id = Guid.NewGuid(), RoomNumber = "A-101" };
        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            InvoiceNumber = "INV-PARK-202605-ABC",
            StudentId = student.Id,
            RoomId = room.Id,
            BillingPeriod = DateTime.UtcNow.ToString("yyyy-MM"),
            InvoiceKind = InvoiceKind.VehicleParking,
            IssueDate = DateTime.UtcNow.Date.AddDays(-3),
            DueDate = DateTime.UtcNow.Date.AddDays(-1),
            TotalAmount = 40000m,
            PaidAmount = 0m,
            Status = InvoiceStatus.Unpaid
        };
        var unitOfWork = new InMemoryUnitOfWork();
        unitOfWork.Set<Student>().Items.Add(student);
        unitOfWork.Set<Room>().Items.Add(room);
        unitOfWork.Set<Invoice>().Items.Add(invoice);
        var service = new BillingService(
            new AllowAllPermissionService(),
            unitOfWork,
            new RecordingAuditLogService(),
            new TestCurrentUser(RoleNames.Manager));

        var invoices = await service.GetInvoicesAsync();

        var dto = Assert.Single(invoices);
        Assert.Equal(InvoiceStatus.Overdue, dto.Status);
        Assert.Equal(InvoiceStatus.Overdue, invoice.Status);
        Assert.Equal(1, unitOfWork.SaveChangesCount);
    }

    private static FeeType FeeType(string code) =>
        new()
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = code,
            IsRecurring = true
        };

    private static Invoice Invoice(
        string invoiceNumber,
        Guid studentId,
        Guid roomId,
        InvoiceKind kind,
        InvoiceStatus status,
        decimal total,
        decimal paid) =>
        new()
        {
            Id = Guid.NewGuid(),
            InvoiceNumber = invoiceNumber,
            StudentId = studentId,
            RoomId = roomId,
            BillingPeriod = "2026-06",
            InvoiceKind = kind,
            IssueDate = new DateTime(2026, 6, 1),
            DueDate = new DateTime(2026, 6, 10),
            TotalAmount = total,
            PaidAmount = paid,
            Status = status
        };

    private static async Task<object> InvokePreviewUtilityBillingAsync(
        BillingService service,
        Guid roomId,
        string billingPeriod,
        decimal electricityCurrent,
        decimal waterCurrent)
    {
        var requestType = Type.GetType("DormitoryManagement.Application.DTOs.Billing.UtilityBillingPreviewRequest, DormitoryManagement.Application");
        Assert.NotNull(requestType);
        var request = Activator.CreateInstance(requestType)!;
        SetProperty(request, "RoomId", roomId);
        SetProperty(request, "BillingPeriod", billingPeriod);
        SetProperty(request, "ElectricityCurrent", electricityCurrent);
        SetProperty(request, "WaterCurrent", waterCurrent);
        var method = typeof(BillingService).GetMethod("PreviewUtilityBillingAsync", new[] { requestType, typeof(CancellationToken) })
            ?? typeof(BillingService).GetMethod("PreviewUtilityBillingAsync", new[] { requestType });
        Assert.NotNull(method);
        var parameters = method.GetParameters().Length == 2
            ? new object?[] { request, CancellationToken.None }
            : new object?[] { request };
        var task = Assert.IsAssignableFrom<Task>(method.Invoke(service, parameters));
        await task;
        var resultProperty = task.GetType().GetProperty("Result");
        Assert.NotNull(resultProperty);
        return resultProperty.GetValue(task)!;
    }

    private static void SetProperty(object target, string propertyName, object value)
    {
        var property = target.GetType().GetProperty(propertyName);
        Assert.NotNull(property);
        property.SetValue(target, value);
    }

    private static T GetPreviewValue<T>(object preview, string propertyName)
    {
        var property = preview.GetType().GetProperty(propertyName);
        Assert.NotNull(property);
        return Assert.IsType<T>(property.GetValue(preview));
    }
}

