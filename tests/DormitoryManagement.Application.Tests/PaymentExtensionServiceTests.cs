using DormitoryManagement.Application.DTOs.Payments;
using DormitoryManagement.Application.Services.Payments;
using DormitoryManagement.Domain.Constants;
using DormitoryManagement.Domain.Entities;
using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Application.Tests;

public sealed class PaymentExtensionServiceTests
{
    [Fact]
    public async Task RequestExtensionAsync_allows_student_to_request_own_monthly_utility_invoice()
    {
        var student = new Student { Id = Guid.NewGuid(), StudentCode = "SV001", FullName = "Nguyen Van An" };
        var invoice = MonthlyInvoice(student.Id, new DateTime(2026, 7, 5));
        var unitOfWork = new InMemoryUnitOfWork();
        unitOfWork.Set<Student>().Items.Add(student);
        unitOfWork.Set<Invoice>().Items.Add(invoice);
        var audit = new RecordingAuditLogService();
        var service = new PaymentExtensionService(
            new AllowAllPermissionService(),
            unitOfWork,
            audit,
            new TestCurrentUser(RoleNames.Student, studentId: student.Id));

        var dto = await service.RequestExtensionAsync(new CreatePaymentExtensionRequest
        {
            InvoiceId = invoice.Id,
            RequestedDueDate = new DateTime(2026, 7, 10),
            Reason = "Need a few more days"
        });

        var extension = Assert.Single(unitOfWork.Set<PaymentExtension>().Items);
        Assert.Equal(extension.Id, dto.Id);
        Assert.Equal(PaymentExtensionStatus.Pending, extension.Status);
        Assert.Equal(invoice.Id, extension.InvoiceId);
        Assert.Equal(student.Id, extension.StudentId);
        Assert.Equal(new DateTime(2026, 7, 10), extension.RequestedDueDate);
        Assert.Contains(audit.Entries, entry => entry.Action == "PaymentExtension.Requested");
    }

    [Fact]
    public async Task RequestExtensionAsync_rejects_paid_monthly_utility_invoice()
    {
        var student = new Student { Id = Guid.NewGuid(), StudentCode = "SV001", FullName = "Nguyen Van An" };
        var invoice = MonthlyInvoice(student.Id, new DateTime(2026, 7, 5));
        invoice.Status = InvoiceStatus.Paid;
        invoice.PaidAmount = invoice.TotalAmount;
        var unitOfWork = new InMemoryUnitOfWork();
        unitOfWork.Set<Student>().Items.Add(student);
        unitOfWork.Set<Invoice>().Items.Add(invoice);
        var service = new PaymentExtensionService(
            new AllowAllPermissionService(),
            unitOfWork,
            new RecordingAuditLogService(),
            new TestCurrentUser(RoleNames.Student, studentId: student.Id));

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.RequestExtensionAsync(new CreatePaymentExtensionRequest
        {
            InvoiceId = invoice.Id,
            RequestedDueDate = new DateTime(2026, 7, 10),
            Reason = "Need a few more days"
        }));

