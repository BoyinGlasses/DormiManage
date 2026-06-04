using System.IO;
using System.Globalization;
using System.Resources;
using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml.Linq;
using DormitoryManagement.Application.Abstractions.Auth;
using DormitoryManagement.Application.DTOs.Auth;
using DormitoryManagement.Application.DTOs.SupportTickets;
using DormitoryManagement.Application.Services.SupportTickets;
using DormitoryManagement.Domain.Constants;
using DormitoryManagement.Domain.Enums;
using DormitoryManagement.WPF.ViewModels.SupportTickets;
using DormitoryManagement.WPF.Views.SupportTickets;

namespace DormitoryManagement.WPF.Tests;

public sealed class SupportTicketViewTests
{
    private static readonly XNamespace XamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";

    [Fact]
    public void Compiled_resources_include_support_ticket_view_and_tokens()
    {
        var keys = GetCompiledResourceKeys();

        Assert.Contains("views/supporttickets/supportticketlistview.baml", keys);
        Assert.Contains("resources/supporttickets.baml", keys);
    }

    [Fact]
    public void Support_ticket_resources_define_reference_student_route_tokens()
    {
        var document = LoadSupportTicketResourceDocument();

        AssertResourceValue(document, "Color", "SupportTicketsColorCanvas", "#F8F9FF");
        AssertResourceValue(document, "Color", "SupportTicketsColorSurface", "#FFFFFF");
        AssertResourceValue(document, "Color", "SupportTicketsColorSurfaceLow", "#EFF4FF");
        AssertResourceValue(document, "Color", "SupportTicketsColorSurfaceContainer", "#E5EEFF");
        AssertResourceValue(document, "Color", "SupportTicketsColorSurfaceHigh", "#DCE9FF");
        AssertResourceValue(document, "Color", "SupportTicketsColorSurfaceHighest", "#D3E4FE");
        AssertResourceValue(document, "Color", "SupportTicketsColorPrimary", "#AB3500");
        AssertResourceValue(document, "Color", "SupportTicketsColorPrimaryContainer", "#FF6B35");
        AssertResourceValue(document, "Color", "SupportTicketsColorSecondary", "#4648D4");
        AssertResourceValue(document, "Color", "SupportTicketsColorTertiary", "#006C49");
        AssertResourceValue(document, "FontFamily", "SupportTicketsHeadlineFontFamily", "pack://application:,,,/DormitoryManagement.WPF;component/Assets/Fonts/PlusJakartaSans/#Plus Jakarta Sans");
        AssertResourceValue(document, "FontFamily", "SupportTicketsBodyFontFamily", "pack://application:,,,/DormitoryManagement.WPF;component/Assets/Fonts/Inter/#Inter");
        AssertResourceValue(document, "Double", "SupportTicketsPageTitleSize", "32");
        AssertResourceValue(document, "Double", "SupportTicketsSectionTitleSize", "18");
        AssertResourceValue(document, "Double", "SupportTicketsMetricSize", "24");
        AssertResourceValue(document, "Double", "SupportTicketsBadgeMinHeight", "24");
        AssertResourceValue(document, "Double", "SupportTicketsActionButtonSize", "32");
        AssertResourceValue(document, "Double", "SupportTicketsDataGridRowMinHeight", "48");
        AssertResourceValue(document, "CornerRadius", "SupportTicketsCardCornerRadius", "16");
        AssertResourceValue(document, "Thickness", "SupportTicketsCardPaddingThickness", "24");
        AssertResourceValue(document, "Thickness", "SupportTicketsDataGridCellPadding", "20,13");
    }

