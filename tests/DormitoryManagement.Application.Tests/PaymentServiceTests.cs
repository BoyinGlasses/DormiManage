using DormitoryManagement.Application.Abstractions.Payments;
using DormitoryManagement.Application.DTOs.Payments;
using DormitoryManagement.Application.Services.Payments;
using DormitoryManagement.Domain.Constants;
using DormitoryManagement.Domain.Entities;
using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Application.Tests;

public sealed class PaymentServiceTests
{
    [Fact]
    public async Task GenerateInvoiceQrAsync_creates_stable_transfer_content_and_qr_data()
    {
        var student = new Student { Id = Guid.NewGuid(), StudentCode = "SV001", FullName = "Nguyen Van An" };
        var invoice = Invoice(student.Id, "INV-QR", new DateTime(2026, 6, 5), 500000m, 0m, InvoiceStatus.Unpaid);
        var unitOfWork = new InMemoryUnitOfWork();
        unitOfWork.Set<Student>().Items.Add(student);
        unitOfWork.Set<Invoice>().Items.Add(invoice);
        var audit = new RecordingAuditLogService();
        var qrService = new RecordingPayOsService("data:image/png;base64,AAAA");
        var service = new PaymentService(
            new AllowAllPermissionService(),
            unitOfWork,
            audit,
            new TestCurrentUser(RoleNames.Manager),
            payOsService: qrService);

        var qr = await service.GenerateInvoiceQrAsync(invoice.Id);
        var again = await service.GenerateInvoiceQrAsync(invoice.Id);

        Assert.Equal(invoice.Id, qr.InvoiceId);
        Assert.Equal(500000m, qr.Amount);
        Assert.Equal(9, qr.TransferContent.Length);
        Assert.StartsWith("K", qr.TransferContent);
        Assert.Equal("data:image/png;base64,AAAA", qr.QrDataUrl);
        Assert.Equal(qr.TransferContent, invoice.TransferContent);
        Assert.Equal(qr.QrDataUrl, invoice.QrDataUrl);
        Assert.Equal(qr.TransferContent, again.TransferContent);
        Assert.Equal(1, qrService.CallCount);
        Assert.Contains(audit.Entries, entry => entry.Action == "Payment.QrGenerated");
    }

