using System.IO;
using System.Globalization;
using System.Resources;
using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
        AssertResourceValue(document, "Double", "SupportTicketsPageMaxWidth", "1440");
        AssertResourceValue(document, "Double", "SupportTicketsContentMaxWidth", "1376");
        AssertResourceValue(document, "Double", "SupportTicketsPageTitleSize", "32");
        AssertResourceValue(document, "Double", "SupportTicketsSectionTitleSize", "18");
        AssertResourceValue(document, "Double", "SupportTicketsMetricSize", "24");
        AssertResourceValue(document, "Double", "SupportTicketsSummaryIconSize", "48");
        AssertResourceValue(document, "Double", "SupportTicketsBadgeMinHeight", "24");
        AssertResourceValue(document, "Double", "SupportTicketsActionButtonSize", "32");
        AssertResourceValue(document, "Double", "SupportTicketsTableHeaderHeight", "48");
        AssertResourceValue(document, "Double", "SupportTicketsTableRowMinHeight", "48");
        AssertResourceValue(document, "Double", "SupportTicketsTableMinHeight", "240");
        AssertResourceValue(document, "CornerRadius", "SupportTicketsCardCornerRadius", "16");
        AssertResourceValue(document, "Thickness", "SupportTicketsCardPaddingThickness", "24");
        AssertResourceValue(document, "Thickness", "SupportTicketsTableCellPadding", "24,16");
    }

    [Fact]
    public void Support_ticket_view_uses_reference_sections_and_preserves_ticket_field_contracts()
    {
        var xaml = LoadSupportTicketViewDocument().ToString(SaveOptions.DisableFormatting);

        Assert.Contains("Text=\"Yêu cầu hỗ trợ\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"Quản lý và theo dõi các yêu cầu dịch vụ kỹ thuật của bạn.\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"Danh sách yêu cầu gần đây\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"Gửi yêu cầu mới\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"TỔNG SỐ YÊU CẦU\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"ĐANG XỬ LÝ\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"ĐÃ HOÀN THÀNH\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"MÃ ĐƠN\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"CHỦ ĐỀ\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"LOẠI VẤN ĐỀ\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"NGÀY GỬI\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"TRẠNG THÁI\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"HÀNH ĐỘNG\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"{Binding TicketFooterSummaryText}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("ConverterParameter=\"TicketReference\"", xaml, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.Name=\"Chủ đề yêu cầu\"", xaml, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.Name=\"Mô tả yêu cầu\"", xaml, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.Name=\"Loại vấn đề yêu cầu\"", xaml, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.Name=\"Mức độ ưu tiên yêu cầu\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Command=\"{Binding ToggleCreateFormCommand}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Command=\"{Binding ToggleFiltersCommand}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("SupportTicketValueConverter", xaml, StringComparison.Ordinal);
        Assert.Contains("SupportTicketsBadgeBorderStyle", xaml, StringComparison.Ordinal);
        Assert.Contains("SupportTicketsActionButtonSize", xaml, StringComparison.Ordinal);
        Assert.Contains("SupportTicketsActionIconSize", xaml, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.Name=\"Xem yêu cầu\"", xaml, StringComparison.Ordinal);
        Assert.Contains("AutomationProperties.Name=\"Xóa yêu cầu\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Background=\"{StaticResource SupportTicketsSurfaceBrush}\" BorderBrush=\"{StaticResource SupportTicketsSurfaceContainerBrush}\" BorderThickness=\"0,0,0,1\" Padding=\"24\"", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("Foreground=\"#FFBA1A1A\"", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("<controls:StatusBadge", xaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"SupportTicketsTableHeaderGrid\"", xaml, StringComparison.Ordinal);
        Assert.Contains("x:Name=\"SupportTicketsTableRows\"", xaml, StringComparison.Ordinal);
        Assert.Contains("HorizontalAlignment=\"Stretch\" ItemsSource=\"{Binding Tickets}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("<ItemsControl.ItemContainerStyle>", xaml, StringComparison.Ordinal);
        Assert.Contains("<Setter Property=\"HorizontalAlignment\" Value=\"Stretch\" />", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("Width=\"{StaticResource SupportTicketsContentWidth}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("MaxWidth=\"{StaticResource SupportTicketsContentMaxWidth}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Property=\"Margin\" Value=\"24,16\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Grid.Column=\"0\" Padding=\"{StaticResource SupportTicketsTableCellPadding}\" Background=\"Transparent\"", xaml, StringComparison.Ordinal);
        Assert.Contains("MinHeight=\"{StaticResource SupportTicketsTableMinHeight}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Command=\"{Binding PreviousPageCommand}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Command=\"{Binding NextPageCommand}\"", xaml, StringComparison.Ordinal);
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
    public void Support_ticket_review_fixture_matches_canonical_reference_sample()
    {
        RunOnStaThread(() =>
        {
            EnsureApplicationResources();

            var viewModel = new SupportTicketListViewModel(new CanonicalScreenshotSupportTicketService(), new StubCurrentUser(RoleNames.Student));
            viewModel.LoadCommand.Execute(null);
            WaitUntil(() => viewModel.HasTickets);

            Assert.Equal("12", viewModel.TotalTicketCountText);
            Assert.Equal("3", viewModel.OpenTicketCountText);
            Assert.Equal("9", viewModel.ResolvedTicketCountText);
            Assert.Equal("Hiển thị 1-4 trên 12 yêu cầu", viewModel.TicketFooterSummaryText);

            var converter = new DormitoryManagement.WPF.Converters.SupportTicketValueConverter();
            var visibleReferences = viewModel.Tickets
                .Select(ticket => converter.Convert(ticket, typeof(string), "TicketReference", CultureInfo.InvariantCulture))
                .Cast<string>()
                .ToArray();
            var visibleCategories = viewModel.Tickets
                .Select(ticket => converter.Convert(ticket.Category, typeof(string), null!, CultureInfo.InvariantCulture))
                .Cast<string>()
                .ToArray();

            Assert.Equal(["#SP-2024-089", "#SP-2024-085", "#SP-2024-072", "#SP-2024-068"], visibleReferences);
            Assert.Equal(["Cơ sở vật chất", "Kỹ thuật", "Cơ sở vật chất", "Dịch vụ"], visibleCategories);
        });
    }

    [Fact]
    public void Support_ticket_resources_match_canonical_density_tokens()
    {
        var document = LoadSupportTicketResourceDocument();

        AssertStyleSetterValue(document, "SupportTicketsSummaryLabelTextStyle", "FontSize", "12");
        AssertStyleSetterValue(document, "SupportTicketsSummaryLabelTextStyle", "FontWeight", "Medium");
        AssertStyleSetterValue(document, "SupportTicketsBadgeTextStyle", "FontSize", "12");
        AssertStyleSetterValue(document, "SupportTicketsBadgeTextStyle", "FontWeight", "Medium");
        AssertStyleSetterValue(document, "SupportTicketsTableHeaderTextStyle", "FontSize", "12");
        AssertStyleSetterValue(document, "SupportTicketsTableHeaderTextStyle", "FontWeight", "Medium");
        AssertStyleSetterValue(document, "SupportTicketsTableCellTextStyle", "FontSize", "14");
        AssertStyleSetterValue(document, "SupportTicketsTableCellTextStyle", "LineHeight", "22");
    }

    [Fact]
    public void Support_ticket_review_artifact_contract_uses_reference_viewport_and_dedicated_outputs()
    {
        RunOnStaThread(() =>
        {
            EnsureApplicationResources();
            var repoRoot = FindRepositoryRoot();
            var strictArtifactPath = Path.Combine(repoRoot, ".ai", "artifacts", "support-tickets-wpf-recovered.png");
            var listArtifactPath = Path.Combine(repoRoot, ".ai", "artifacts", "support-ticket-list-card.png");
            var rowsArtifactPath = Path.Combine(repoRoot, ".ai", "artifacts", "support-ticket-list-rows.png");
            GenerateSupportTicketArtifacts(new CanonicalScreenshotSupportTicketService());

            var bitmap = LoadBitmap(strictArtifactPath);

            Assert.Equal(512, bitmap.PixelWidth);
            Assert.Equal(377, bitmap.PixelHeight);
            Assert.True(File.Exists(listArtifactPath));
            Assert.True(File.Exists(rowsArtifactPath));
        });
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

            GenerateSupportTicketArtifacts(new CanonicalScreenshotSupportTicketService());

            Assert.True(File.Exists(artifactPath));
            Assert.True(new FileInfo(artifactPath).Length > 0);
            Assert.True(File.Exists(Path.Combine(repoRoot, ".ai", "artifacts", "support-tickets-wpf.png")));
            Assert.True(File.Exists(Path.Combine(repoRoot, ".ai", "artifacts", "support-tickets-wpf-desktop.png")));
        });
    }

    [Fact]
    public void Support_ticket_view_renders_without_root_horizontal_overflow_on_narrow_desktop_host()
    {
        RunOnStaThread(() =>
        {
            EnsureApplicationResources();

            var viewModel = new SupportTicketListViewModel(new StubSupportTicketService(), new StubCurrentUser(RoleNames.Student));
            viewModel.LoadCommand.Execute(null);
            WaitUntil(() => viewModel.HasTickets);

            var view = new SupportTicketListView
            {
                DataContext = viewModel,
                Width = 1120,
                Height = 820
            };

            var host = CreateHost(view, 1120, 820);
            host.Show();
            WaitForLayout();

            var scrollViewer = Assert.IsType<ScrollViewer>(view.FindName("SupportTicketPageScrollViewer"));
            var contentRoot = Assert.IsType<Grid>(view.FindName("ResponsiveContentRoot"));

            Assert.Equal(Visibility.Collapsed, scrollViewer.ComputedHorizontalScrollBarVisibility);
            Assert.True(double.IsNaN(contentRoot.Width));
            Assert.True(contentRoot.ActualWidth <= scrollViewer.ViewportWidth + 1);

            host.Close();
        });
    }

    [Fact]
    public void Support_ticket_recent_list_table_stretches_nearly_full_card_width()
    {
        RunOnStaThread(() =>
        {
            EnsureApplicationResources();

            var viewModel = new SupportTicketListViewModel(new StubSupportTicketService(), new StubCurrentUser(RoleNames.Student));
            viewModel.LoadCommand.Execute(null);
            WaitUntil(() => viewModel.HasTickets);

            var view = new SupportTicketListView
            {
                DataContext = viewModel,
                Width = 1720,
                Height = 820
            };

            var host = CreateHost(view, 1720, 820);
            host.Show();
            WaitForLayout();

            var listCard = Assert.IsType<Border>(view.FindName("SupportTicketsListCard"));
            var headerGrid = Assert.IsType<Grid>(view.FindName("SupportTicketsTableHeaderGrid"));
            var rowsControl = Assert.IsType<ItemsControl>(view.FindName("SupportTicketsTableRows"));

            Assert.True(
                headerGrid.ActualWidth >= listCard.ActualWidth - 4,
                $"Header grid width was {headerGrid.ActualWidth:F1}px while list card width was {listCard.ActualWidth:F1}px; table should stretch nearly the full card width.");
            Assert.True(
                rowsControl.ActualWidth >= listCard.ActualWidth - 4,
                $"Rows control width was {rowsControl.ActualWidth:F1}px while list card width was {listCard.ActualWidth:F1}px; rows should stretch nearly the full card width.");

            host.Close();
        });
    }

    [Fact]
    public void Support_ticket_table_allocates_at_least_50_percent_of_width_to_right_side_columns()
    {
        RunOnStaThread(() =>
        {
            EnsureApplicationResources();

            var viewModel = new SupportTicketListViewModel(new CanonicalScreenshotSupportTicketService(), new StubCurrentUser(RoleNames.Student));
            viewModel.LoadCommand.Execute(null);
            WaitUntil(() => viewModel.HasTickets);

            var view = new SupportTicketListView
            {
                DataContext = viewModel,
                Width = 1376,
                Height = 820
            };

            var host = CreateHost(view, 1376, 820);
            host.Show();
            WaitForLayout();

            var headerGrid = Assert.IsType<Grid>(view.FindName("SupportTicketsTableHeaderGrid"));
            var totalWidth = headerGrid.ColumnDefinitions.Sum(column => column.ActualWidth);
            var rightSideWidth = headerGrid.ColumnDefinitions.Skip(3).Sum(column => column.ActualWidth);
            var rightSideShare = rightSideWidth / totalWidth;

            Assert.True(
                rightSideShare >= 0.50,
                $"Right-side width share was {rightSideShare:P1}; expected at least 50.0% to avoid the current left-heavy table composition.");

            host.Close();
        });
    }

    [Fact]
    public void Support_ticket_recent_list_rows_stretch_across_the_table_width_even_with_a_single_ticket()
    {
        RunOnStaThread(() =>
        {
            EnsureApplicationResources();

            var viewModel = new SupportTicketListViewModel(new SingleTicketSupportTicketService(), new StubCurrentUser(RoleNames.Student));
            viewModel.LoadCommand.Execute(null);
            WaitUntil(() => viewModel.HasTickets);

            var view = new SupportTicketListView
            {
                DataContext = viewModel,
                Width = 1720,
                Height = 820
            };

            var host = CreateHost(view, 1720, 820);
            host.Show();
            WaitForLayout();

            var rowsControl = Assert.IsType<ItemsControl>(view.FindName("SupportTicketsTableRows"));
            var presenter = Assert.IsType<ContentPresenter>(rowsControl.ItemContainerGenerator.ContainerFromIndex(0));
            var rowGrid = FindDescendant<Grid>(presenter);

            Assert.NotNull(rowGrid);
            Assert.True(
                rowGrid!.ActualWidth >= rowsControl.ActualWidth - 4,
                $"Row grid width was {rowGrid.ActualWidth:F1}px while rows control width was {rowsControl.ActualWidth:F1}px; single-ticket rows should still stretch across the table width.");

            host.Close();
        });
    }

    [Fact]
    public void Support_ticket_table_keeps_columns_aligned_and_trims_long_content_under_constrained_width()
    {
        RunOnStaThread(() =>
        {
            EnsureApplicationResources();

            var viewModel = new SupportTicketListViewModel(new LongContentSupportTicketService(), new StubCurrentUser(RoleNames.Student));
            viewModel.LoadCommand.Execute(null);
            WaitUntil(() => viewModel.HasTickets);

            var view = new SupportTicketListView
            {
                DataContext = viewModel,
                Width = 1120,
                Height = 820
            };

            var host = CreateHost(view, 1120, 820);
            host.Show();
            WaitForLayout();

            var headerGrid = Assert.IsType<Grid>(view.FindName("SupportTicketsTableHeaderGrid"));
            var rowsControl = Assert.IsType<ItemsControl>(view.FindName("SupportTicketsTableRows"));
            var presenter = Assert.IsType<ContentPresenter>(rowsControl.ItemContainerGenerator.ContainerFromIndex(0));
            var rowGrid = FindDescendant<Grid>(presenter);
            Assert.NotNull(rowGrid);
            Assert.Equal(6, headerGrid.ColumnDefinitions.Count);
            Assert.Equal(6, rowGrid!.ColumnDefinitions.Count);

            for (var index = 0; index < 6; index++)
            {
                Assert.InRange(Math.Abs(headerGrid.ColumnDefinitions[index].ActualWidth - rowGrid.ColumnDefinitions[index].ActualWidth), 0, 1.5);
            }

            var titleBorder = FindDescendant<Border>(rowGrid, border => Grid.GetColumn(border) == 1);
            Assert.NotNull(titleBorder);
            var titleText = FindDescendant<TextBlock>(titleBorder!);
            Assert.NotNull(titleText);
            Assert.Equal(TextTrimming.CharacterEllipsis, titleText!.TextTrimming);

            host.Close();
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

    private static void AssertStyleSetterValue(XDocument document, string styleKey, string property, string expectedValue)
    {
        var style = document.Root!
            .Elements()
            .FirstOrDefault(candidate =>
                string.Equals(candidate.Name.LocalName, "Style", StringComparison.Ordinal)
                && string.Equals(candidate.Attribute(XamlNamespace + "Key")?.Value, styleKey, StringComparison.Ordinal));

        Assert.NotNull(style);

        var setter = style!
            .Elements()
            .FirstOrDefault(candidate =>
                string.Equals(candidate.Name.LocalName, "Setter", StringComparison.Ordinal)
                && string.Equals(candidate.Attribute("Property")?.Value, property, StringComparison.Ordinal));

        Assert.NotNull(setter);
        Assert.Equal(expectedValue, setter!.Attribute("Value")?.Value);
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

    private static void GenerateSupportTicketArtifacts(ISupportTicketService service)
    {
        EnsureApplicationResources();

        var repoRoot = FindRepositoryRoot();
        var artifactsDirectory = Path.Combine(repoRoot, ".ai", "artifacts");
        Directory.CreateDirectory(artifactsDirectory);

        var desktopArtifactPath = Path.Combine(artifactsDirectory, "support-tickets-wpf-desktop.png");
        var listArtifactPath = Path.Combine(artifactsDirectory, "support-ticket-list-card.png");
        var rowsArtifactPath = Path.Combine(artifactsDirectory, "support-ticket-list-rows.png");
        var recoveredArtifactPath = Path.Combine(artifactsDirectory, "support-tickets-wpf-recovered.png");
        var fullDiffArtifactPath = Path.Combine(artifactsDirectory, "support-ticket-full-diff.png");
        var listDiffArtifactPath = Path.Combine(artifactsDirectory, "support-ticket-list-diff.png");
        var rowsDiffArtifactPath = Path.Combine(artifactsDirectory, "support-ticket-list_rows-diff.png");

        RenderSupportTicketSurface(service, 1440, 860, desktopArtifactPath, listArtifactPath, rowsArtifactPath);
        SaveReferenceViewportArtifact(desktopArtifactPath, recoveredArtifactPath, 512, 377);
        SaveDifferenceArtifacts(recoveredArtifactPath, listArtifactPath, rowsArtifactPath, fullDiffArtifactPath, listDiffArtifactPath, rowsDiffArtifactPath);
    }

    private static void SaveDifferenceArtifacts(
        string recoveredArtifactPath,
        string listArtifactPath,
        string rowsArtifactPath,
        string fullDiffArtifactPath,
        string listDiffArtifactPath,
        string rowsDiffArtifactPath)
    {
        var repoRoot = FindRepositoryRoot();
        var referencePath = Path.Combine(repoRoot, "stitch-downloads", "Dorm", "f800aa1e608c47bba0667fef296a6832", "Quan-ly-yeu-cau-ho-tro-DormManagement.png");
        var reference = LoadBitmap(referencePath);
        var recovered = LoadBitmap(recoveredArtifactPath);
        var list = LoadBitmap(listArtifactPath);
        var rows = LoadBitmap(rowsArtifactPath);

        var referenceList = new CroppedBitmap(reference, new Int32Rect(17, 160, 478, 198));
        var referenceRows = new CroppedBitmap(reference, new Int32Rect(17, 196, 478, 124));

        SaveDifferenceArtifact(reference, recovered, fullDiffArtifactPath);
        SaveDifferenceArtifact(referenceList, list, listDiffArtifactPath);
        SaveDifferenceArtifact(referenceRows, rows, rowsDiffArtifactPath);
    }

    private static void SaveDifferenceArtifact(BitmapSource reference, BitmapSource candidate, string outputPath)
    {
        var normalizedCandidate = NormalizeBitmap(candidate, reference.PixelWidth, reference.PixelHeight);
        var stride = reference.PixelWidth * 4;
        var referencePixels = new byte[reference.PixelHeight * stride];
        var candidatePixels = new byte[reference.PixelHeight * stride];
        var diffPixels = new byte[reference.PixelHeight * stride];

        reference.CopyPixels(referencePixels, stride, 0);
        normalizedCandidate.CopyPixels(candidatePixels, stride, 0);

        for (var index = 0; index < diffPixels.Length; index += 4)
        {
            diffPixels[index] = (byte)Math.Abs(referencePixels[index] - candidatePixels[index]);
            diffPixels[index + 1] = (byte)Math.Abs(referencePixels[index + 1] - candidatePixels[index + 1]);
            diffPixels[index + 2] = (byte)Math.Abs(referencePixels[index + 2] - candidatePixels[index + 2]);
            diffPixels[index + 3] = 255;
        }

        var diff = BitmapSource.Create(reference.PixelWidth, reference.PixelHeight, 96, 96, PixelFormats.Bgra32, null, diffPixels, stride);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(diff));

        using var stream = File.Create(outputPath);
        encoder.Save(stream);
    }

    private static BitmapSource NormalizeBitmap(BitmapSource source, int width, int height)
    {
        if (source.PixelWidth == width && source.PixelHeight == height)
        {
            return source;
        }

        return new TransformedBitmap(source, new ScaleTransform(width / (double)source.PixelWidth, height / (double)source.PixelHeight));
    }

    private static void SaveReferenceViewportArtifact(string sourcePath, string outputPath, int targetWidth, int targetHeight)
    {
        var bitmap = LoadBitmap(sourcePath);
        var crop = GetCenteredAspectCrop(bitmap.PixelWidth, bitmap.PixelHeight, targetWidth, targetHeight);
        var cropped = new CroppedBitmap(bitmap, crop);
        var scale = new ScaleTransform(targetWidth / (double)crop.Width, targetHeight / (double)crop.Height);
        var scaled = new TransformedBitmap(cropped, scale);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(scaled));

        using var stream = File.Create(outputPath);
        encoder.Save(stream);
    }

    private static Int32Rect GetCenteredAspectCrop(int sourceWidth, int sourceHeight, int targetWidth, int targetHeight)
    {
        var targetAspect = targetWidth / (double)targetHeight;
        var sourceAspect = sourceWidth / (double)sourceHeight;

        if (sourceAspect > targetAspect)
        {
            var cropWidth = (int)Math.Round(sourceHeight * targetAspect);
            var cropX = 0;
            return new Int32Rect(cropX, 0, cropWidth, sourceHeight);
        }

        var cropHeight = (int)Math.Round(sourceWidth / targetAspect);
        var cropY = (sourceHeight - cropHeight) / 2;
        return new Int32Rect(0, cropY, sourceWidth, cropHeight);
    }

    private static void RenderSupportTicketSurface(
        ISupportTicketService service,
        double width,
        double height,
        string artifactPath,
        string? listArtifactPath,
        string? rowsArtifactPath)
    {
        var viewModel = new SupportTicketListViewModel(service, new StubCurrentUser(RoleNames.Student));
        viewModel.LoadCommand.Execute(null);
        WaitUntil(() => viewModel.HasTickets);

        var view = new SupportTicketListView
        {
            DataContext = viewModel,
            Width = width,
            Height = height
        };

        var host = CreateHost(view, width, height);
        host.Show();
        WaitForLayout();
        view.Measure(new Size(host.Width, host.Height));
        view.Arrange(new Rect(0, 0, host.Width, host.Height));
        view.UpdateLayout();
        WaitForLayout();
        SaveFrameworkElementAsPng(view, artifactPath);

        if (!string.IsNullOrWhiteSpace(listArtifactPath) && !string.IsNullOrWhiteSpace(rowsArtifactPath))
        {
            var listCard = Assert.IsType<Border>(view.FindName("SupportTicketsListCard"));
            var rowsControl = Assert.IsType<ItemsControl>(view.FindName("SupportTicketsTableRows"));
            var rowPresenter = Assert.IsType<ContentPresenter>(rowsControl.ItemContainerGenerator.ContainerFromIndex(0));
            var rowGrid = FindDescendant<Grid>(rowPresenter);

            Assert.NotNull(rowGrid);
            SaveCroppedRegion(artifactPath, GetElementBoundsInAncestor(listCard, view), listArtifactPath!);
            SaveCroppedRegion(artifactPath, GetElementBoundsInAncestor(rowGrid!, view), rowsArtifactPath!);
        }

        host.Close();
    }

    private static Rect GetElementBoundsInAncestor(FrameworkElement element, Visual ancestor)
    {
        var topLeft = element.TransformToAncestor(ancestor).Transform(new Point(0, 0));
        return new Rect(topLeft, new Size(element.ActualWidth, element.ActualHeight));
    }

    private static void SaveCroppedRegion(string sourcePath, Rect bounds, string outputPath)
    {
        var bitmap = LoadBitmap(sourcePath);
        var x = Math.Max(0, (int)Math.Floor(bounds.X));
        var y = Math.Max(0, (int)Math.Floor(bounds.Y));
        var width = Math.Max(1, Math.Min(bitmap.PixelWidth - x, (int)Math.Ceiling(bounds.Width)));
        var height = Math.Max(1, Math.Min(bitmap.PixelHeight - y, (int)Math.Ceiling(bounds.Height)));

        var cropped = new CroppedBitmap(bitmap, new Int32Rect(x, y, width, height));
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(cropped));

        using var stream = File.Create(outputPath);
        encoder.Save(stream);
    }

    private static BitmapImage LoadBitmap(string path)
    {
        using var stream = File.OpenRead(path);
        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.StreamSource = stream;
        bitmap.EndInit();
        bitmap.Freeze();
        return bitmap;
    }

    private static Window CreateHost(FrameworkElement view, double width, double height) =>
        new()
        {
            Width = width,
            Height = height,
            WindowStyle = WindowStyle.None,
            ResizeMode = ResizeMode.NoResize,
            ShowInTaskbar = false,
            Content = view,
            Background = Brushes.White
        };


    private static T? FindDescendant<T>(DependencyObject root)
        where T : DependencyObject
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is T match)
            {
                return match;
            }

            var nested = FindDescendant<T>(child);
            if (nested is not null)
            {
                return nested;
            }
        }

        return null;
    }

    private static T? FindDescendant<T>(DependencyObject root, Func<T, bool> predicate)
        where T : DependencyObject
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is T match && predicate(match))
            {
                return match;
            }

            var nested = FindDescendant(child, predicate);
            if (nested is not null)
            {
                return nested;
            }
        }

        return null;
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
            new() { Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), Title = "Đăng ký thêm định mức nước", Description = "D", Category = SupportTicketCategory.Other, Priority = PriorityLevel.Low, Status = SupportTicketStatus.Resolved, CreatedAt = new DateTime(2024, 10, 10) },
            new() { Id = Guid.Parse("55555555-5555-5555-5555-555555555555"), Title = "Kiểm tra ổ cắm điện", Description = "E", Category = SupportTicketCategory.Maintenance, Priority = PriorityLevel.Medium, Status = SupportTicketStatus.New, CreatedAt = new DateTime(2024, 10, 8) },
            new() { Id = Guid.Parse("66666666-6666-6666-6666-666666666666"), Title = "Hỗ trợ tài khoản cổng nội trú", Description = "F", Category = SupportTicketCategory.Account, Priority = PriorityLevel.Medium, Status = SupportTicketStatus.Assigned, CreatedAt = new DateTime(2024, 10, 6) },
            new() { Id = Guid.Parse("77777777-7777-7777-7777-777777777777"), Title = "Rò rỉ nước khu vệ sinh", Description = "G", Category = SupportTicketCategory.Maintenance, Priority = PriorityLevel.High, Status = SupportTicketStatus.InProgress, CreatedAt = new DateTime(2024, 10, 4) },
            new() { Id = Guid.Parse("88888888-8888-8888-8888-888888888888"), Title = "Báo mất thẻ xe", Description = "H", Category = SupportTicketCategory.Security, Priority = PriorityLevel.High, Status = SupportTicketStatus.Resolved, CreatedAt = new DateTime(2024, 10, 2) },
            new() { Id = Guid.Parse("99999999-9999-9999-9999-999999999999"), Title = "Hỏi về phí dịch vụ", Description = "I", Category = SupportTicketCategory.Billing, Priority = PriorityLevel.Low, Status = SupportTicketStatus.New, CreatedAt = new DateTime(2024, 9, 30) },
            new() { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Title = "Đề nghị thay vòi sen", Description = "J", Category = SupportTicketCategory.Maintenance, Priority = PriorityLevel.Medium, Status = SupportTicketStatus.Resolved, CreatedAt = new DateTime(2024, 9, 28) },
            new() { Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Title = "Khiếu nại tiếng ồn sau giờ nghỉ", Description = "K", Category = SupportTicketCategory.Complaint, Priority = PriorityLevel.Medium, Status = SupportTicketStatus.Assigned, CreatedAt = new DateTime(2024, 9, 26) },
            new() { Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"), Title = "Đăng ký chỗ để xe bổ sung", Description = "L", Category = SupportTicketCategory.Vehicle, Priority = PriorityLevel.Low, Status = SupportTicketStatus.Closed, CreatedAt = new DateTime(2024, 9, 24) }
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

    private sealed class CanonicalScreenshotSupportTicketService : ISupportTicketService
    {
        private readonly IReadOnlyList<SupportTicketDto> _tickets =
        [
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000001"), StudentCode = "#SP-2024-089", Title = "Sửa bóng đèn phòng tắm", Description = "A", Category = SupportTicketCategory.Maintenance, Priority = PriorityLevel.Medium, Status = SupportTicketStatus.New, CreatedAt = new DateTime(2024, 10, 24) },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000002"), StudentCode = "#SP-2024-085", Title = "Wifi yếu khu vực giường tầng 2", Description = "B", Category = SupportTicketCategory.Account, Priority = PriorityLevel.High, Status = SupportTicketStatus.Assigned, CreatedAt = new DateTime(2024, 10, 22) },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000003"), StudentCode = "#SP-2024-072", Title = "Vòi nước bồn rửa mặt bị rỉ", Description = "C", Category = SupportTicketCategory.Maintenance, Priority = PriorityLevel.Low, Status = SupportTicketStatus.Resolved, CreatedAt = new DateTime(2024, 10, 15) },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000004"), StudentCode = "#SP-2024-068", Title = "Đăng ký thêm định mức nước", Description = "D", Category = SupportTicketCategory.Other, Priority = PriorityLevel.Low, Status = SupportTicketStatus.Closed, CreatedAt = new DateTime(2024, 10, 10) },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000005"), StudentCode = "#SP-2024-061", Title = "Điều chỉnh lịch bảo trì quạt trần", Description = "E", Category = SupportTicketCategory.Maintenance, Priority = PriorityLevel.Medium, Status = SupportTicketStatus.Resolved, CreatedAt = new DateTime(2024, 10, 8) },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000006"), StudentCode = "#SP-2024-059", Title = "Kiểm tra lại áp lực nước", Description = "F", Category = SupportTicketCategory.Other, Priority = PriorityLevel.Medium, Status = SupportTicketStatus.Resolved, CreatedAt = new DateTime(2024, 10, 6) },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000007"), StudentCode = "#SP-2024-052", Title = "Vệ sinh khu vực hành lang", Description = "G", Category = SupportTicketCategory.Other, Priority = PriorityLevel.Low, Status = SupportTicketStatus.Resolved, CreatedAt = new DateTime(2024, 10, 4) },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000008"), StudentCode = "#SP-2024-049", Title = "Kiểm tra ổ khóa cửa sổ", Description = "H", Category = SupportTicketCategory.Maintenance, Priority = PriorityLevel.High, Status = SupportTicketStatus.Resolved, CreatedAt = new DateTime(2024, 10, 2) },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000009"), StudentCode = "#SP-2024-044", Title = "Kiểm tra hệ thống đèn hành lang", Description = "I", Category = SupportTicketCategory.Account, Priority = PriorityLevel.Low, Status = SupportTicketStatus.Resolved, CreatedAt = new DateTime(2024, 9, 30) },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000010"), StudentCode = "#SP-2024-040", Title = "Điều chỉnh lịch cấp nước", Description = "J", Category = SupportTicketCategory.Other, Priority = PriorityLevel.Medium, Status = SupportTicketStatus.Resolved, CreatedAt = new DateTime(2024, 9, 28) },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000011"), StudentCode = "#SP-2024-036", Title = "Kiểm tra đầu nối mạng", Description = "K", Category = SupportTicketCategory.Account, Priority = PriorityLevel.Medium, Status = SupportTicketStatus.InProgress, CreatedAt = new DateTime(2024, 9, 26) },
            new() { Id = Guid.Parse("10000000-0000-0000-0000-000000000012"), StudentCode = "#SP-2024-031", Title = "Bổ sung vệ sinh nhà xe", Description = "L", Category = SupportTicketCategory.Other, Priority = PriorityLevel.Low, Status = SupportTicketStatus.Resolved, CreatedAt = new DateTime(2024, 9, 24) }
        ];

        public Task<IReadOnlyList<SupportTicketDto>> GetTicketsAsync(CancellationToken ct = default) =>
            Task.FromResult(_tickets);

        public Task<SupportTicketDto?> GetTicketAsync(Guid ticketId, CancellationToken ct = default) =>
            Task.FromResult(_tickets.FirstOrDefault(ticket => ticket.Id == ticketId));

        public Task<SupportTicketDto> CreateTicketAsync(CreateSupportTicketRequest request, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task AssignTicketAsync(Guid ticketId, Guid managerId, CancellationToken ct = default) => Task.CompletedTask;
        public Task AddResponseAsync(Guid ticketId, string message, CancellationToken ct = default) => Task.CompletedTask;
        public Task UpdateStatusAsync(UpdateSupportTicketStatusRequest request, CancellationToken ct = default) => Task.CompletedTask;
        public Task CloseTicketAsync(Guid ticketId, CancellationToken ct = default) => Task.CompletedTask;
    }

    private sealed class SingleTicketSupportTicketService : ISupportTicketService
    {
        private readonly IReadOnlyList<SupportTicketDto> _tickets =
        [
            new SupportTicketDto
            {
                Id = Guid.Parse("d1111111-1111-1111-1111-111111111111"),
                StudentCode = "#SP-90000000",
                Title = "Invoice clarification",
                Description = "A",
                Category = SupportTicketCategory.Billing,
                Priority = PriorityLevel.Medium,
                Status = SupportTicketStatus.New,
                CreatedAt = new DateTime(2026, 6, 9)
            }
        ];

        public Task<IReadOnlyList<SupportTicketDto>> GetTicketsAsync(CancellationToken ct = default) =>
            Task.FromResult(_tickets);

        public Task<SupportTicketDto?> GetTicketAsync(Guid ticketId, CancellationToken ct = default) =>
            Task.FromResult(_tickets.FirstOrDefault(ticket => ticket.Id == ticketId));

        public Task<SupportTicketDto> CreateTicketAsync(CreateSupportTicketRequest request, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task AssignTicketAsync(Guid ticketId, Guid managerId, CancellationToken ct = default) => Task.CompletedTask;
        public Task AddResponseAsync(Guid ticketId, string message, CancellationToken ct = default) => Task.CompletedTask;
        public Task UpdateStatusAsync(UpdateSupportTicketStatusRequest request, CancellationToken ct = default) => Task.CompletedTask;
        public Task CloseTicketAsync(Guid ticketId, CancellationToken ct = default) => Task.CompletedTask;
    }

    private sealed class LongContentSupportTicketService : ISupportTicketService
    {
        private readonly IReadOnlyList<SupportTicketDto> _tickets =
        [
            new SupportTicketDto
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Title = "Yeu cau sua chua he thong cap nuoc tai phong 1208 voi mo ta rat dai de xac nhan cat chu va giu bo cuc cot on dinh tren cua so hep",
                Description = "A",
                Category = SupportTicketCategory.Maintenance,
                Priority = PriorityLevel.High,
                Status = SupportTicketStatus.InProgress,
                CreatedAt = new DateTime(2026, 6, 5)
            }
        ];

        public Task<IReadOnlyList<SupportTicketDto>> GetTicketsAsync(CancellationToken ct = default) =>
            Task.FromResult(_tickets);

        public Task<SupportTicketDto?> GetTicketAsync(Guid ticketId, CancellationToken ct = default) =>
            Task.FromResult(_tickets.FirstOrDefault(ticket => ticket.Id == ticketId));

        public Task<SupportTicketDto> CreateTicketAsync(CreateSupportTicketRequest request, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task AssignTicketAsync(Guid ticketId, Guid managerId, CancellationToken ct = default) => Task.CompletedTask;
        public Task AddResponseAsync(Guid ticketId, string message, CancellationToken ct = default) => Task.CompletedTask;
        public Task UpdateStatusAsync(UpdateSupportTicketStatusRequest request, CancellationToken ct = default) => Task.CompletedTask;
        public Task CloseTicketAsync(Guid ticketId, CancellationToken ct = default) => Task.CompletedTask;
    }
}