        Assert.Empty(unitOfWork.Set<PaymentExtension>().Items);
    }

    [Fact]
    public async Task RequestExtensionAsync_rejects_vehicle_parking_invoice()
    {
        var student = new Student { Id = Guid.NewGuid(), StudentCode = "SV001", FullName = "Nguyen Van An" };
        var invoice = MonthlyInvoice(student.Id, new DateTime(2026, 7, 5));
        invoice.InvoiceKind = InvoiceKind.VehicleParking;
        var unitOfWork = new InMemoryUnitOfWork();
        unitOfWork.Set<Student>().Items.Add(student);
        unitOfWork.Set<Invoice>().Items.Add(invoice);
        var service = new PaymentExtensionService(
            new AllowAllPermissionService(),
            unitOfWork,
            new RecordingAuditLogService(),
            new TestCurrentUser(RoleNames.Student, studentId: student.Id));

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.RequestExtensionAsync(new CreatePaymentExtensionRequest
        {
            InvoiceId = invoice.Id,
            RequestedDueDate = new DateTime(2026, 7, 10),
            Reason = "Need a few more days"
        }));

        Assert.Empty(unitOfWork.Set<PaymentExtension>().Items);
    }

    [Fact]
    public async Task RequestExtensionAsync_rejects_invoice_owned_by_another_student()
    {
        var currentStudent = new Student { Id = Guid.NewGuid(), StudentCode = "SV001", FullName = "Nguyen Van An" };
        var otherStudent = new Student { Id = Guid.NewGuid(), StudentCode = "SV002", FullName = "Tran Thi Binh" };
        var invoice = MonthlyInvoice(otherStudent.Id, new DateTime(2026, 7, 5));
        var unitOfWork = new InMemoryUnitOfWork();
        unitOfWork.Set<Student>().Items.Add(currentStudent);
        unitOfWork.Set<Student>().Items.Add(otherStudent);
        unitOfWork.Set<Invoice>().Items.Add(invoice);
        var service = new PaymentExtensionService(
            new AllowAllPermissionService(),
            unitOfWork,
            new RecordingAuditLogService(),
            new TestCurrentUser(RoleNames.Student, studentId: currentStudent.Id));

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.RequestExtensionAsync(new CreatePaymentExtensionRequest
        {
            InvoiceId = invoice.Id,
            RequestedDueDate = new DateTime(2026, 7, 10),
            Reason = "Need a few more days"
        }));

        Assert.Empty(unitOfWork.Set<PaymentExtension>().Items);
    }

    [Fact]
    public async Task RequestExtensionAsync_rejects_requested_due_date_more_than_five_days_after_current_due_date()
    {
        var student = new Student { Id = Guid.NewGuid(), StudentCode = "SV001", FullName = "Nguyen Van An" };
        var invoice = MonthlyInvoice(student.Id, new DateTime(2026, 7, 5));
        var unitOfWork = new InMemoryUnitOfWork();
        unitOfWork.Set<Student>().Items.Add(student);
        unitOfWork.Set<Invoice>().Items.Add(invoice);
        var service = new PaymentExtensionService(
            new AllowAllPermissionService(),
            unitOfWork,
            new RecordingAuditLogService(),
            new TestCurrentUser(RoleNames.Student, studentId: student.Id));

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.RequestExtensionAsync(new CreatePaymentExtensionRequest
        {
            InvoiceId = invoice.Id,
            RequestedDueDate = new DateTime(2026, 7, 11),
            Reason = "Need a few more days"
        }));

        Assert.Empty(unitOfWork.Set<PaymentExtension>().Items);
    }

    [Fact]
    public async Task RequestExtensionAsync_rejects_requested_due_date_after_day_15()
    {
        var student = new Student { Id = Guid.NewGuid(), StudentCode = "SV001", FullName = "Nguyen Van An" };
        var invoice = MonthlyInvoice(student.Id, new DateTime(2026, 7, 12));
        var unitOfWork = new InMemoryUnitOfWork();
        unitOfWork.Set<Student>().Items.Add(student);
        unitOfWork.Set<Invoice>().Items.Add(invoice);
        var service = new PaymentExtensionService(
            new AllowAllPermissionService(),
            unitOfWork,
            new RecordingAuditLogService(),
            new TestCurrentUser(RoleNames.Student, studentId: student.Id));

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.RequestExtensionAsync(new CreatePaymentExtensionRequest
        {
            InvoiceId = invoice.Id,
            RequestedDueDate = new DateTime(2026, 7, 16),
            Reason = "Need a few more days"
        }));

        Assert.Empty(unitOfWork.Set<PaymentExtension>().Items);
    }

    [Fact]
    public async Task ApproveExtensionAsync_caps_due_date_at_day_15()
    {
        var student = new Student { Id = Guid.NewGuid(), StudentCode = "SV001", FullName = "Nguyen Van An" };
        var room = new Room { Id = Guid.NewGuid(), BuildingId = Guid.NewGuid(), RoomNumber = "A-101" };
        var invoice = MonthlyInvoice(student.Id, new DateTime(2026, 7, 10));
        invoice.RoomId = room.Id;
        var extension = new PaymentExtension
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoice.Id,
            StudentId = student.Id,
            RequestedDueDate = new DateTime(2026, 7, 20),
            Reason = "Need a few more days",
            Status = PaymentExtensionStatus.Pending
        };
        var unitOfWork = new InMemoryUnitOfWork();
        unitOfWork.Set<Student>().Items.Add(student);
        unitOfWork.Set<Room>().Items.Add(room);
        unitOfWork.Set<Invoice>().Items.Add(invoice);
        unitOfWork.Set<PaymentExtension>().Items.Add(extension);
        var audit = new RecordingAuditLogService();
        var service = new PaymentExtensionService(
            new AllowAllPermissionService(),
            unitOfWork,
            audit,
            new TestCurrentUser(RoleNames.BuildingManager, buildingId: room.BuildingId));

        await service.ApproveExtensionAsync(extension.Id);

        Assert.Equal(PaymentExtensionStatus.Approved, extension.Status);
        Assert.Equal(new DateTime(2026, 7, 15), invoice.DueDate);
        Assert.Contains(audit.Entries, entry => entry.Action == "PaymentExtension.Approved");
    }

    [Fact]
    public async Task ApproveExtensionAsync_as_building_manager_rejects_invoice_outside_assigned_building()
    {
        var student = new Student { Id = Guid.NewGuid(), StudentCode = "SV001", FullName = "Nguyen Van An" };
        var room = new Room { Id = Guid.NewGuid(), BuildingId = Guid.NewGuid(), RoomNumber = "B-201" };
        var invoice = MonthlyInvoice(student.Id, new DateTime(2026, 7, 5));
        invoice.RoomId = room.Id;
        var extension = new PaymentExtension
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoice.Id,
            StudentId = student.Id,
            RequestedDueDate = new DateTime(2026, 7, 10),
            Reason = "Need a few more days",
            Status = PaymentExtensionStatus.Pending
        };
        var unitOfWork = new InMemoryUnitOfWork();
        unitOfWork.Set<Student>().Items.Add(student);
        unitOfWork.Set<Room>().Items.Add(room);
        unitOfWork.Set<Invoice>().Items.Add(invoice);
        unitOfWork.Set<PaymentExtension>().Items.Add(extension);
        var service = new PaymentExtensionService(
            new AllowAllPermissionService(),
            unitOfWork,
            new RecordingAuditLogService(),
            new TestCurrentUser(RoleNames.BuildingManager, buildingId: Guid.NewGuid()));

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ApproveExtensionAsync(extension.Id));

        Assert.Equal(PaymentExtensionStatus.Pending, extension.Status);
        Assert.Equal(new DateTime(2026, 7, 5), invoice.DueDate);
    }

    [Fact]
    public async Task RequestExtensionAsync_rejects_contract_prepayment_invoice()
    {
        var student = new Student { Id = Guid.NewGuid(), StudentCode = "SV001", FullName = "Nguyen Van An" };
        var invoice = MonthlyInvoice(student.Id, new DateTime(2026, 7, 1));
        invoice.InvoiceKind = InvoiceKind.ContractPrepayment;
        var unitOfWork = new InMemoryUnitOfWork();
        unitOfWork.Set<Student>().Items.Add(student);
        unitOfWork.Set<Invoice>().Items.Add(invoice);
        var service = new PaymentExtensionService(
            new AllowAllPermissionService(),
            unitOfWork,
            new RecordingAuditLogService(),
            new TestCurrentUser(RoleNames.Student, studentId: student.Id));

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.RequestExtensionAsync(new CreatePaymentExtensionRequest
        {
            InvoiceId = invoice.Id,
            RequestedDueDate = new DateTime(2026, 7, 6),
            Reason = "Need a few more days"
        }));
    }

    private static Invoice MonthlyInvoice(Guid studentId, DateTime dueDate) =>
        new()
        {
            Id = Guid.NewGuid(),
            InvoiceNumber = "INV-2026-06-001",
            StudentId = studentId,
            RoomId = Guid.NewGuid(),
            BillingPeriod = "2026-06",
            IssueDate = new DateTime(2026, 6, 5),
            DueDate = dueDate,
            TotalAmount = 100000m,
            PaidAmount = 0m,
            Status = InvoiceStatus.Unpaid,
            InvoiceKind = InvoiceKind.MonthlyUtility
        };
}