    [Fact]
    public async Task GenerateInvoiceQrAsync_rejects_paid_invoice()
    {
        var student = new Student { Id = Guid.NewGuid(), StudentCode = "SV001", FullName = "Nguyen Van An" };
        var invoice = Invoice(student.Id, "INV-PAID", new DateTime(2026, 6, 5), 500000m, 500000m, InvoiceStatus.Paid);
        var unitOfWork = new InMemoryUnitOfWork();
        unitOfWork.Set<Student>().Items.Add(student);
        unitOfWork.Set<Invoice>().Items.Add(invoice);
        var service = new PaymentService(
            new AllowAllPermissionService(),
            unitOfWork,
            new RecordingAuditLogService(),
            new TestCurrentUser(RoleNames.Manager),
            payOsService: new RecordingPayOsService("data:image/png;base64,AAAA"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GenerateInvoiceQrAsync(invoice.Id));
    }

    [Fact]
    public async Task ProcessBankTransferAsync_matches_invoice_and_creates_success_payment()
    {
        var student = new Student { Id = Guid.NewGuid(), StudentCode = "SV045", FullName = "Le Minh" };
        var invoice = Invoice(student.Id, "HD1024", new DateTime(2026, 6, 5), 500000m, 0m, InvoiceStatus.Unpaid);
        invoice.TransferContent = "K1234ABC";
        invoice.QrDataUrl = "https://qr.local/hd1024";
        var unitOfWork = new InMemoryUnitOfWork();
        unitOfWork.Set<Student>().Items.Add(student);
        unitOfWork.Set<Invoice>().Items.Add(invoice);
        var audit = new RecordingAuditLogService();
        var service = new PaymentService(
            new AllowAllPermissionService(),
            unitOfWork,
            audit,
            new TestCurrentUser(RoleNames.Manager));

        var result = await service.ProcessBankTransferAsync(new BankTransferNotificationDto
        {
            TransactionId = "BANK-100",
            Amount = 500000m,
            Description = "Thanh toan K1234ABC",
            TransactionDate = new DateTime(2026, 6, 2, 8, 30, 0)
        });

        Assert.True(result.Matched);
        Assert.False(result.Duplicate);
        Assert.Equal(invoice.Id, result.InvoiceId);
        Assert.Equal(InvoiceStatus.Paid, invoice.Status);
        Assert.Equal(500000m, invoice.PaidAmount);
        Assert.Equal("BANK-100", invoice.BankTransactionId);
        var payment = Assert.Single(unitOfWork.Set<Payment>().Items);
        Assert.Equal(PaymentMethod.QrBanking, payment.Method);
        Assert.Equal(PaymentStatus.Success, payment.Status);
        Assert.Equal("BANK-100", payment.TransactionRef);
        Assert.Equal(invoice.Id, payment.InvoiceId);
        Assert.True(unitOfWork.LastTransaction?.Committed);
        Assert.Contains(audit.Entries, entry => entry.Action == "Payment.BankTransferMatched");
    }

    [Fact]
    public async Task ProcessBankTransferAsync_is_idempotent_for_duplicate_transaction()
    {
        var student = new Student { Id = Guid.NewGuid(), StudentCode = "SV045", FullName = "Le Minh" };
        var invoice = Invoice(student.Id, "HD1024", new DateTime(2026, 6, 5), 500000m, 500000m, InvoiceStatus.Paid);
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            StudentId = student.Id,
            InvoiceId = invoice.Id,
            PaymentCode = "PAY-OLD",
            Amount = 500000m,
            Method = PaymentMethod.QrBanking,
            Status = PaymentStatus.Success,
            TransactionRef = "BANK-100"
        };
        var unitOfWork = new InMemoryUnitOfWork();
        unitOfWork.Set<Student>().Items.Add(student);
        unitOfWork.Set<Invoice>().Items.Add(invoice);
        unitOfWork.Set<Payment>().Items.Add(payment);
        var service = new PaymentService(
            new AllowAllPermissionService(),
            unitOfWork,
            new RecordingAuditLogService(),
            new TestCurrentUser(RoleNames.Manager));

        var result = await service.ProcessBankTransferAsync(new BankTransferNotificationDto
        {
            TransactionId = "BANK-100",
            Amount = 500000m,
            Description = "Thanh toan K1234ABC",
            TransactionDate = new DateTime(2026, 6, 2, 8, 30, 0)
        });

        Assert.True(result.Duplicate);
        Assert.False(result.Matched);
        Assert.Equal(payment.Id, result.PaymentId);
        Assert.Single(unitOfWork.Set<Payment>().Items);
    }

    [Fact]
    public async Task ProcessBankTransferAsync_leaves_invoice_unchanged_when_amount_mismatches()
    {
        var student = new Student { Id = Guid.NewGuid(), StudentCode = "SV045", FullName = "Le Minh" };
        var invoice = Invoice(student.Id, "HD1024", new DateTime(2026, 6, 5), 500000m, 0m, InvoiceStatus.Unpaid);
        invoice.TransferContent = "K1234ABC";
        var unitOfWork = new InMemoryUnitOfWork();
        unitOfWork.Set<Student>().Items.Add(student);
        unitOfWork.Set<Invoice>().Items.Add(invoice);
        var audit = new RecordingAuditLogService();
        var service = new PaymentService(
            new AllowAllPermissionService(),
            unitOfWork,
            audit,
            new TestCurrentUser(RoleNames.Manager));

        var result = await service.ProcessBankTransferAsync(new BankTransferNotificationDto
        {
            TransactionId = "BANK-200",
            Amount = 400000m,
            Description = "Thanh toan K1234ABC",
            TransactionDate = new DateTime(2026, 6, 2, 8, 30, 0)
        });

        Assert.False(result.Matched);
        Assert.Equal("Unmatched", result.Status);
        Assert.Equal(InvoiceStatus.Unpaid, invoice.Status);
        Assert.Equal(0m, invoice.PaidAmount);
        Assert.Empty(unitOfWork.Set<Payment>().Items);
        Assert.Contains(audit.Entries, entry => entry.Action == "Payment.BankTransferUnmatched");
    }

