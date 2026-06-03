using DormitoryManagement.Application.Abstractions.Auth;
using DormitoryManagement.Application.DTOs.Auth;
using DormitoryManagement.Application.DTOs.Billing;
using DormitoryManagement.Application.DTOs.Payments;
using DormitoryManagement.Application.Services.Payments;
using DormitoryManagement.Domain.Constants;
using DormitoryManagement.Domain.Enums;
using DormitoryManagement.WPF.ViewModels.Billing;

namespace DormitoryManagement.WPF.Tests;

public sealed class PaymentViewModelTests
{
    [Fact]
    public async Task Load_displays_qr_payment_details_for_selected_invoice()
    {
        var invoice = new OutstandingInvoiceDto
        {
            Id = Guid.NewGuid(),
            InvoiceNumber = "INV-QR",
            InvoiceKind = InvoiceKind.MonthlyUtility,
            BillingPeriod = "2026-06",
            DueDate = new DateTime(2026, 6, 5),
            TotalAmount = 500000m,
            PaidAmount = 0m,
            RemainingAmount = 500000m,
            Status = InvoiceStatus.Unpaid
        };
        var qr = new InvoicePaymentQrDto
        {
            InvoiceId = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            StudentId = Guid.NewGuid(),
            Amount = 500000m,
            TransferContent = "KTX HDINV-QR SV001",
            QrDataUrl = "https://qr.local/inv-qr.png",
            Status = "Unpaid",
            DueDate = invoice.DueDate
        };
        var viewModel = new PaymentViewModel(
            new StubPaymentService(new[] { invoice }, new[] { qr }),
            StudentUser(),
            new StubPaymentExtensionService());

        viewModel.LoadCommand.Execute(null);

        Assert.True(await WaitUntilAsync(() => viewModel.HasQrPaymentDetails));
        Assert.Equal("KTX HDINV-QR SV001", viewModel.QrTransferContent);
        Assert.Equal(500000m.ToString("N0") + " VND", viewModel.QrAmountText);
        Assert.Equal("Unpaid", viewModel.QrStatus);
        Assert.True(viewModel.HasQrImage);
        Assert.True(viewModel.CanRefreshQrStatus);
    }

    [Fact]
    public async Task RefreshQrStatusCommand_updates_paid_status_and_paid_time()
    {
        var invoice = new OutstandingInvoiceDto
        {
            Id = Guid.NewGuid(),
            InvoiceNumber = "INV-QR",
            InvoiceKind = InvoiceKind.MonthlyUtility,
            BillingPeriod = "2026-06",
            DueDate = new DateTime(2026, 6, 5),
            TotalAmount = 500000m,
            PaidAmount = 0m,
            RemainingAmount = 500000m,
            Status = InvoiceStatus.Unpaid
        };
        var paidAt = new DateTime(2026, 6, 2, 8, 30, 0);
        var pendingQr = new InvoicePaymentQrDto
        {
            InvoiceId = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            Amount = 500000m,
            TransferContent = "KTX HDINV-QR SV001",
            QrDataUrl = "https://qr.local/inv-qr.png",
            Status = "Unpaid",
            DueDate = invoice.DueDate
        };
        var paidQr = new InvoicePaymentQrDto
        {
            InvoiceId = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            Amount = 0m,
            TransferContent = "KTX HDINV-QR SV001",
            QrDataUrl = "https://qr.local/inv-qr.png",
            Status = "Paid",
            DueDate = invoice.DueDate,
            PaidAt = paidAt
        };
        var viewModel = new PaymentViewModel(
            new StubPaymentService(new[] { invoice }, new[] { pendingQr, paidQr }),
            StudentUser(),
            new StubPaymentExtensionService());

        viewModel.LoadCommand.Execute(null);
        Assert.True(await WaitUntilAsync(() => viewModel.QrStatus == "Unpaid"));

        viewModel.RefreshQrStatusCommand.Execute(null);

        Assert.True(await WaitUntilAsync(() => viewModel.QrStatus == "Paid"));
        Assert.True(viewModel.HasQrPaidAt);
        Assert.Equal(paidAt.ToString("g"), viewModel.QrPaidAtText);
    }

    [Fact]
    public void Selecting_contract_prepayment_invoice_sets_full_remaining_amount()
    {
        var viewModel = new PaymentViewModel(new StubPaymentService(), StudentUser(), new StubPaymentExtensionService());
        var invoice = new OutstandingInvoiceDto
        {
            Id = Guid.NewGuid(),
            InvoiceNumber = "INV-PREPAY",
            InvoiceKind = InvoiceKind.ContractPrepayment,
            RemainingAmount = 4500000m,
            TotalAmount = 4500000m
        };

        viewModel.OutstandingInvoices.Add(invoice);
        viewModel.SelectedInvoice = invoice;

        Assert.Equal(4500000m, viewModel.Amount);
        Assert.True(viewModel.CanCreatePayment);
    }

