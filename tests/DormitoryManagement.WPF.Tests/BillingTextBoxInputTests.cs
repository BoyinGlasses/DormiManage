using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using DormitoryManagement.Application.Abstractions.Auth;
using DormitoryManagement.Application.DTOs.Auth;
using DormitoryManagement.Application.DTOs.Billing;
using DormitoryManagement.Application.DTOs.Payments;
using DormitoryManagement.Application.DTOs.Vehicles;
using DormitoryManagement.Application.Services.Billing;
using DormitoryManagement.Application.Services.Vehicles;
using DormitoryManagement.Domain.Constants;
using DormitoryManagement.WPF.Views.Vehicles;
using DormitoryManagement.WPF.Views.Billing;
using DormitoryManagement.WPF.ViewModels.Billing;
using DormitoryManagement.WPF.ViewModels.Vehicles;

namespace DormitoryManagement.WPF.Tests;

public sealed class BillingTextBoxInputTests
{
    [Fact]
    public void WpfUi_vehicle_license_plate_textbox_accepts_keyboard_input()
    {
        RunOnStaThread(() =>
        {
            EnsureApplication();
            var viewModel = new VehicleRegistrationViewModel(new PopulatedVehicleHistoryService());
            var view = new VehicleRegistrationView { DataContext = viewModel };
            var window = new Window
            {
                Width = 900,
                Height = 500,
                Content = view,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Left = -10000,
                Top = -10000,
                ShowInTaskbar = false
            };

            try
            {
                window.Show();
                WaitForLayout();
                WaitUntil(() => !viewModel.IsLoading);

                var licensePlate = FindTextBox(view, "Biển số xe");
                AssertWpfUiTextBox(licensePlate);
                AssertTextBoxCanEdit(licensePlate);
                AssertVehicleRegistrationTextBoxShape(licensePlate);
                AssertHitTestTargetsTextBox(view, licensePlate);

                TypeInto(window, licensePlate, "59a12345");

                Assert.Equal("59A12345", licensePlate.Text);
                Assert.Equal("59A12345", viewModel.LicensePlate);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void Vehicle_registration_history_review_surface_uses_reference_headers_without_legacy_datagrid()
    {
        RunOnStaThread(() =>
        {
            EnsureApplication();
            var viewModel = new VehicleRegistrationViewModel(new PopulatedVehicleHistoryService());
            var view = new VehicleRegistrationView { DataContext = viewModel };
            var window = new Window
            {
                Width = 1360,
                Height = 840,
                Content = view,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Left = -10000,
                Top = -10000,
                ShowInTaskbar = false
            };

            try
            {
                window.Show();
                WaitForLayout();
                WaitUntil(() => !viewModel.IsLoading);

                Assert.Empty(FindDescendants<DataGrid>(view));

                var texts = GetVisibleStringSequence(view);
                Assert.Contains("Xem tất cả", texts);
                AssertTextOrder(
                    texts,
                    "Lịch sử đăng ký",
                    "Ngày đăng ký",
                    "Biển số xe",
                    "Thời hạn",
                    "Tổng tiền",
                    "Trạng thái");
                Assert.DoesNotContain("STT", texts);
                Assert.DoesNotContain("Ngày thanh toán", texts);
                Assert.DoesNotContain("Ngày hết hạn vé tháng", texts);
            }
            finally
            {
                window.Close();
            }
        });
    }
    [Fact]
    public void WpfUi_billing_textboxes_accept_keyboard_input()
    {
        RunOnStaThread(() =>
        {
            EnsureApplication();
            var viewModel = new InvoiceListViewModel(
                new StubBillingService(),
                new StubCurrentUser(RoleNames.Manager));
            var view = new InvoiceListView { DataContext = viewModel };
            var window = new Window
            {
                Width = 1360,
                Height = 840,
                Content = view,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Left = -10000,
                Top = -10000,
                ShowInTaskbar = false
            };

            try
            {
                window.Show();
                WaitForLayout();
                WaitUntil(() => !viewModel.IsLoading);

                var electricity = FindTextBox(view, "Electricity reading");
                var water = FindTextBox(view, "Water reading");
                var billingPeriod = FindTextBox(view, "Billing period");
                AssertWpfUiTextBox(electricity);
                AssertWpfUiTextBox(water);
                AssertWpfUiTextBox(billingPeriod);
                AssertTextBoxCanEdit(electricity);
                AssertTextBoxCanEdit(water);
                AssertTextBoxCanEdit(billingPeriod);
                AssertVehicleTextBoxShape(electricity);
                AssertVehicleTextBoxShape(water);
                AssertVehicleTextBoxShape(billingPeriod);
                AssertHitTestTargetsTextBox(view, electricity);
                AssertHitTestTargetsTextBox(view, water);
                AssertHitTestTargetsTextBox(view, billingPeriod);

                TypeInto(window, electricity, "123");
                TypeInto(window, water, "45");
                ReplaceText(window, billingPeriod, "2026-08");

                Assert.Equal("123", electricity.Text);
                Assert.Equal("45", water.Text);
                Assert.Equal("2026-08", billingPeriod.Text);
                Assert.Equal("123", viewModel.ElectricityCurrentText);
                Assert.Equal("45", viewModel.WaterCurrentText);
                Assert.Equal("2026-08", viewModel.BillingPeriod);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void Invoice_records_grids_do_not_auto_generate_columns()
    {
        RunOnStaThread(() =>
        {
            EnsureApplication();
            var viewModel = new InvoiceListViewModel(
                new StubBillingService(),
                new StubCurrentUser(RoleNames.Student));
            var view = new InvoiceListView { DataContext = viewModel };
            var window = new Window
            {
                Width = 1360,
                Height = 840,
                Content = view,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Left = -10000,
                Top = -10000,
                ShowInTaskbar = false
            };

            try
            {
                window.Show();
                WaitForLayout();

                var grids = FindDescendants<DataGrid>(view).ToArray();

                Assert.True(grids.Length >= 3);
                Assert.All(grids, grid => Assert.False(grid.AutoGenerateColumns));
            }
            finally
            {
                window.Close();
            }
        });
    }

    private static System.Windows.Application EnsureApplication()
    {
        if (System.Windows.Application.Current is not null)
        {
            return System.Windows.Application.Current;
        }

        var application = new System.Windows.Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        application.Resources.MergedDictionaries.Add(new ResourceDictionary
        {
            Source = new Uri("pack://application:,,,/DormitoryManagement.WPF;component/Resources/Themes.xaml")
        });
        return application;
    }

    private static void AssertTextBoxCanEdit(TextBox textBox)
    {
        Assert.True(textBox.IsEnabled);
        Assert.False(textBox.IsReadOnly);
        Assert.True(textBox.Focusable);
        Assert.True(textBox.IsHitTestVisible);
        textBox.ApplyTemplate();
        Assert.NotNull(textBox.Template.FindName("PART_ContentHost", textBox));
    }

    private static void AssertWpfUiTextBox(TextBox textBox)
    {
        Assert.Equal("Wpf.Ui.Controls.TextBox", textBox.GetType().FullName);
    }

    private static void AssertVehicleTextBoxShape(TextBox textBox)
    {
        Assert.Equal(150, textBox.Width);
        Assert.Equal(58, textBox.Height);
        Assert.Equal(HorizontalAlignment.Left, textBox.HorizontalAlignment);
        Assert.Equal(VerticalAlignment.Center, textBox.VerticalContentAlignment);
    }

    private static void AssertVehicleRegistrationTextBoxShape(TextBox textBox)
    {
        Assert.Equal(220, textBox.Width);
        Assert.Equal(44, textBox.Height);
        Assert.Equal(HorizontalAlignment.Left, textBox.HorizontalAlignment);
        Assert.Equal(VerticalAlignment.Center, textBox.VerticalContentAlignment);
    }

    private static void AssertHitTestTargetsTextBox(UIElement root, TextBox textBox)
    {
        var center = textBox.TranslatePoint(
            new Point(textBox.ActualWidth / 2, textBox.ActualHeight / 2),
            root);
        var hit = root.InputHitTest(center) as DependencyObject;
        Assert.True(IsDescendantOf(hit, textBox));
    }

    private static bool IsDescendantOf(DependencyObject? candidate, DependencyObject ancestor)
    {
        while (candidate is not null)
        {
            if (ReferenceEquals(candidate, ancestor))
            {
                return true;
            }

            candidate = VisualTreeHelper.GetParent(candidate);
        }

        return false;
    }

    private static TextBox FindTextBox(DependencyObject root, string automationName)
    {
        var textBox = FindDescendant<TextBox>(
            root,
            candidate => AutomationProperties.GetName(candidate) == automationName);
        return Assert.IsAssignableFrom<TextBox>(textBox);
    }

    private static T? FindDescendant<T>(DependencyObject root, Func<T, bool> predicate)
        where T : DependencyObject
    {
        var count = VisualTreeHelper.GetChildrenCount(root);
        for (var i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            if (child is T candidate && predicate(candidate))
            {
                return candidate;
            }

            var match = FindDescendant(child, predicate);
            if (match is not null)
            {
                return match;
            }
        }

        return null;
    }

    private static IEnumerable<T> FindDescendants<T>(DependencyObject root)
        where T : DependencyObject
    {
        var count = VisualTreeHelper.GetChildrenCount(root);
        for (var i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            if (child is T candidate)
            {
                yield return candidate;
            }

            foreach (var nested in FindDescendants<T>(child))
            {
                yield return nested;
            }
        }
    }

    private static List<string> GetVisibleStringSequence(DependencyObject root)
    {
        var textBlocks = FindDescendants<TextBlock>(root)
            .Where(textBlock => textBlock.IsVisible)
            .Select(textBlock => textBlock.Text);
        var contentControls = FindDescendants<ContentControl>(root)
            .Where(control => control.IsVisible && control.Content is string)
            .Select(control => (string)control.Content);

        return textBlocks
            .Concat(contentControls)
            .Where(text => !string.IsNullOrWhiteSpace(text))
            .ToList();
    }

    private static void AssertTextOrder(IList<string> texts, params string[] orderedValues)
    {
        var lastIndex = -1;
        foreach (var value in orderedValues)
        {
            var index = -1;
            for (var i = lastIndex + 1; i < texts.Count; i++)
            {
                if (string.Equals(texts[i], value, StringComparison.Ordinal))
                {
                    index = i;
                    break;
                }
            }

            Assert.True(index >= 0, $"Expected to render '{value}' in vehicle registration review tree.");
            lastIndex = index;
        }
    }

    private static void TypeInto(Window window, TextBox textBox, string text)
    {
        textBox.Clear();
        FocusTextBox(window, textBox);
        SendUnicodeText(textBox, text);
        WaitForLayout();
    }

    private static void ReplaceText(Window window, TextBox textBox, string text)
    {
        FocusTextBox(window, textBox);
        textBox.SelectAll();
        SendUnicodeText(textBox, text);
        WaitForLayout();
    }

    private static void FocusTextBox(Window window, TextBox textBox)
    {
        window.Activate();
        Assert.True(textBox.Focus());
        Assert.Same(textBox, Keyboard.Focus(textBox));
        WaitForLayout();
    }

    private static void SendUnicodeText(TextBox textBox, string text)
    {
        var target = Keyboard.FocusedElement as IInputElement ?? textBox;
        Assert.NotNull(target);
        foreach (var character in text)
        {
            var args = new TextCompositionEventArgs(
                Keyboard.PrimaryDevice,
                new TextComposition(InputManager.Current, target, character.ToString()))
            {
                RoutedEvent = TextCompositionManager.TextInputEvent
            };
            if (target is UIElement uiElement)
            {
                uiElement.RaiseEvent(args);
            }
            else if (target is ContentElement contentElement)
            {
                contentElement.RaiseEvent(args);
            }
            else
            {
                throw new Xunit.Sdk.XunitException("Unable to raise text input for current target.");
            }
        }
    }

    private static void WaitUntil(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow.AddSeconds(3);
        while (!condition() && DateTime.UtcNow < deadline)
        {
            WaitForLayout();
        }

        Assert.True(condition());
    }

    private static void WaitForLayout()
    {
        var frame = new DispatcherFrame();
        Dispatcher.CurrentDispatcher.BeginInvoke(
            DispatcherPriority.Background,
            new Action(() => frame.Continue = false));
        Dispatcher.PushFrame(frame);
    }

    private static void RunOnStaThread(Action action)
    {
        Exception? exception = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (exception is not null)
        {
            ExceptionDispatchInfo.Capture(exception).Throw();
        }
    }

    private sealed class StubBillingService : IBillingService
    {
        public Task<UtilityBillingPreviewDto> PreviewUtilityBillingAsync(UtilityBillingPreviewRequest request, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task UpsertUtilityReadingAsync(UtilityReadingRequest request, CancellationToken ct = default) =>
            Task.CompletedTask;

        public Task<GenerateMonthlyInvoiceResult> GenerateMonthlyInvoicesAsync(GenerateMonthlyInvoiceRequest request, CancellationToken ct = default) =>
            Task.FromResult(new GenerateMonthlyInvoiceResult());

        public Task<IReadOnlyList<InvoiceDto>> GetInvoicesAsync(string? billingPeriod = null, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<InvoiceDto>>(Array.Empty<InvoiceDto>());

        public Task<IReadOnlyList<StudentBillingRowDto>> GetStudentBillingRowsAsync(string? billingPeriod = null, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<StudentBillingRowDto>>(Array.Empty<StudentBillingRowDto>());

        public Task<InvoiceDto?> GetInvoiceAsync(Guid invoiceId, CancellationToken ct = default) =>
            Task.FromResult<InvoiceDto?>(null);

        public Task<InvoiceDto> CreateInvoiceAsync(Guid studentId, Guid roomId, string billingPeriod, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task AddInvoiceItemAsync(Guid invoiceId, InvoiceItemDto item, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task<int> MarkOverdueInvoicesAsync(DateTime asOfDate, CancellationToken ct = default) =>
            Task.FromResult(0);

        public Task AdjustInvoiceAsync(Guid invoiceId, decimal amount, string reason, CancellationToken ct = default) =>
            throw new NotSupportedException();
    }

    private sealed class StubCurrentUser : ICurrentUserService
    {
        public StubCurrentUser(string roleName)
        {
            CurrentUser = new CurrentUserDto
            {
                UserId = Guid.NewGuid(),
                Username = roleName,
                Email = roleName + "@ktx.local",
                FullName = roleName,
                RoleName = roleName
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

    private sealed class StubVehicleService : IVehicleService
    {
        public Task<IReadOnlyList<VehicleRegistrationDto>> GetCurrentStudentVehicleRegistrationsAsync(DateTime? asOfDate = null, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<VehicleRegistrationDto>>(Array.Empty<VehicleRegistrationDto>());

        public Task<VehicleRegistrationDto> RegisterVehicleAsync(CreateVehicleRegistrationRequest request, CancellationToken ct = default) =>
            Task.FromResult(new VehicleRegistrationDto
            {
                Id = Guid.NewGuid(),
                LicensePlate = request.LicensePlate,
                NormalizedPlate = request.LicensePlate,
                MonthCount = request.MonthCount,
                RegisteredAt = DateTime.Today
            });

        public Task ApproveVehicleAsync(Guid registrationId, CancellationToken ct = default) =>
            Task.CompletedTask;

        public Task RejectVehicleAsync(Guid registrationId, string reason, CancellationToken ct = default) =>
            Task.CompletedTask;

        public Task CancelVehicleAsync(Guid registrationId, CancellationToken ct = default) =>
            Task.CompletedTask;
    }

    private sealed class PopulatedVehicleHistoryService : IVehicleService
    {
        public Task<IReadOnlyList<VehicleRegistrationDto>> GetCurrentStudentVehicleRegistrationsAsync(DateTime? asOfDate = null, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<VehicleRegistrationDto>>(
            [
                new VehicleRegistrationDto
                {
                    Id = Guid.NewGuid(),
                    RowNumber = 1,
                    LicensePlate = "59A1-23456",
                    NormalizedPlate = "59A1-23456",
                    MonthCount = 3,
                    Amount = 120000m,
                    RegisteredAt = new DateTime(2023, 10, 15),
                    Status = DormitoryManagement.Domain.Enums.VehicleStatus.Approved,
                    StatusText = "Đã duyệt"
                },
                new VehicleRegistrationDto
                {
                    Id = Guid.NewGuid(),
                    RowNumber = 2,
                    LicensePlate = "59A1-23456",
                    NormalizedPlate = "59A1-23456",
                    MonthCount = 1,
                    Amount = 40000m,
                    RegisteredAt = new DateTime(2023, 9, 10),
                    Status = DormitoryManagement.Domain.Enums.VehicleStatus.Expired,
                    StatusText = "Hết hạn"
                },
                new VehicleRegistrationDto
                {
                    Id = Guid.NewGuid(),
                    RowNumber = 3,
                    LicensePlate = "59A1-23456",
                    NormalizedPlate = "59A1-23456",
                    MonthCount = 1,
                    Amount = 40000m,
                    RegisteredAt = new DateTime(2023, 8, 10),
                    Status = DormitoryManagement.Domain.Enums.VehicleStatus.Expired,
                    StatusText = "Hết hạn"
                }
            ]);

        public Task<VehicleRegistrationDto> RegisterVehicleAsync(CreateVehicleRegistrationRequest request, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task ApproveVehicleAsync(Guid registrationId, CancellationToken ct = default) =>
            Task.CompletedTask;

        public Task RejectVehicleAsync(Guid registrationId, string reason, CancellationToken ct = default) =>
            Task.CompletedTask;

        public Task CancelVehicleAsync(Guid registrationId, CancellationToken ct = default) =>
            Task.CompletedTask;
    }
}









