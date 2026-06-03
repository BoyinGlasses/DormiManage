using System.Collections;
using DormitoryManagement.Application.Abstractions.Auth;
using DormitoryManagement.Application.DTOs.Auth;
using DormitoryManagement.Application.DTOs.Billing;
using DormitoryManagement.Application.Services.Billing;
using DormitoryManagement.Domain.Constants;
using DormitoryManagement.Domain.Enums;
using DormitoryManagement.WPF.Common;
using DormitoryManagement.WPF.Navigation;
using DormitoryManagement.WPF.ViewModels.Billing;

namespace DormitoryManagement.WPF.Tests;

public sealed class InvoiceListViewModelTests
{
    [Fact]
    public void Utility_billing_inputs_default_period_and_accept_editable_text()
    {
        var viewModel = new InvoiceListViewModel(
            new StubBillingService(Array.Empty<InvoiceDto>()),
            ManagerUser());

        Assert.Equal(DateTime.Today.ToString("yyyy-MM"), viewModel.BillingPeriod);
        Assert.Equal(new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(1).AddDays(4), viewModel.DueDate);

        viewModel.BillingPeriod = "2026-08";
        viewModel.ElectricityCurrentText = "abc123";
        viewModel.WaterCurrentText = "45.6";

        Assert.Equal("2026-08", viewModel.BillingPeriod);
        Assert.Equal(new DateTime(2026, 9, 5), viewModel.DueDate);
        Assert.Equal("abc123", viewModel.ElectricityCurrentText);
        Assert.Equal("45.6", viewModel.WaterCurrentText);
    }

    [Fact]
    public async Task Student_billing_rows_expose_extend_and_payment_action_state()
    {
        var invoices = new[]
        {
            Invoice("INV-UTILITY-UNPAID", InvoiceKind.MonthlyUtility, InvoiceStatus.Unpaid, 178500m, 0m),
            Invoice("INV-UTILITY-PAID", InvoiceKind.MonthlyUtility, InvoiceStatus.Paid, 178500m, 178500m),
            Invoice("INV-PARK", InvoiceKind.VehicleParking, InvoiceStatus.Unpaid, 50000m, 0m)
        };
        var navigation = new RecordingNavigationService();
        var paymentNavigationState = new PaymentNavigationState();
        var viewModel = new InvoiceListViewModel(
            new StubBillingService(invoices),
            StudentUser(),
            navigationService: navigation,
            paymentNavigationState: paymentNavigationState);

        viewModel.LoadCommand.Execute(null);

        Assert.True(await WaitUntilAsync(() => viewModel.Invoices.Count == 3));
        var rows = GetEnumerableProperty(viewModel, "StudentBillingRows").Cast<object>().ToArray();
        Assert.Equal(3, rows.Length);

        var unpaidUtility = RowByInvoice(rows, "INV-UTILITY-UNPAID");
        Assert.Equal(InvoiceKind.MonthlyUtility, GetProperty<InvoiceKind>(unpaidUtility, "InvoiceKind"));
        Assert.True(GetProperty<bool>(unpaidUtility, "CanShowExtendAction"));
        Assert.True(GetProperty<bool>(unpaidUtility, "CanExtend"));
        Assert.True(GetProperty<bool>(unpaidUtility, "CanPay"));
        Assert.True(viewModel.PayInvoiceCommand.CanExecute(unpaidUtility));
        viewModel.PayInvoiceCommand.Execute(unpaidUtility);
        Assert.Equal("INV-UTILITY-UNPAID", viewModel.SelectedPaymentContext?.InvoiceNumber);
        Assert.Equal(178500m, viewModel.SelectedPaymentContext?.RemainingAmount);
        Assert.Equal("INV-UTILITY-UNPAID", paymentNavigationState.PaymentContext?.InvoiceNumber);
        Assert.Equal(typeof(PaymentViewModel), navigation.LastViewModelType);
        Assert.True(viewModel.ExtendInvoiceCommand.CanExecute(unpaidUtility));
        viewModel.ExtendInvoiceCommand.Execute(unpaidUtility);
        Assert.Equal("INV-UTILITY-UNPAID", viewModel.SelectedExtensionContext?.InvoiceNumber);
        Assert.Equal("INV-UTILITY-UNPAID", paymentNavigationState.ExtensionContext?.InvoiceNumber);
        Assert.Equal(typeof(PaymentViewModel), navigation.LastViewModelType);

        var paidUtility = RowByInvoice(rows, "INV-UTILITY-PAID");
        Assert.Equal(InvoiceStatus.Paid, GetProperty<InvoiceStatus>(paidUtility, "Status"));
        Assert.True(GetProperty<bool>(paidUtility, "CanShowExtendAction"));
        Assert.False(GetProperty<bool>(paidUtility, "CanExtend"));
        Assert.True(GetProperty<bool>(paidUtility, "CanPay"));
        Assert.False(viewModel.ExtendInvoiceCommand.CanExecute(paidUtility));

        var vehicleParking = RowByInvoice(rows, "INV-PARK");
        Assert.Equal(InvoiceKind.VehicleParking, GetProperty<InvoiceKind>(vehicleParking, "InvoiceKind"));
        Assert.False(GetProperty<bool>(vehicleParking, "CanShowExtendAction"));
        Assert.False(GetProperty<bool>(vehicleParking, "CanExtend"));
        Assert.True(GetProperty<bool>(vehicleParking, "CanPay"));
        Assert.False(viewModel.ExtendInvoiceCommand.CanExecute(vehicleParking));
    }