    [Fact]
    public void Support_ticket_view_uses_reference_sections_and_preserves_ticket_field_contracts()
    {
        var xaml = LoadSupportTicketViewDocument().ToString(SaveOptions.DisableFormatting);

        Assert.Contains("Text=\"Yêu cầu hỗ trợ\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"Quản lý và theo dõi các yêu cầu dịch vụ kỹ thuật của bạn.\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"Danh sách yêu cầu gần đây\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"Gửi yêu cầu mới\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"Tổng số yêu cầu\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"Đang xử lý\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"Đã hoàn thành\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Header=\"Mã đơn\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Header=\"Chủ đề\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Header=\"Loại vấn đề\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Header=\"Ngày gửi\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Header=\"Trạng thái\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Header=\"Hành động\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"{Binding TicketFooterSummaryText}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("ConverterParameter=\"TicketReference\"", xaml, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.Name=\"Ticket title\"", xaml, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.Name=\"Ticket description\"", xaml, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.Name=\"Ticket category\"", xaml, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.Name=\"Ticket priority\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Command=\"{Binding ToggleCreateFormCommand}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Command=\"{Binding ToggleFiltersCommand}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("SupportTicketValueConverter", xaml, StringComparison.Ordinal);
        Assert.Contains("SupportTicketsBadgeBorderStyle", xaml, StringComparison.Ordinal);
        Assert.Contains("SupportTicketsActionButtonSize", xaml, StringComparison.Ordinal);
        Assert.Contains("SupportTicketsActionIconSize", xaml, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.Name=\"Xem yêu cầu\"", xaml, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.Name=\"Xóa yêu cầu\"", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("<controls:StatusBadge", xaml, StringComparison.Ordinal);
        Assert.Contains("Command=\"{Binding DataContext.SelectTicketCommand, RelativeSource={RelativeSource AncestorType=UserControl}}\"", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void Support_ticket_default_student_review_state_keeps_optional_surfaces_closed()
    {
        var viewModel = new SupportTicketListViewModel(new StubSupportTicketService(), new StubCurrentUser(RoleNames.Student));

        Assert.False(viewModel.AreFiltersOpen);
        Assert.False(viewModel.IsCreateFormOpen);
        Assert.False(viewModel.IsStaffUser);

        var xaml = LoadSupportTicketViewDocument().ToString(SaveOptions.DisableFormatting);
        Assert.Contains("Visibility=\"Collapsed\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Visibility=\"{Binding AreFiltersOpen, Converter={StaticResource BoolToVisibility}}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Visibility=\"{Binding IsCreateFormOpen, Converter={StaticResource BoolToVisibility}}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Visibility=\"{Binding IsStaffUser, Converter={StaticResource BoolToVisibility}}\"", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void Support_ticket_status_labels_match_approved_meanings()
    {
        var converter = new DormitoryManagement.WPF.Converters.SupportTicketValueConverter();

        Assert.Equal("Chờ xử lý", converter.Convert(SupportTicketStatus.New, typeof(string), null!, CultureInfo.InvariantCulture));
        Assert.Equal("Đang thực hiện", converter.Convert(SupportTicketStatus.Assigned, typeof(string), null!, CultureInfo.InvariantCulture));
        Assert.Equal("Đang thực hiện", converter.Convert(SupportTicketStatus.InProgress, typeof(string), null!, CultureInfo.InvariantCulture));
        Assert.Equal("Đã giải quyết", converter.Convert(SupportTicketStatus.Resolved, typeof(string), null!, CultureInfo.InvariantCulture));
        Assert.Equal("Đã giải quyết", converter.Convert(SupportTicketStatus.Closed, typeof(string), null!, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void Support_ticket_view_can_render_current_review_artifact()
    {
        RunOnStaThread(() =>
        {
            EnsureApplicationResources();
            var repoRoot = FindRepositoryRoot();
            var artifactPath = Path.Combine(repoRoot, ".ai", "artifacts", "support-tickets-wpf-recovered.png");
            Directory.CreateDirectory(Path.GetDirectoryName(artifactPath)!);

            var viewModel = new SupportTicketListViewModel(new StubSupportTicketService(), new StubCurrentUser(RoleNames.Student));
            viewModel.LoadCommand.Execute(null);
            WaitUntil(() => viewModel.HasTickets);

            var view = new SupportTicketListView
            {
                DataContext = viewModel,
                Width = 1440,
                Height = 860
            };

            var host = new Window
            {
                Width = 1440,
                Height = 860,
                WindowStyle = WindowStyle.None,
                ResizeMode = ResizeMode.NoResize,
                ShowInTaskbar = false,
                Content = view,
                Background = Brushes.White
            };

            host.Show();
            WaitForLayout();
            view.Measure(new Size(host.Width, host.Height));
            view.Arrange(new Rect(0, 0, host.Width, host.Height));
            view.UpdateLayout();
            WaitForLayout();
            SaveFrameworkElementAsPng(view, artifactPath);
            host.Close();

            Assert.True(File.Exists(artifactPath));
            Assert.True(new FileInfo(artifactPath).Length > 0);
            Assert.True(File.Exists(Path.Combine(repoRoot, ".ai", "artifacts", "support-tickets-wpf.png")));
        });
    }

    private static HashSet<string> GetCompiledResourceKeys()
    {
        var assembly = typeof(SupportTicketListView).Assembly;
        using var stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.g.resources");
        Assert.NotNull(stream);
        using var reader = new ResourceReader(stream!);
        var keys = new HashSet<string>(StringComparer.Ordinal);
        foreach (System.Collections.DictionaryEntry entry in reader)
        {
            keys.Add((string)entry.Key);
        }

        return keys;
    }

    private static XDocument LoadSupportTicketViewDocument()
    {
        var repoRoot = FindRepositoryRoot();
        var viewPath = Path.Combine(repoRoot, "src", "DormitoryManagement.WPF", "Views", "SupportTickets", "SupportTicketListView.xaml");
        Assert.True(File.Exists(viewPath));
        return XDocument.Load(viewPath);
    }

    private static XDocument LoadSupportTicketResourceDocument()
    {
        var repoRoot = FindRepositoryRoot();
        var viewPath = Path.Combine(repoRoot, "src", "DormitoryManagement.WPF", "Resources", "SupportTickets.xaml");
        Assert.True(File.Exists(viewPath));
        return XDocument.Load(viewPath);
    }

    private static void AssertResourceValue(XDocument document, string localName, string key, string expectedValue)
    {
        var element = document.Root!
            .Elements()
            .FirstOrDefault(candidate =>
                string.Equals(candidate.Name.LocalName, localName, StringComparison.Ordinal)
                && string.Equals(candidate.Attribute(XamlNamespace + "Key")?.Value, key, StringComparison.Ordinal));

        Assert.NotNull(element);
        Assert.Equal(expectedValue, element!.Value.Trim());
    }

    private static void SaveFrameworkElementAsPng(FrameworkElement element, string path)
    {
        element.UpdateLayout();
        var width = Math.Max(1, (int)Math.Ceiling(element.ActualWidth));
        var height = Math.Max(1, (int)Math.Ceiling(element.ActualHeight));
        var renderTarget = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        renderTarget.Render(element);

        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(renderTarget));

        using var stream = File.Create(path);
        encoder.Save(stream);
    }

    private static void EnsureApplicationResources()
    {
        var themeUri = new Uri("/DormitoryManagement.WPF;component/Resources/Themes.xaml", UriKind.Relative);

        if (System.Windows.Application.Current is not null)
        {
            AttachThemeResources(System.Windows.Application.Current, themeUri);
            return;
        }

        try
        {
            var application = new System.Windows.Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            AttachThemeResources(application, themeUri);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Cannot create more than one System.Windows.Application instance", StringComparison.Ordinal))
        {
            if (System.Windows.Application.Current is not null)
            {
                AttachThemeResources(System.Windows.Application.Current, themeUri);
            }
        }
    }

    private static void AttachThemeResources(System.Windows.Application application, Uri themeUri)
    {
        var hasTheme = application.Resources.MergedDictionaries.Any(dictionary => Equals(dictionary.Source, themeUri));
        if (!hasTheme)
        {
            application.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = themeUri });
        }
    }

    private static void WaitForLayout()
    {
        var frame = new DispatcherFrame();
        Dispatcher.CurrentDispatcher.BeginInvoke(
            DispatcherPriority.Background,
            new DispatcherOperationCallback(_ =>
            {
                frame.Continue = false;
                return null;
            }),
            null);
        Dispatcher.PushFrame(frame);
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

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            var projectFile = Path.Combine(current.FullName, "src", "DormitoryManagement.WPF", "DormitoryManagement.WPF.csproj");
            if (File.Exists(projectFile))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new Xunit.Sdk.XunitException("Could not locate repository root from test output directory.");
    }

    private sealed class StubCurrentUser : ICurrentUserService
    {
        private readonly string _roleName;

        public StubCurrentUser(string roleName)
        {
            _roleName = roleName;
            CurrentUser = new CurrentUserDto
            {
                UserId = Guid.NewGuid(),
                Username = "student",
                Email = "student@ktx.local",
                FullName = "Nguyễn Văn A",
                RoleName = roleName,
                StudentId = Guid.NewGuid()
            };
        }

        public CurrentUserDto? CurrentUser { get; }
        public Guid? UserId => CurrentUser?.UserId;
        public string? UserName => CurrentUser?.Username;
        public string? Email => CurrentUser?.Email;
        public string? FullName => CurrentUser?.FullName;
        public IReadOnlyCollection<string> Roles => [_roleName];
        public bool IsAuthenticated => true;
        public bool IsInRole(string roleName) => string.Equals(roleName, _roleName, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class StubSupportTicketService : ISupportTicketService
    {
        private readonly List<SupportTicketDto> _tickets =
        [
            new() { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Title = "Sửa bóng đèn phòng tắm", Description = "A", Category = SupportTicketCategory.Maintenance, Priority = PriorityLevel.Medium, Status = SupportTicketStatus.New, CreatedAt = new DateTime(2024, 10, 24) },
            new() { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Title = "Wifi yếu khu vực giường tầng 2", Description = "B", Category = SupportTicketCategory.Account, Priority = PriorityLevel.High, Status = SupportTicketStatus.InProgress, CreatedAt = new DateTime(2024, 10, 22) },
            new() { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Title = "Vòi nước bồn rửa mặt bị rỉ", Description = "C", Category = SupportTicketCategory.Maintenance, Priority = PriorityLevel.Low, Status = SupportTicketStatus.Resolved, CreatedAt = new DateTime(2024, 10, 15) },
            new() { Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), Title = "Đăng ký thêm định mức nước", Description = "D", Category = SupportTicketCategory.Other, Priority = PriorityLevel.Low, Status = SupportTicketStatus.Resolved, CreatedAt = new DateTime(2024, 10, 10) }
        ];

        public Task<IReadOnlyList<SupportTicketDto>> GetTicketsAsync(CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<SupportTicketDto>>(_tickets.ToArray());

        public Task<SupportTicketDto?> GetTicketAsync(Guid ticketId, CancellationToken ct = default) =>
            Task.FromResult(_tickets.FirstOrDefault(ticket => ticket.Id == ticketId));

        public Task<SupportTicketDto> CreateTicketAsync(CreateSupportTicketRequest request, CancellationToken ct = default)
        {
            var created = new SupportTicketDto
            {
                Id = Guid.NewGuid(),
                StudentId = request.StudentId,
                Title = request.Title,
                Description = request.Description,
                Category = request.Category,
                Priority = request.Priority,
                Status = SupportTicketStatus.New,
                CreatedAt = new DateTime(2026, 6, 4)
            };
            _tickets.Insert(0, created);
            return Task.FromResult(created);
        }

        public Task AssignTicketAsync(Guid ticketId, Guid managerId, CancellationToken ct = default) => Task.CompletedTask;
        public Task AddResponseAsync(Guid ticketId, string message, CancellationToken ct = default) => Task.CompletedTask;
        public Task UpdateStatusAsync(UpdateSupportTicketStatusRequest request, CancellationToken ct = default) => Task.CompletedTask;
        public Task CloseTicketAsync(Guid ticketId, CancellationToken ct = default) => Task.CompletedTask;
    }
}