    [Fact]
    public async Task CreateMockPaymentAsync_as_student_creates_pending_payment_for_current_student()
    {
        var student = new Student { Id = Guid.NewGuid(), StudentCode = "SV001", FullName = "Nguyen Van An" };
        var invoice = Invoice(student.Id, "INV-PREPAY", new DateTime(2026, 6, 1), 4500000m, 0m, InvoiceStatus.Unpaid);
        invoice.InvoiceKind = InvoiceKind.ContractPrepayment;
        var unitOfWork = new InMemoryUnitOfWork();
        unitOfWork.Set<Student>().Items.Add(student);
        unitOfWork.Set<Invoice>().Items.Add(invoice);
        var audit = new RecordingAuditLogService();
        var service = new PaymentService(
            new AllowAllPermissionService(),
            unitOfWork,
            audit,
            new TestCurrentUser(RoleNames.Student, studentId: student.Id));

        var payment = await service.CreateMockPaymentAsync(new CreatePaymentRequest
        {
            InvoiceId = invoice.Id,
            Amount = 4500000m,
            Method = PaymentMethod.QrBanking
        });

        var entity = Assert.Single(unitOfWork.Set<Payment>().Items);
        Assert.Equal(student.Id, entity.StudentId);
        Assert.Equal(invoice.Id, entity.InvoiceId);
        Assert.Equal(4500000m, entity.Amount);
        Assert.Equal(PaymentStatus.Pending, entity.Status);
        Assert.Equal(entity.Id, payment.Id);
        Assert.Contains(audit.Entries, entry => entry.Action == "Payment.Created");
    }

    [Fact]
    public async Task CreateMockPaymentAsync_rejects_partial_contract_prepayment()
    {
        var student = new Student { Id = Guid.NewGuid(), StudentCode = "SV001", FullName = "Nguyen Van An" };
        var invoice = Invoice(student.Id, "INV-PREPAY", new DateTime(2026, 6, 1), 4500000m, 0m, InvoiceStatus.Unpaid);
        invoice.InvoiceKind = InvoiceKind.ContractPrepayment;
        var unitOfWork = new InMemoryUnitOfWork();
        unitOfWork.Set<Student>().Items.Add(student);
        unitOfWork.Set<Invoice>().Items.Add(invoice);
        var service = new PaymentService(
            new AllowAllPermissionService(),
            unitOfWork,
            new RecordingAuditLogService(),
            new TestCurrentUser(RoleNames.Student, studentId: student.Id));

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateMockPaymentAsync(new CreatePaymentRequest
        {
            InvoiceId = invoice.Id,
            Amount = 1000000m,
            Method = PaymentMethod.QrBanking
        }));