    private static InvoiceDto Invoice(string number, InvoiceKind kind, InvoiceStatus status, decimal total, decimal paid) =>
        new()
        {
            Id = Guid.NewGuid(),
            InvoiceNumber = number,
            StudentId = Guid.NewGuid(),
            StudentCode = "SV001",
            StudentName = "Nguyen Van An",
            RoomId = Guid.NewGuid(),
            RoomNumber = "A-101",
            BillingPeriod = "2026-06",
            InvoiceKind = kind,
            IssueDate = new DateTime(2026, 6, 1),
            DueDate = new DateTime(2026, 6, 10),
            TotalAmount = total,
            PaidAmount = paid,
            Status = status
        };

    private static ICurrentUserService StudentUser() => new StubCurrentUser(RoleNames.Student, Guid.NewGuid());

    private static ICurrentUserService ManagerUser() => new StubCurrentUser(RoleNames.Manager, Guid.NewGuid());

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

    private static IEnumerable GetEnumerableProperty(object target, string propertyName)
    {
        var property = target.GetType().GetProperty(propertyName);
        Assert.NotNull(property);
        var value = property.GetValue(target);
        return Assert.IsAssignableFrom<IEnumerable>(value);
    }

    private static object RowByInvoice(IEnumerable<object> rows, string invoiceNumber) =>
        rows.Single(row => GetProperty<string>(row, "InvoiceNumber") == invoiceNumber);

    private static T GetProperty<T>(object target, string propertyName)
    {
        var property = target.GetType().GetProperty(propertyName);
        Assert.NotNull(property);
        return Assert.IsType<T>(property.GetValue(target));
    }

    private sealed class StubBillingService : IBillingService
    {
        private readonly IReadOnlyList<InvoiceDto> _invoices;

        public StubBillingService(IReadOnlyList<InvoiceDto> invoices)
        {
            _invoices = invoices;
        }

        public Task<UtilityBillingPreviewDto> PreviewUtilityBillingAsync(UtilityBillingPreviewRequest request, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task UpsertUtilityReadingAsync(UtilityReadingRequest request, CancellationToken ct = default) => Task.CompletedTask;

        public Task<GenerateMonthlyInvoiceResult> GenerateMonthlyInvoicesAsync(GenerateMonthlyInvoiceRequest request, CancellationToken ct = default) =>
            Task.FromResult(new GenerateMonthlyInvoiceResult());

        public Task<IReadOnlyList<InvoiceDto>> GetInvoicesAsync(string? billingPeriod = null, CancellationToken ct = default) =>
            Task.FromResult(_invoices);

        public Task<IReadOnlyList<StudentBillingRowDto>> GetStudentBillingRowsAsync(string? billingPeriod = null, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<StudentBillingRowDto>>(_invoices.Select(invoice =>
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
            }).ToArray());

        public Task<InvoiceDto?> GetInvoiceAsync(Guid invoiceId, CancellationToken ct = default) =>
            Task.FromResult(_invoices.FirstOrDefault(invoice => invoice.Id == invoiceId));

        public Task<InvoiceDto> CreateInvoiceAsync(Guid studentId, Guid roomId, string billingPeriod, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task AddInvoiceItemAsync(Guid invoiceId, InvoiceItemDto item, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task<int> MarkOverdueInvoicesAsync(DateTime asOfDate, CancellationToken ct = default) =>
            Task.FromResult(0);

        public Task AdjustInvoiceAsync(Guid invoiceId, decimal amount, string reason, CancellationToken ct = default) =>
            throw new NotSupportedException();
    }

    private sealed class RecordingNavigationService : INavigationService
    {
        public Type? LastViewModelType { get; private set; }

        public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase
        {
            LastViewModelType = typeof(TViewModel);
        }
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