    [Fact]
    public void Monthly_utility_invoice_enables_extension_request_command()
    {
        var viewModel = new PaymentViewModel(new StubPaymentService(), StudentUser(), new StubPaymentExtensionService());
        var invoice = new OutstandingInvoiceDto
        {
            Id = Guid.NewGuid(),
            InvoiceNumber = "INV-UTILITY",
            InvoiceKind = InvoiceKind.MonthlyUtility,
            RemainingAmount = 100000m,
            TotalAmount = 100000m
        };

        viewModel.OutstandingInvoices.Add(invoice);
        viewModel.SelectedInvoice = invoice;
        viewModel.ExtensionRequestedDueDate = new DateTime(2026, 6, 15);
        viewModel.ExtensionReason = "Need more time";

        Assert.True(viewModel.CanRequestExtension);
    }

    [Fact]
    public async Task Payment_context_preselects_matching_invoice_and_amount()
    {
        var invoice = new OutstandingInvoiceDto
        {
            Id = Guid.NewGuid(),
            InvoiceNumber = "INV-UTILITY",
            InvoiceKind = InvoiceKind.MonthlyUtility,
            BillingPeriod = "2026-06",
            DueDate = new DateTime(2026, 6, 10),
            TotalAmount = 178500m,
            PaidAmount = 50000m,
            RemainingAmount = 128500m,
            Status = InvoiceStatus.Partial
        };
        var viewModel = new PaymentViewModel(new StubPaymentService(new[] { invoice }), StudentUser(), new StubPaymentExtensionService());

        viewModel.LoadCommand.Execute(null);
        Assert.True(await WaitUntilAsync(() => viewModel.OutstandingInvoices.Count == 1));
        InvokeContextMethod(viewModel, "ApplyPaymentContext", CreatePaymentNavigationContext(invoice));

        Assert.Equal(invoice.Id, viewModel.SelectedInvoice?.Id);
        Assert.Equal(invoice.RemainingAmount, viewModel.Amount);
    }

    [Fact]
    public async Task Extension_context_preselects_invoice_and_defaults_to_max_due_date()
    {
        var invoice = new OutstandingInvoiceDto
        {
            Id = Guid.NewGuid(),
            InvoiceNumber = "INV-UTILITY",
            InvoiceKind = InvoiceKind.MonthlyUtility,
            BillingPeriod = "2026-06",
            DueDate = new DateTime(2026, 6, 10),
            TotalAmount = 178500m,
            PaidAmount = 0m,
            RemainingAmount = 178500m,
            Status = InvoiceStatus.Unpaid
        };
        var viewModel = new PaymentViewModel(new StubPaymentService(new[] { invoice }), StudentUser(), new StubPaymentExtensionService());

        viewModel.LoadCommand.Execute(null);
        Assert.True(await WaitUntilAsync(() => viewModel.OutstandingInvoices.Count == 1));
        InvokeContextMethod(viewModel, "ApplyExtensionContext", CreatePaymentNavigationContext(invoice));

        Assert.Equal(invoice.Id, viewModel.SelectedInvoice?.Id);
        Assert.Equal(new DateTime(2026, 6, 15), viewModel.ExtensionRequestedDueDate);
    }

    [Fact]
    public async Task Stored_payment_context_preselects_invoice_on_load()
    {
        var invoice = new OutstandingInvoiceDto
        {
            Id = Guid.NewGuid(),
            InvoiceNumber = "INV-UTILITY",
            InvoiceKind = InvoiceKind.MonthlyUtility,
            BillingPeriod = "2026-06",
            DueDate = new DateTime(2026, 6, 10),
            TotalAmount = 178500m,
            PaidAmount = 50000m,
            RemainingAmount = 128500m,
            Status = InvoiceStatus.Partial
        };
        var state = new PaymentNavigationState();
        state.SetPaymentContext(CreatePaymentNavigationContext(invoice));
        var viewModel = new PaymentViewModel(new StubPaymentService(new[] { invoice }), StudentUser(), new StubPaymentExtensionService(), state);

        viewModel.LoadCommand.Execute(null);
        Assert.True(await WaitUntilAsync(() => viewModel.OutstandingInvoices.Count == 1));

        Assert.Equal(invoice.Id, viewModel.SelectedInvoice?.Id);
        Assert.Equal(invoice.RemainingAmount, viewModel.Amount);
        Assert.Null(state.PaymentContext);
    }

    private static ICurrentUserService StudentUser() => new StubCurrentUser(RoleNames.Student, Guid.NewGuid());

    private static async Task<bool> WaitUntilAsync(Func<bool> condition)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        while (!cts.IsCancellationRequested)
        {
            if (condition())
            {
                return true;
            }

            await Task.Delay(10, cts.Token);
        }