        Assert.Empty(unitOfWork.Set<Payment>().Items);
    }

    [Fact]
    public async Task ConfirmPaymentAsync_targeted_contract_prepayment_activates_assignment_contract_and_registration()
    {
        var student = new Student { Id = Guid.NewGuid(), StudentCode = "SV001", FullName = "Nguyen Van An" };
        var room = new Room { Id = Guid.NewGuid(), RoomNumber = "A-101", Capacity = 2, MonthlyPrice = 750000m, Status = RoomStatus.Available };
        var registration = new RoomRegistration
        {
            Id = Guid.NewGuid(),
            StudentId = student.Id,
            RoomId = room.Id,
            Status = RegistrationStatus.PaymentPending
        };
        var contract = new Contract
        {
            Id = Guid.NewGuid(),
            StudentId = student.Id,
            RoomId = room.Id,
            RoomRegistrationId = registration.Id,
            StartDate = new DateTime(2026, 6, 1),
            EndDate = new DateTime(2026, 11, 30),
            Status = ContractStatus.PendingPayment,
            TotalAmount = 4500000m
        };
        var invoice = Invoice(student.Id, "INV-PREPAY", new DateTime(2026, 6, 1), 4500000m, 0m, InvoiceStatus.Unpaid);
        invoice.InvoiceKind = InvoiceKind.ContractPrepayment;
        invoice.ContractId = contract.Id;
        contract.UpfrontInvoiceId = invoice.Id;
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            StudentId = student.Id,
            InvoiceId = invoice.Id,
            PaymentCode = "PAY-PREPAY",
            Amount = 4500000m,
            Status = PaymentStatus.Pending,
            CreatedAt = new DateTime(2026, 5, 1)
        };
        var unitOfWork = new InMemoryUnitOfWork();
        unitOfWork.Set<Student>().Items.Add(student);
        unitOfWork.Set<Room>().Items.Add(room);
        unitOfWork.Set<RoomRegistration>().Items.Add(registration);
        unitOfWork.Set<Contract>().Items.Add(contract);
        unitOfWork.Set<Invoice>().Items.Add(invoice);
        unitOfWork.Set<Payment>().Items.Add(payment);
        var audit = new RecordingAuditLogService();
        var service = new PaymentService(
            new AllowAllPermissionService(),
            unitOfWork,
            audit,
            new TestCurrentUser(RoleNames.Manager));

        await service.ConfirmPaymentAsync(new ConfirmPaymentRequest { PaymentId = payment.Id, TransactionRef = "BANK-999" });

        Assert.Equal(InvoiceStatus.Paid, invoice.Status);
        Assert.Equal(4500000m, invoice.PaidAmount);
        Assert.Equal(ContractStatus.Active, contract.Status);
        Assert.Equal(RegistrationStatus.Approved, registration.Status);
        var assignment = Assert.Single(unitOfWork.Set<RoomAssignment>().Items);
        Assert.True(assignment.IsActive);
        Assert.Equal(student.Id, assignment.StudentId);
        Assert.Equal(room.Id, assignment.RoomId);
        Assert.Equal(1, room.CurrentOccupancy);
        Assert.Equal(room.Id, student.CurrentRoomId);
        Assert.Contains(audit.Entries, entry => entry.Action == "Contract.Activated");
        Assert.Contains(audit.Entries, entry => entry.Action == "RoomAssignment.Created");
    }

    [Fact]
    public async Task ConfirmPaymentAsync_pays_target_invoice_in_full()
    {
        var student = new Student { Id = Guid.NewGuid(), StudentCode = "SV002", FullName = "Tran Thi Binh" };
        var invoice = Invoice(student.Id, "INV-FULL", new DateTime(2026, 5, 10), 100000m, 0m, InvoiceStatus.Unpaid);
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            StudentId = student.Id,
            InvoiceId = invoice.Id,
            PaymentCode = "PAY-DEMO",
            Amount = 100000m,
            Status = PaymentStatus.Pending,
            CreatedAt = new DateTime(2026, 5, 1)
        };
        var unitOfWork = new InMemoryUnitOfWork();
        unitOfWork.Set<Student>().Items.Add(student);
        unitOfWork.Set<Payment>().Items.Add(payment);
        unitOfWork.Set<Invoice>().Items.Add(invoice);
        var audit = new RecordingAuditLogService();
        var service = new PaymentService(
            new AllowAllPermissionService(),
            unitOfWork,
            audit,
            new TestCurrentUser(RoleNames.Manager));

        var confirmed = await service.ConfirmPaymentAsync(new ConfirmPaymentRequest
        {
            PaymentId = payment.Id,
            TransactionRef = "BANK-123"
        });

        Assert.Equal(PaymentStatus.Success, payment.Status);
        Assert.Equal(PaymentStatus.Success, confirmed.Status);
        Assert.Equal(invoice.Id, confirmed.InvoiceId);
        Assert.Equal("BANK-123", payment.TransactionRef);
        Assert.NotNull(payment.PaidAt);
        Assert.Equal(InvoiceStatus.Paid, invoice.Status);
        Assert.Equal(100000m, invoice.PaidAmount);
        Assert.True(unitOfWork.LastTransaction?.Committed);
        Assert.Contains(audit.Entries, entry => entry.Action == "Payment.Confirmed");
    }

    [Fact]
    public async Task ConfirmPaymentAsync_marks_vehicle_registration_payment_date_when_vehicle_invoice_paid()
    {
        var student = new Student { Id = Guid.NewGuid(), StudentCode = "SV001", FullName = "Nguyen Van An" };
        var invoice = Invoice(student.Id, "INV-PARK", DateTime.UtcNow.Date.AddDays(2), 40000m, 0m, InvoiceStatus.Unpaid);
        invoice.InvoiceKind = InvoiceKind.VehicleParking;
        var registration = new VehicleRegistration
        {
            Id = Guid.NewGuid(),
            StudentId = student.Id,
            LicensePlate = "59A1-2345",
            MonthCount = 1,
            Amount = 40000m,
            InvoiceId = invoice.Id
        };
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            StudentId = student.Id,
            InvoiceId = invoice.Id,
            PaymentCode = "PAY-PARK",
            Amount = 40000m,
            Status = PaymentStatus.Pending
        };
        var unitOfWork = new InMemoryUnitOfWork();
        unitOfWork.Set<Student>().Items.Add(student);
        unitOfWork.Set<Invoice>().Items.Add(invoice);
        unitOfWork.Set<VehicleRegistration>().Items.Add(registration);
        unitOfWork.Set<Payment>().Items.Add(payment);
        var service = new PaymentService(
            new AllowAllPermissionService(),
            unitOfWork,
            new RecordingAuditLogService(),
            new TestCurrentUser(RoleNames.Manager));

        await service.ConfirmPaymentAsync(new ConfirmPaymentRequest { PaymentId = payment.Id, TransactionRef = "BANK-PARK" });

        Assert.Equal(InvoiceStatus.Paid, invoice.Status);
        Assert.NotNull(payment.PaidAt);
        Assert.Equal(payment.PaidAt.Value.Date, registration.PaymentDate);
    }

    private static Invoice Invoice(Guid studentId, string number, DateTime dueDate, decimal total, decimal paid, InvoiceStatus status) =>
        new()
        {
            Id = Guid.NewGuid(),
            InvoiceNumber = number,
            StudentId = studentId,
            RoomId = Guid.NewGuid(),
            BillingPeriod = "2026-05",
            IssueDate = new DateTime(2026, 5, 1),
            DueDate = dueDate,
            TotalAmount = total,
            PaidAmount = paid,
            Status = status
        };

    private sealed class RecordingPayOsService : IPayOsService
    {
        private readonly string _qrDataUrl;

        public RecordingPayOsService(string qrDataUrl)
        {
            _qrDataUrl = qrDataUrl;
        }

        public int CallCount { get; private set; }

        public Task<PayOsPaymentLinkDto> CreatePaymentLinkAsync(PayOsCreatePaymentRequest request, CancellationToken ct = default)
        {
            CallCount++;
            return Task.FromResult(new PayOsPaymentLinkDto
            {
                OrderCode = request.OrderCode,
                CheckoutUrl = "https://payos.local/checkout",
                PaymentLinkId = "plink_123",
                QrCode = "000201010212",
                QrDataUrl = _qrDataUrl,
                Status = "PENDING"
            });
        }

        public Task<PayOsPaymentStatusDto> GetPaymentLinkAsync(long orderCode, CancellationToken ct = default) =>
            Task.FromResult(new PayOsPaymentStatusDto { OrderCode = orderCode, Status = "PENDING" });

        public Task ConfirmWebhookAsync(string webhookUrl, CancellationToken ct = default) => Task.CompletedTask;

        public PayOsWebhookEventDto ParseWebhook(string payload) => throw new NotSupportedException();
    }
}