        return false;
    }

    private static PaymentNavigationContextDto CreatePaymentNavigationContext(OutstandingInvoiceDto invoice) =>
        new()
        {
            InvoiceId = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            InvoiceKind = invoice.InvoiceKind,
            BillingPeriod = invoice.BillingPeriod,
            DueDate = invoice.DueDate,
            TotalAmount = invoice.TotalAmount,
            PaidAmount = invoice.PaidAmount,
            RemainingAmount = invoice.RemainingAmount
        };

    private static void InvokeContextMethod(PaymentViewModel viewModel, string methodName, object context)
    {
        var method = typeof(PaymentViewModel).GetMethod(methodName, new[] { context.GetType() });
        Assert.NotNull(method);
        method.Invoke(viewModel, new[] { context });
    }

    private sealed class StubPaymentService : IPaymentService
    {
        private readonly IReadOnlyList<OutstandingInvoiceDto> _outstandingInvoices;
        private readonly Dictionary<Guid, Queue<InvoicePaymentQrDto>> _qrDetails;

        public StubPaymentService(
            IReadOnlyList<OutstandingInvoiceDto>? outstandingInvoices = null,
            IReadOnlyList<InvoicePaymentQrDto>? qrDetails = null)
        {
            _outstandingInvoices = outstandingInvoices ?? Array.Empty<OutstandingInvoiceDto>();
            _qrDetails = (qrDetails ?? Array.Empty<InvoicePaymentQrDto>())
                .GroupBy(qr => qr.InvoiceId)
                .ToDictionary(group => group.Key, group => new Queue<InvoicePaymentQrDto>(group));
        }

        public Task<IReadOnlyList<OutstandingInvoiceDto>> GetOutstandingInvoicesAsync(CancellationToken ct = default) =>
            Task.FromResult(_outstandingInvoices);

        public Task<IReadOnlyList<PaymentDto>> GetPendingPaymentsAsync(CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<PaymentDto>>(Array.Empty<PaymentDto>());

        public Task<PaymentDto> CreateMockPaymentAsync(CreatePaymentRequest request, CancellationToken ct = default) =>
            Task.FromResult(new PaymentDto { Id = Guid.NewGuid(), PaymentCode = "PAY-1", Status = PaymentStatus.Pending });

        public Task<InvoicePaymentQrDto> GenerateInvoiceQrAsync(Guid invoiceId, CancellationToken ct = default) =>
            GetInvoicePaymentQrAsync(invoiceId, ct);

        public Task<InvoicePaymentQrDto> GetInvoicePaymentQrAsync(Guid invoiceId, CancellationToken ct = default)
        {
            if (!_qrDetails.TryGetValue(invoiceId, out var queue) || queue.Count == 0)
            {
                return Task.FromResult(new InvoicePaymentQrDto { InvoiceId = invoiceId, Status = "Unpaid" });
            }

            return Task.FromResult(queue.Count > 1 ? queue.Dequeue() : queue.Peek());
        }

        public Task<BankTransferProcessResultDto> ProcessBankTransferAsync(BankTransferNotificationDto notification, CancellationToken ct = default) =>
            Task.FromResult(new BankTransferProcessResultDto());

        public Task<PaymentDto> ConfirmPaymentAsync(ConfirmPaymentRequest request, CancellationToken ct = default) =>
            Task.FromResult(new PaymentDto { Id = request.PaymentId, PaymentCode = "PAY-1", Status = PaymentStatus.Success });

        public Task AllocatePaymentAsync(Guid paymentId, Guid invoiceId, decimal amount, CancellationToken ct = default) =>
            Task.CompletedTask;

        public Task CancelPaymentAsync(Guid paymentId, string reason, CancellationToken ct = default) =>
            Task.CompletedTask;
    }

    private sealed class StubPaymentExtensionService : IPaymentExtensionService
    {
        public Task<PaymentExtensionDto> RequestExtensionAsync(CreatePaymentExtensionRequest request, CancellationToken ct = default) =>
            Task.FromResult(new PaymentExtensionDto { Id = Guid.NewGuid(), InvoiceId = request.InvoiceId, Status = PaymentExtensionStatus.Pending });

        public Task<IReadOnlyList<PaymentExtensionDto>> GetPendingExtensionsAsync(CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<PaymentExtensionDto>>(Array.Empty<PaymentExtensionDto>());

        public Task ApproveExtensionAsync(Guid extensionId, CancellationToken ct = default) => Task.CompletedTask;
        public Task RejectExtensionAsync(Guid extensionId, string reason, CancellationToken ct = default) => Task.CompletedTask;
    }

    private sealed class StubCurrentUser : ICurrentUserService
    {
        public StubCurrentUser(string roleName, Guid studentId)
        {
            CurrentUser = new CurrentUserDto
            {
                UserId = Guid.NewGuid(),
                Username = roleName,
                Email = roleName + "@ktx.local",
                FullName = roleName,
                RoleName = roleName,
                StudentId = studentId
            };
        }

        public CurrentUserDto? CurrentUser { get; }
        public Guid? UserId => CurrentUser?.UserId;
        public string? UserName => CurrentUser?.Username;
        public string? Email => CurrentUser?.Email;
        public string? FullName => CurrentUser?.FullName;
        public IReadOnlyCollection<string> Roles => CurrentUser?.RoleName is { Length: > 0 } roleName ? new[] { roleName } : Array.Empty<string>();
        public bool IsAuthenticated => CurrentUser is not null;
        public bool IsInRole(string roleName) => Roles.Contains(roleName, StringComparer.OrdinalIgnoreCase);
    }
}
