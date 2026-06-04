using System.IO;
using System.Reflection;
using System.Resources;
using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml.Linq;
using DormitoryManagement.Application.Abstractions.Auth;
using DormitoryManagement.Application.Common;
using DormitoryManagement.Application.DTOs.Auth;
using DormitoryManagement.Application.DTOs.Dashboard;
using DormitoryManagement.Application.DTOs.Registrations;
using DormitoryManagement.Application.DTOs.Rooms;
using DormitoryManagement.Application.DTOs.Vehicles;
using DormitoryManagement.Application.Services.Dashboard;
using DormitoryManagement.Application.Services.Registrations;
using DormitoryManagement.Application.Services.Rooms;
using DormitoryManagement.Application.Services.Vehicles;
using DormitoryManagement.Domain.Enums;
using DormitoryManagement.WPF.Common;
using DormitoryManagement.WPF.Navigation;
using DormitoryManagement.WPF.ViewModels;
using DormitoryManagement.WPF.ViewModels.Auth;
using DormitoryManagement.WPF.Views.Auth;
using DormitoryManagement.Application.Services.Auth;
using DormitoryManagement.WPF.ViewModels.Dashboard;
using DormitoryManagement.WPF.ViewModels.Registrations;
using DormitoryManagement.WPF.ViewModels.Vehicles;
using DormitoryManagement.WPF.Views.Dashboard;
using DormitoryManagement.WPF.Views.Vehicles;
using Microsoft.Extensions.DependencyInjection;
using DormitoryManagement.WPF.ViewModels.Forum;
using DormitoryManagement.WPF.Views.Forum;
using DormitoryManagement.WPF.Views.Shared;

namespace DormitoryManagement.WPF.Tests;

public sealed class WpfResourceTests
{
    [Fact]
    public void Compiled_resources_include_main_window_baml()
    {
        var keys = GetCompiledResourceKeys();

        Assert.Contains("views/shared/mainwindow.baml", keys);
        Assert.Contains("views/shared/shellview.baml", keys);
    }

    [Fact]
    public void Main_window_starts_maximized_for_fullscreen_shell_review()
    {
        var document = LoadMainWindowDocument();

        Assert.Equal("Maximized", document.Root?.Attribute("WindowState")?.Value);
    }

    [Fact]
    public void Compiled_resources_include_forum_home_view_and_tokens()
    {
        var keys = GetCompiledResourceKeys();

        Assert.Contains("views/forum/forumhomeview.baml", keys);
        Assert.Contains("resources/forumhome.baml", keys);
    }

    [Fact]
    public void Forum_home_asset_directories_exist_and_are_not_empty()
    {
        var repoRoot = FindRepositoryRoot();
        var interDir = Path.Combine(repoRoot, "src", "DormitoryManagement.WPF", "Assets", "Fonts", "Inter");
        var plusJakartaDir = Path.Combine(repoRoot, "src", "DormitoryManagement.WPF", "Assets", "Fonts", "PlusJakartaSans");
        var imagesDir = Path.Combine(repoRoot, "src", "DormitoryManagement.WPF", "Assets", "Images", "ForumHome");

        AssertDirectoryHasFiles(interDir);
        AssertDirectoryHasFiles(plusJakartaDir);
        AssertDirectoryHasFiles(imagesDir);
    }

    [Fact]
    public void Forum_home_named_header_avatar_asset_exists_for_visual_review()
    {
        var repoRoot = FindRepositoryRoot();
        var imagesDir = Path.Combine(repoRoot, "src", "DormitoryManagement.WPF", "Assets", "Images", "ForumHome");

        Assert.True(File.Exists(Path.Combine(imagesDir, "forum-home-avatar-student.png")));
    }

    [Fact]
    public void Forum_home_named_author_avatar_assets_exist_for_visual_review()
    {
        var repoRoot = FindRepositoryRoot();
        var imagesDir = Path.Combine(repoRoot, "src", "DormitoryManagement.WPF", "Assets", "Images", "ForumHome");

        Assert.True(File.Exists(Path.Combine(imagesDir, "forum-home-author-management.png")));
        Assert.True(File.Exists(Path.Combine(imagesDir, "forum-home-author-volunteer.png")));
        Assert.True(File.Exists(Path.Combine(imagesDir, "forum-home-author-lan-anh.png")));
    }

    [Fact]
    public void Forum_home_resources_define_expected_inter_and_plus_jakarta_font_keys()
    {
        var document = LoadForumHomeResourceDocument();
        var fontFamilies = document.Root!
            .Elements(WpfNamespace + "FontFamily")
            .ToDictionary(
                element => element.Attribute(XamlNamespace + "Key")?.Value ?? string.Empty,
                element => element.Value.Trim(),
                StringComparer.Ordinal);

        Assert.Equal(
            "pack://application:,,,/DormitoryManagement.WPF;component/Assets/Fonts/PlusJakartaSans/#Plus Jakarta Sans",
            fontFamilies["ForumHomeHeadlineFontFamily"]);
        Assert.Equal(
            "pack://application:,,,/DormitoryManagement.WPF;component/Assets/Fonts/Inter/#Inter",
            fontFamilies["ForumHomeBodyFontFamily"]);
    }

    [Fact]
    public void Forum_home_resources_apply_expected_font_keys_to_text_and_input_styles()
    {
        var document = LoadForumHomeResourceDocument();

        AssertFontSetter(document, "ForumHomeBrandTextStyle", "{StaticResource ForumHomeHeadlineFontFamily}");
        AssertFontSetter(document, "ForumHomeDisplayTextStyle", "{StaticResource ForumHomeHeadlineFontFamily}");
        AssertFontSetter(document, "ForumHomeHeadlineTextStyle", "{StaticResource ForumHomeHeadlineFontFamily}");
        AssertFontSetter(document, "ForumHomeBodyTextStyle", "{StaticResource ForumHomeBodyFontFamily}");
        AssertFontSetter(document, "ForumHomeLabelMediumTextStyle", "{StaticResource ForumHomeBodyFontFamily}");
        AssertFontSetter(document, "ForumHomeMetaTextStyle", "{StaticResource ForumHomeBodyFontFamily}");
        AssertFontSetter(document, "ForumHomeSectionLabelTextStyle", "{StaticResource ForumHomeBodyFontFamily}");
        AssertFontSetter(document, "ForumHomePrimaryButtonStyle", "{StaticResource ForumHomeBodyFontFamily}");
        AssertFontSetter(document, "ForumHomeGhostButtonStyle", "{StaticResource ForumHomeBodyFontFamily}");
        AssertFontSetter(document, "ForumHomeChipButtonStyle", "{StaticResource ForumHomeBodyFontFamily}");
        AssertFontSetter(document, "ForumHomeTextBoxStyle", "{StaticResource ForumHomeBodyFontFamily}");
    }

    [Fact]
    public void Forum_home_resources_define_exact_reference_color_tokens()
    {
        var document = LoadForumHomeResourceDocument();

        AssertResourceValue(document, "Color", "ForumHomeColorCanvas", "#F8F9FF");
        AssertResourceValue(document, "Color", "ForumHomeColorSurface", "#FFFFFF");
        AssertResourceValue(document, "Color", "ForumHomeColorSurfaceLow", "#EFF4FF");
        AssertResourceValue(document, "Color", "ForumHomeColorSurfaceContainer", "#E5EEFF");
        AssertResourceValue(document, "Color", "ForumHomeColorSurfaceHigh", "#DCE9FF");
        AssertResourceValue(document, "Color", "ForumHomeColorSurfaceHighest", "#D3E4FE");
        AssertResourceValue(document, "Color", "ForumHomeColorOutline", "#8D7168");
        AssertResourceValue(document, "Color", "ForumHomeColorOutlineVariant", "#E1BFB5");
        AssertResourceValue(document, "Color", "ForumHomeColorTextPrimary", "#0B1C30");
        AssertResourceValue(document, "Color", "ForumHomeColorTextSecondary", "#594139");
        AssertResourceValue(document, "Color", "ForumHomeColorPrimary", "#AB3500");
        AssertResourceValue(document, "Color", "ForumHomeColorPrimaryContainer", "#FF6B35");
        AssertResourceValue(document, "Color", "ForumHomeColorSecondary", "#4648D4");
        AssertResourceValue(document, "Color", "ForumHomeColorSecondaryContainer", "#6063EE");
        AssertResourceValue(document, "Color", "ForumHomeColorTertiary", "#006C49");
        AssertResourceValue(document, "Color", "ForumHomeColorTertiaryContainer", "#00AF79");
    }

    [Fact]
    public void Forum_home_resources_define_exact_reference_spacing_and_radius_tokens()
    {
        var document = LoadForumHomeResourceDocument();

        AssertResourceValue(document, "Double", "ForumHomeDisplayLargeSize", "48");
        AssertResourceValue(document, "Double", "ForumHomeHeadlineLargeSize", "32");
        AssertResourceValue(document, "Double", "ForumHomeHeadlineMediumSize", "24");
        AssertResourceValue(document, "Double", "ForumHomeHeadlineSmallSize", "18");
        AssertResourceValue(document, "Double", "ForumHomeBodyLargeSize", "16");
        AssertResourceValue(document, "Double", "ForumHomeBodyMediumSize", "14");
        AssertResourceValue(document, "Double", "ForumHomeLabelMediumSize", "14");
        AssertResourceValue(document, "Double", "ForumHomeLabelSmallSize", "12");
        AssertResourceValue(document, "Double", "ForumHomeHeaderHeight", "64");
        AssertResourceValue(document, "Double", "ForumHomeMarginDesktop", "32");
        AssertResourceValue(document, "Double", "ForumHomeMarginMobile", "16");
        AssertResourceValue(document, "Double", "ForumHomeGutter", "24");
        AssertResourceValue(document, "Double", "ForumHomeContainerMaxWidth", "1440");
        AssertResourceValue(document, "Double", "ForumHomeStackSmall", "8");
        AssertResourceValue(document, "Double", "ForumHomeStackMedium", "16");
        AssertResourceValue(document, "Double", "ForumHomeStackLarge", "24");

        AssertResourceValue(document, "CornerRadius", "ForumHomeControlCornerRadius", "8");
        AssertResourceValue(document, "CornerRadius", "ForumHomeCardCornerRadius", "12");
        AssertResourceValue(document, "CornerRadius", "ForumHomeFeaturedCornerRadius", "12");
        AssertResourceValue(document, "CornerRadius", "ForumHomePillCornerRadius", "999");

        AssertResourceValue(document, "Thickness", "ForumHomeInputPadding", "16,8");
        AssertResourceValue(document, "Thickness", "ForumHomeChipPadding", "12,4");
        AssertResourceValue(document, "Thickness", "ForumHomeSectionSpacing", "0,0,0,24");
    }

    [Fact]
    public void Forum_home_text_styles_define_exact_reference_line_heights()
    {
        var document = LoadForumHomeResourceDocument();

        AssertStyleSetter(document, "ForumHomeBrandTextStyle", "LineHeight", "32");
        AssertStyleSetter(document, "ForumHomeDisplayTextStyle", "LineHeight", "56");
        AssertStyleSetter(document, "ForumHomeHeadlineTextStyle", "LineHeight", "24");
        AssertStyleSetter(document, "ForumHomeBodyTextStyle", "LineHeight", "26");
        AssertStyleSetter(document, "ForumHomeLabelMediumTextStyle", "LineHeight", "20");
        AssertStyleSetter(document, "ForumHomeMetaTextStyle", "LineHeight", "22");
        AssertStyleSetter(document, "ForumHomeSectionLabelTextStyle", "LineHeight", "16");
    }

    [Fact]
    public void Forum_home_text_styles_use_ideal_text_formatting_for_cleaner_vietnamese_copy()
    {
        var document = LoadForumHomeResourceDocument();

        AssertStyleSetter(document, "ForumHomeBrandTextStyle", "TextOptions.TextFormattingMode", "Ideal");
        AssertStyleSetter(document, "ForumHomeHeadlineTextStyle", "TextOptions.TextFormattingMode", "Ideal");
        AssertStyleSetter(document, "ForumHomeHeadlineMediumTextStyle", "TextOptions.TextFormattingMode", "Ideal");
        AssertStyleSetter(document, "ForumHomeBodyTextStyle", "TextOptions.TextFormattingMode", "Ideal");
        AssertStyleSetter(document, "ForumHomeLabelMediumTextStyle", "TextOptions.TextFormattingMode", "Ideal");
        AssertStyleSetter(document, "ForumHomeMetaTextStyle", "TextOptions.TextFormattingMode", "Ideal");
        AssertStyleSetter(document, "ForumHomeSectionLabelTextStyle", "TextOptions.TextFormattingMode", "Ideal");
        AssertStyleSetter(document, "ForumHomeBrandTextStyle", "TextOptions.TextRenderingMode", "ClearType");
        AssertStyleSetter(document, "ForumHomeBodyTextStyle", "TextOptions.TextRenderingMode", "ClearType");
        AssertStyleSetter(document, "ForumHomeMetaTextStyle", "TextOptions.TextRenderingMode", "ClearType");
    }

    [Fact]
    public void Forum_home_text_styles_define_exact_reference_font_sizes_and_weights()
    {
        var document = LoadForumHomeResourceDocument();

        AssertStyleSetter(document, "ForumHomeBrandTextStyle", "FontSize", "{StaticResource ForumHomeHeadlineMediumSize}");
        AssertStyleSetter(document, "ForumHomeBrandTextStyle", "FontWeight", "Bold");
        AssertStyleSetter(document, "ForumHomeDisplayTextStyle", "FontSize", "{StaticResource ForumHomeDisplayLargeSize}");
        AssertStyleSetter(document, "ForumHomeDisplayTextStyle", "FontWeight", "Bold");
        AssertStyleSetter(document, "ForumHomeHeadlineTextStyle", "FontSize", "{StaticResource ForumHomeHeadlineSmallSize}");
        AssertStyleSetter(document, "ForumHomeHeadlineTextStyle", "FontWeight", "SemiBold");
        AssertStyleSetter(document, "ForumHomeBodyTextStyle", "FontSize", "{StaticResource ForumHomeBodyLargeSize}");
        AssertStyleSetter(document, "ForumHomeBodyTextStyle", "FontWeight", "Normal");
        AssertStyleSetter(document, "ForumHomeLabelMediumTextStyle", "FontSize", "{StaticResource ForumHomeLabelMediumSize}");
        AssertStyleSetter(document, "ForumHomeLabelMediumTextStyle", "FontWeight", "SemiBold");
        AssertStyleSetter(document, "ForumHomeMetaTextStyle", "FontSize", "{StaticResource ForumHomeBodyMediumSize}");
        AssertStyleSetter(document, "ForumHomeMetaTextStyle", "FontWeight", "Normal");
        AssertStyleSetter(document, "ForumHomeSectionLabelTextStyle", "FontSize", "{StaticResource ForumHomeLabelSmallSize}");
        AssertStyleSetter(document, "ForumHomeSectionLabelTextStyle", "FontWeight", "Medium");
    }

    [Fact]
    public void Forum_home_feed_cards_follow_the_no_image_reference_and_keep_support_metadata_local_only()
    {
        var viewModel = new ForumHomeViewModel();

        Assert.NotEmpty(viewModel.FeedCards);
        Assert.Contains(viewModel.FeedCards, card => !card.UseFallbackVisual);
        Assert.Contains(viewModel.FeedCards, card => card.UseFallbackVisual);

        Assert.All(viewModel.FeedCards, card => Assert.True(
            string.IsNullOrWhiteSpace(card.CoverAssetPath),
            $"Expected no cover image for card '{card.Id}' in the no-image feed."));

        var supportCards = viewModel.FeedCards.Where(card => card.UseFallbackVisual).ToArray();
        Assert.NotEmpty(supportCards);
        Assert.All(supportCards, card => Assert.False(string.IsNullOrWhiteSpace(card.FallbackBadgeText)));
    }

    [Fact]
    public void Forum_home_view_renders_required_phase1_text_when_hosted_offscreen()
    {
        RunOnStaThread(() =>
        {
            var view = new ForumHomeView
            {
                DataContext = new ForumHomeViewModel(),
                Width = 1600,
                Height = 1000
            };
            var window = new Window
            {
                Width = 1600,
                Height = 1000,
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

                var texts = GetVisibleStringValues(view);
                string[] requiredTexts =
                [
                    "DMForum",
                    "Tìm kiếm tin tức, thông báo...",
                    "Nguyễn Văn A",
                    "Phòng 402 - Khu B",
                    "DANH MỤC",
                    "Tin tức chung",
                    "CLB & Sự kiện",
                    "Mua bán & Trao đổi",
                    "Góp ý & Khiếu nại",
                    "THẺ PHỔ BIẾN",
                    "#thongbao",
                    "#clbsukien",
                    "#passdo",
                    "#cangtin",
                    "#thethao",
                    "#matdien",
                    "KHU VỰC",
                    "Khu A",
                    "Khu B",
                    "Nhà ăn & Dịch vụ",
                    "Bạn có thông tin gì muốn chia sẻ với mọi người?",
                    "Đăng bài",
                    "Thông báo lịch bảo trì điện nước khu B",
                    "Tuyển thành viên CLB Tình nguyện KTX",
                    "Review quán cơm mới mở cạnh căng tin khu C",
                    "Hoạt động SV",
                    "Thêm",
                    "Giải bóng đá sinh viên nam KTX",
                    "Đêm nhạc Acoustic gây quỹ",
                    "Liên hệ khẩn cấp",
                    "Phòng Bảo vệ KTX",
                    "Trạm y tế Tòa nhà C",
                    "Văn phòng Ban Quản lý"
                ];

                foreach (var requiredText in requiredTexts)
                {
                    Assert.Contains(requiredText, texts);
                }
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void Forum_home_view_renders_phase1_modules_in_expected_sequence()
    {
        RunOnStaThread(() =>
        {
            var view = new ForumHomeView { DataContext = new ForumHomeViewModel() };
            var window = new Window
            {
                Width = 1600,
                Height = 1000,
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

                var texts = GetVisibleStringSequence(view);

                AssertTextOrder(
                    texts,
                    "DMForum",
                    "DANH MỤC",
                    "THẺ PHỔ BIẾN",
                    "KHU VỰC",
                    "Bạn có thông tin gì muốn chia sẻ với mọi người?",
                    "Đăng bài",
                    "Thông báo lịch bảo trì điện nước khu B",
                    "Tuyển thành viên CLB Tình nguyện KTX",
                    "Review quán cơm mới mở cạnh căng tin khu C",
                    "Hoạt động SV",
                    "Thêm",
                    "Liên hệ khẩn cấp");
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void Forum_home_default_phase1_render_hides_phase2_only_preview_surfaces()
    {
        RunOnStaThread(() =>
        {
            var view = new ForumHomeView
            {
                DataContext = new ForumHomeViewModel(),
                Width = 1600,
                Height = 1000
            };
            var window = new Window
            {
                Width = 1600,
                Height = 1000,
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

                var texts = GetVisibleStringValues(view);

                string[] excludedTexts =
                [
                    "Xem trước cục bộ",
                    "Tin nhắn gần đây",
                    "Thông báo xem trước",
                    "Tóm tắt hồ sơ",
                    "Bộ lọc xem trước đang bật",
                    "Kết quả hiện chỉ thay đổi trong bản xem trước cục bộ.",
                    "Không tìm thấy bài viết phù hợp.",
                    "Xem lại toàn bộ bài viết",
                    "Đăng bài viết mới",
                    "Đăng tải lên bảng tin của cộng đồng KTX",
                    "Tiêu đề bài đăng *",
                    "Nội dung chi tiết *",
                    "Chọn nhanh thẻ phổ biến",
                    "Xem trước liên hệ khẩn cấp",
                    "Bản xem trước chỉ mô phỏng thao tác liên hệ, không thực hiện cuộc gọi thật."
                ];

                foreach (var excludedText in excludedTexts)
                {
                    Assert.DoesNotContain(excludedText, texts);
                }
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void Forum_home_view_xaml_defines_exact_desktop_outer_margins_and_gutter_rhythm()
    {
        var document = LoadForumHomeViewDocument();

        var headerBorder = document.Root!
            .Descendants(WpfNamespace + "Border")
            .FirstOrDefault(element => string.Equals(element.Attribute("Grid.Row")?.Value, "0", StringComparison.Ordinal));

        Assert.NotNull(headerBorder);
        Assert.Equal("{StaticResource ForumHomeHeaderHeight}", headerBorder!.Attribute("Height")?.Value);

        var contentGrid = document.Root!
            .Descendants(WpfNamespace + "Grid")
            .FirstOrDefault(element =>
                string.Equals(element.Attribute("Grid.Row")?.Value, "1", StringComparison.Ordinal)
                && string.Equals(element.Attribute("Margin")?.Value, "32,24,32,32", StringComparison.Ordinal));

        Assert.NotNull(contentGrid);

        var columnWidths = contentGrid!
            .Element(WpfNamespace + "Grid.ColumnDefinitions")!
            .Elements(WpfNamespace + "ColumnDefinition")
            .Select(element => element.Attribute("Width")?.Value ?? string.Empty)
            .ToArray();

        Assert.Equal(["256", "24", "*", "24", "320"], columnWidths);
    }

    [Fact]
    public void Forum_home_header_renders_brand_search_and_user_summary_in_left_to_right_order()
    {
        RunOnStaThread(() =>
        {
            var view = new ForumHomeView { DataContext = new ForumHomeViewModel() };
            var window = new Window
            {
                Width = 1600,
                Height = 1000,
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

                var brand = FindVisibleTextBlock(view, "DMForum");
                var search = FindVisibleTextBlock(view, "Tìm kiếm tin tức, thông báo...");
                var user = FindVisibleTextBlock(view, "Nguyễn Văn A");

                var brandBounds = GetBoundsRelativeToAncestor(brand, view);
                var searchBounds = GetBoundsRelativeToAncestor(search, view);
                var userBounds = GetBoundsRelativeToAncestor(user, view);

                Assert.True(brandBounds.Left < searchBounds.Left, "Brand should render left of search.");
                Assert.True(searchBounds.Left < userBounds.Left, "Search should render left of user summary.");
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void Forum_home_header_xaml_defines_a_longer_search_lane_and_circular_profile_avatar()
    {
        var document = LoadForumHomeViewDocument();
        var searchGrid = document.Root!
            .Descendants(WpfNamespace + "Grid")
            .FirstOrDefault(element =>
                string.Equals(element.Attribute("Grid.Column")?.Value, "1", StringComparison.Ordinal)
                && string.Equals(element.Attribute("MaxWidth")?.Value, "576", StringComparison.Ordinal)
                && string.Equals(element.Attribute("Margin")?.Value, "48,0", StringComparison.Ordinal));

        Assert.NotNull(searchGrid);

        var headerAvatarBorder = document.Root!
            .Descendants(WpfNamespace + "Border")
            .FirstOrDefault(element =>
                string.Equals(element.Attribute("Width")?.Value, "40", StringComparison.Ordinal)
                && string.Equals(element.Attribute("Height")?.Value, "40", StringComparison.Ordinal)
                && string.Equals(element.Attribute("CornerRadius")?.Value, "20", StringComparison.Ordinal));

        Assert.NotNull(headerAvatarBorder);
        Assert.NotNull(headerAvatarBorder!.Element(WpfNamespace + "Border.Clip")?.Element(WpfNamespace + "EllipseGeometry"));
    }

    [Fact]
    public void Forum_home_root_grid_enables_fixed_text_hinting_for_clear_type_rendering()
    {
        var document = LoadForumHomeViewDocument();
        var rootGrid = document.Root!.Element(WpfNamespace + "Grid");

        Assert.NotNull(rootGrid);
        Assert.Equal("Fixed", rootGrid!.Attribute("TextOptions.TextHintingMode")?.Value);
    }

    [Fact]
    public void Forum_home_feed_card_xaml_defines_single_column_no_image_layout_with_fixed_action_alignment()
    {
        var document = LoadForumHomeViewDocument();
        var coverImage = document.Root!
            .Descendants(WpfNamespace + "Image")
            .FirstOrDefault(element => string.Equals(element.Attribute("Source")?.Value, "{Binding CoverAssetPath}", StringComparison.Ordinal));

        Assert.Null(coverImage);

        var headerBadgeGrid = document.Root!
            .Descendants(WpfNamespace + "Grid")
            .FirstOrDefault(element =>
                string.Equals(element.Attribute("Grid.Row")?.Value, "0", StringComparison.Ordinal)
                && element.Elements(WpfNamespace + "ItemsControl")
                    .Any(itemsControl => string.Equals(itemsControl.Attribute("ItemsSource")?.Value, "{Binding Badges}", StringComparison.Ordinal)));

        Assert.NotNull(headerBadgeGrid);

        var likeButton = document.Root!
            .Descendants(WpfNamespace + "Button")
            .FirstOrDefault(element =>
                string.Equals(element.Attribute("Command")?.Value, "{Binding DataContext.ToggleLikeCommand, RelativeSource={RelativeSource AncestorType=UserControl}}", StringComparison.Ordinal)
                && string.Equals(element.Attribute("HorizontalAlignment")?.Value, "Right", StringComparison.Ordinal)
                && string.Equals(element.Attribute("VerticalAlignment")?.Value, "Top", StringComparison.Ordinal)
                && string.IsNullOrWhiteSpace(element.Attribute("Grid.Column")?.Value));

        Assert.NotNull(likeButton);

        var authorAvatarBorder = document.Root!
            .Descendants(WpfNamespace + "Border")
            .FirstOrDefault(element =>
                string.Equals(element.Attribute("Width")?.Value, "28", StringComparison.Ordinal)
                && string.Equals(element.Attribute("Height")?.Value, "28", StringComparison.Ordinal)
                && string.Equals(element.Attribute("CornerRadius")?.Value, "14", StringComparison.Ordinal));

        Assert.NotNull(authorAvatarBorder);
        Assert.NotNull(authorAvatarBorder!.Element(WpfNamespace + "Border.Clip")?.Element(WpfNamespace + "EllipseGeometry"));
    }

    [Fact]
    public void Forum_home_feed_card_render_keeps_copy_left_aligned_and_metrics_locked_to_the_footer_row()
    {
        RunOnStaThread(() =>
        {
            var view = new ForumHomeView { DataContext = new ForumHomeViewModel() };
            var window = new Window
            {
                Width = 1600,
                Height = 1000,
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

                var cardTitle = FindVisibleTextBlock(view, "Thông báo lịch bảo trì điện nước khu B");
                var authorName = FindVisibleTextBlock(view, "Ban Quản Lý");
                var viewMetric = FindVisibleTextBlock(view, "1.2k");
                var likeMetric = FindVisibleTextBlock(view, "45");
                var commentMetric = FindVisibleTextBlock(view, "12");

                var cardTitleBounds = GetBoundsRelativeToAncestor(cardTitle, view);
                var authorBounds = GetBoundsRelativeToAncestor(authorName, view);
                var viewMetricBounds = GetBoundsRelativeToAncestor(viewMetric, view);
                var likeMetricBounds = GetBoundsRelativeToAncestor(likeMetric, view);
                var commentMetricBounds = GetBoundsRelativeToAncestor(commentMetric, view);

                Assert.True(cardTitleBounds.Left < 430, $"Expected no-image card copy to start near the left card edge, but title started at {cardTitleBounds.Left}.");
                Assert.True(authorBounds.Left < viewMetricBounds.Left, "Author block should stay left of the metric row.");
                Assert.True(viewMetricBounds.Left < likeMetricBounds.Left, "View metric should stay left of the like metric.");
                Assert.True(likeMetricBounds.Left < commentMetricBounds.Left, "Like metric should stay left of the comment metric.");
                Assert.True(authorBounds.Top > cardTitleBounds.Top, "Author row should sit below the title block.");
                Assert.True(Math.Abs(viewMetricBounds.Top - authorBounds.Top) < 20, "Footer metrics should align with the author row.");
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void Forum_home_right_rail_xaml_defines_header_arrows_and_compact_row_sizes()
    {
        var document = LoadForumHomeViewDocument();
        var arrowIcons = document.Root!
            .Descendants()
            .Where(element => string.Equals(element.Name.LocalName, "PackIconMaterial", StringComparison.Ordinal))
            .Count(element => string.Equals(element.Attribute("Kind")?.Value, "ArrowRight", StringComparison.Ordinal));

        Assert.True(arrowIcons >= 3, "Expected arrow affordances for left area header and both right-rail section headers.");

        var arrowHeaderDockPanels = document.Root!
            .Descendants(WpfNamespace + "DockPanel")
            .Where(element => element.Descendants().Any(child =>
                string.Equals(child.Name.LocalName, "PackIconMaterial", StringComparison.Ordinal)
                && string.Equals(child.Attribute("Kind")?.Value, "ArrowRight", StringComparison.Ordinal)))
            .ToArray();

        Assert.True(arrowHeaderDockPanels.Length >= 3, "Expected area/activity/emergency arrow headers to use DockPanel layout.");
        Assert.All(arrowHeaderDockPanels, element => Assert.Equal("False", element.Attribute("LastChildFill")?.Value));

        var contactPhoneIcon = document.Root!
            .Descendants()
            .FirstOrDefault(element =>
                string.Equals(element.Name.LocalName, "PackIconMaterial", StringComparison.Ordinal)
                && string.Equals(element.Attribute("Kind")?.Value, "PhoneOutline", StringComparison.Ordinal)
                && string.Equals(element.Attribute("Grid.Column")?.Value, "2", StringComparison.Ordinal));

        Assert.NotNull(contactPhoneIcon);

        var dateTile = document.Root!
            .Descendants(WpfNamespace + "Border")
            .FirstOrDefault(element =>
                string.Equals(element.Attribute("Width")?.Value, "48", StringComparison.Ordinal)
                && string.Equals(element.Attribute("Height")?.Value, "56", StringComparison.Ordinal));

        Assert.NotNull(dateTile);

        var contactIconTile = document.Root!
            .Descendants(WpfNamespace + "Border")
            .FirstOrDefault(element =>
                string.Equals(element.Attribute("Width")?.Value, "48", StringComparison.Ordinal)
                && string.Equals(element.Attribute("Height")?.Value, "48", StringComparison.Ordinal));

        Assert.NotNull(contactIconTile);
    }

    [Fact]
    public void Forum_home_compose_popup_uses_wpf_ui_controls_for_supported_inputs_and_actions()
    {
        var document = LoadForumHomeViewDocument();
        var wpfUiNamespace = XNamespace.Get("http://schemas.lepo.co/wpfui/2022/xaml");

        Assert.True(document.Root!.Descendants(wpfUiNamespace + "TextBox").Count() >= 3);
        Assert.True(document.Root!.Descendants(wpfUiNamespace + "Button").Count() >= 2);

        var wpfUiDictionaries = document.Root!
            .Descendants()
            .Where(element => element.Name.Namespace == wpfUiNamespace)
            .Select(element => element.Name.LocalName)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains("ThemesDictionary", wpfUiDictionaries);
        Assert.Contains("ControlsDictionary", wpfUiDictionaries);
    }

    [Fact]
    public void Forum_home_view_wraps_long_vietnamese_copy_without_horizontal_overflow_at_narrower_desktop_width()
    {
        RunOnStaThread(() =>
        {
            var view = new ForumHomeView { DataContext = new ForumHomeViewModel() };
            var window = new Window
            {
                Width = 1120,
                Height = 920,
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

                var scrollViewers = FindDescendants<ScrollViewer>(view)
                    .Where(scrollViewer => scrollViewer.IsVisible)
                    .ToArray();

                Assert.NotEmpty(scrollViewers);
                Assert.All(scrollViewers, scrollViewer => Assert.True(
                    scrollViewer.ScrollableWidth <= 0.5,
                    $"Expected no horizontal overflow, but '{scrollViewer.Name}' reported ScrollableWidth={scrollViewer.ScrollableWidth}."));

                var longTitle = FindVisibleTextBlock(view, "Review quán cơm mới mở cạnh căng tin khu C");
                var longExcerpt = FindVisibleTextBlock(view, "Ban quản lý KTX xin thông báo: Từ 8h00 đến 11h30 sáng thứ 7 tuần này, khu B sẽ tạm ngưng cấp điện, nước để tiến hành bảo trì hệ thống định kỳ. Mong các bạn sinh viên chủ động sắp xếp...");

                Assert.True(longTitle.ActualHeight > 30, $"Expected wrapped long Vietnamese title, but height was {longTitle.ActualHeight}.");
                Assert.True(longExcerpt.ActualHeight > 40, $"Expected wrapped Vietnamese excerpt, but height was {longExcerpt.ActualHeight}.");
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void Forum_home_visible_avatar_images_resolve_to_local_bitmaps_offscreen()
    {
        RunOnStaThread(() =>
        {
            var view = new ForumHomeView { DataContext = new ForumHomeViewModel() };
            var window = new Window
            {
                Width = 1600,
                Height = 1000,
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

                var visibleImages = FindDescendants<Image>(view)
                    .Where(image => image.IsVisible)
                    .ToArray();

                Assert.NotEmpty(visibleImages);
                Assert.All(visibleImages, image =>
                {
                    var bitmap = Assert.IsAssignableFrom<BitmapSource>(image.Source);
                    Assert.True(
                        image.ActualWidth <= 64 && image.ActualHeight <= 64,
                        $"Expected only compact avatar imagery in the no-image feed, but found visible image size {image.ActualWidth}x{image.ActualHeight}.");
                    Assert.True(bitmap.PixelWidth > 0, "Expected visible forum-home image to resolve to a non-empty bitmap width.");
                    Assert.True(bitmap.PixelHeight > 0, "Expected visible forum-home image to resolve to a non-empty bitmap height.");
                });
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void Forum_home_phase1_review_screenshot_artifact_can_be_captured()
    {
        RunOnStaThread(() =>
        {
            var view = new ForumHomeView { DataContext = new ForumHomeViewModel() };
            var window = new Window
            {
                Width = 1600,
                Height = 1000,
                Content = view,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Left = -10000,
                Top = -10000,
                ShowInTaskbar = false,
                WindowStyle = WindowStyle.None,
                ResizeMode = ResizeMode.NoResize
            };

            try
            {
                window.Show();
                WaitForLayout();
                view.Measure(new Size(1600, 1000));
                view.Arrange(new Rect(0, 0, 1600, 1000));
                view.UpdateLayout();

                var bitmap = new RenderTargetBitmap(1600, 1000, 96, 96, PixelFormats.Pbgra32);
                var visual = new DrawingVisual();
                using (var drawingContext = visual.RenderOpen())
                {
                    drawingContext.DrawRectangle(Brushes.White, null, new Rect(0, 0, 1600, 1000));
                    drawingContext.DrawRectangle(new VisualBrush(view), null, new Rect(0, 0, 1600, 1000));
                }

                bitmap.Render(visual);

                var artifactPath = Path.Combine(FindRepositoryRoot(), ".ai", "artifacts", "forum-phase1-wpf.png");
                Directory.CreateDirectory(Path.GetDirectoryName(artifactPath)!);

                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));

                using var stream = File.Create(artifactPath);
                encoder.Save(stream);

                Assert.True(File.Exists(artifactPath));
                Assert.True(new FileInfo(artifactPath).Length > 0, $"Expected screenshot artifact '{artifactPath}' to be non-empty.");
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void Forum_home_view_renders_preview_panel_dropdown_when_messages_are_opened()
    {
        RunOnStaThread(() =>
        {
            var viewModel = new ForumHomeViewModel();
            viewModel.OpenMessagesCommand.Execute(null);

            var view = new ForumHomeView
            {
                DataContext = viewModel,
                Width = 1600,
                Height = 1000
            };
            var window = new Window
            {
                Width = 1600,
                Height = 1000,
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

                var texts = GetVisibleStringValues(view);

                Assert.Contains("Tin nhắn gần đây", texts);
                Assert.Contains("Nhóm trực KTX", texts);
                Assert.Contains("Xem trước cục bộ", texts);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void Forum_home_view_renders_empty_state_and_reset_action_for_preview_only_no_match_result()
    {
        RunOnStaThread(() =>
        {
            var viewModel = new ForumHomeViewModel
            {
                SearchText = "khong-co-ket-qua"
            };

            var view = new ForumHomeView
            {
                DataContext = viewModel,
                Width = 1600,
                Height = 1000
            };
            var window = new Window
            {
                Width = 1600,
                Height = 1000,
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

                var texts = GetVisibleStringValues(view);

                Assert.Contains("Không tìm thấy bài viết phù hợp.", texts);
                Assert.Contains("Xem lại toàn bộ bài viết", texts);
                Assert.DoesNotContain("Thông báo lịch bảo trì điện nước khu B", texts);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void Forum_home_view_renders_emergency_contact_preview_surface_offscreen()
    {
        RunOnStaThread(() =>
        {
            var viewModel = new ForumHomeViewModel();
            viewModel.OpenEmergencyContactPreviewCommand.Execute(viewModel.EmergencyContacts.First());

            var view = new ForumHomeView
            {
                DataContext = viewModel,
                Width = 1600,
                Height = 1000
            };
            var window = new Window
            {
                Width = 1600,
                Height = 1000,
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

                var texts = GetVisibleStringValues(view);

                Assert.Contains("Xem trước liên hệ khẩn cấp", texts);
                Assert.Contains("Sẵn sàng kết nối xem trước.", texts);
                Assert.Contains("Bản xem trước chỉ mô phỏng thao tác liên hệ, không thực hiện cuộc gọi thật.", texts);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void Forum_home_view_renders_compose_popup_from_input_strip_offscreen()
    {
        RunOnStaThread(() =>
        {
            var viewModel = new ForumHomeViewModel();
            viewModel.ComposeCommand.Execute(null);

            var view = new ForumHomeView
            {
                DataContext = viewModel,
                Width = 1600,
                Height = 1000
            };
            var window = new Window
            {
                Width = 1600,
                Height = 1000,
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

                var texts = GetVisibleStringValues(view);

                Assert.Contains("Đăng bài viết mới", texts);
                Assert.DoesNotContain("Đăng tải lên bảng tin của cộng đồng KTX", texts);
                Assert.Contains("Nguyễn Văn A", texts);
                Assert.Contains("Tiêu đề bài đăng *", texts);
                Assert.Contains("Nhập tiêu đề ngắn gọn súc tích...", texts);
                Assert.Contains("Danh mục *", texts);
                Assert.Contains("Tin tức chung", texts);
                Assert.Contains("Nội dung chi tiết *", texts);
                Assert.Contains("Bạn muốn chia sẻ điều gì với các bạn sinh viên? Hãy ghi chi tiết để mọi người dễ tương tác...", texts);
                Assert.DoesNotContain("Thuộc khu vực", texts);
                Assert.Contains("Thẻ phụ (Tags)", texts);
                Assert.Contains("ví dụ: #wifi, #passdo, #gopy (cách nhau...)", texts);
                Assert.DoesNotContain("Chọn nhanh thẻ phổ biến", texts);
                Assert.Contains("#thongbao", texts);
                Assert.Contains("#clbsukien", texts);
                Assert.Contains("  ✓", texts);
                Assert.Contains("Hủy", texts);
                Assert.Contains("Đăng bài ngay", texts);

                var bitmap = new RenderTargetBitmap(1600, 1000, 96, 96, PixelFormats.Pbgra32);
                var visual = new DrawingVisual();
                using (var drawingContext = visual.RenderOpen())
                {
                    drawingContext.DrawRectangle(Brushes.White, null, new Rect(0, 0, 1600, 1000));
                    drawingContext.DrawRectangle(new VisualBrush(view), null, new Rect(0, 0, 1600, 1000));
                }

                bitmap.Render(visual);

                var artifactPath = Path.Combine(FindRepositoryRoot(), ".ai", "artifacts", "forum-compose-popup-wpf.png");
                Directory.CreateDirectory(Path.GetDirectoryName(artifactPath)!);

                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));

                using var stream = File.Create(artifactPath);
                encoder.Save(stream);

                Assert.True(new FileInfo(artifactPath).Length > 0, $"Expected popup screenshot artifact '{artifactPath}' to be non-empty.");
            }
            finally
            {
                window.Close();
            }
        });
    }
    [Fact]
    public void Forum_home_popup_xaml_removes_legacy_combo_boxes_and_non_reference_fields()
    {
        var document = LoadForumHomeViewDocument();
        var comboBoxes = document.Root!
            .Descendants(WpfNamespace + "ComboBox")
            .ToArray();

        Assert.Empty(comboBoxes);

        var xaml = document.ToString(SaveOptions.DisableFormatting);
        Assert.DoesNotContain("Thuộc khu vực", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("Thẻ phụ tự viết", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("Chọn nhanh thẻ phổ biến", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("Đóng", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"{Binding ComposeCustomTagsLabel}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Hủy", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void Forum_home_view_renders_recovered_compose_popup_contract_offscreen()
    {
        RunOnStaThread(() =>
        {
            var viewModel = new ForumHomeViewModel();
            viewModel.ComposeCommand.Execute(null);

            var view = new ForumHomeView
            {
                DataContext = viewModel,
                Width = 1600,
                Height = 1000
            };
            var window = new Window
            {
                Width = 1600,
                Height = 1000,
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

                var texts = GetVisibleStringValues(view);

                Assert.Contains("Đăng bài viết mới", texts);
                Assert.Contains("Tiêu đề bài đăng *", texts);
                Assert.Contains("Danh mục *", texts);
                Assert.Contains("Nội dung chi tiết *", texts);
                Assert.Contains("Thẻ phụ (Tags)", texts);
                Assert.Contains("Hủy", texts);
                Assert.Contains("Đăng bài ngay", texts);
                Assert.DoesNotContain("Đăng tải lên bảng tin của cộng đồng KTX", texts);
                Assert.DoesNotContain("Thuộc khu vực", texts);
                Assert.DoesNotContain("Thẻ phụ tự viết", texts);
                Assert.DoesNotContain("Chọn nhanh thẻ phổ biến", texts);
                Assert.DoesNotContain("Đóng", texts);
            }
            finally
            {
                window.Close();
            }
        });
    }
    [Fact]
    public void Compiled_resources_include_forum_post_detail_view_and_tokens()
    {
        var keys = GetCompiledResourceKeys();

        Assert.Contains("views/forum/forumpostdetailview.baml", keys);
        Assert.Contains("resources/forumpostdetail.baml", keys);
    }

    [Fact]
    public void Forum_post_detail_view_renders_required_reference_text_when_hosted_offscreen()
    {
        RunOnStaThread(() =>
        {
            const string viewTypeName = "DormitoryManagement.WPF.Views.Forum.ForumPostDetailView, DormitoryManagement.WPF";
            const string viewModelTypeName = "DormitoryManagement.WPF.ViewModels.Forum.ForumPostDetailViewModel, DormitoryManagement.WPF";

            var viewType = Type.GetType(viewTypeName);
            var viewModelType = Type.GetType(viewModelTypeName);

            Assert.NotNull(viewType);
            Assert.NotNull(viewModelType);

            var view = Assert.IsAssignableFrom<FrameworkElement>(Activator.CreateInstance(viewType!));
            view.DataContext = Activator.CreateInstance(viewModelType!);
            view.Width = 1440;
            view.Height = 1400;

            var window = new Window
            {
                Width = 1440,
                Height = 1400,
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

                var texts = GetVisibleStringValues(view);
                string[] requiredTexts =
                [
                    "DMForum",
                    "Search forum...",
                    "Home",
                    "General News",
                    "Thông báo lịch bảo trì điện nước khu B",
                    "Ban Quản Lý",
                    "Comments (32)",
                    "Add to the discussion...",
                    "Create New Post",
                    "Danh mục",
                    "Related Posts",
                    "Trending Tags"
                ];

                foreach (var requiredText in requiredTexts)
                {
                    Assert.Contains(requiredText, texts);
                }
            }
            finally
            {
                window.Close();
            }
        });
    }
    [Fact]
    public void Forum_post_detail_view_writes_review_screenshot_artifact()
    {
        RunOnStaThread(() =>
        {
            const string viewTypeName = "DormitoryManagement.WPF.Views.Forum.ForumPostDetailView, DormitoryManagement.WPF";
            const string viewModelTypeName = "DormitoryManagement.WPF.ViewModels.Forum.ForumPostDetailViewModel, DormitoryManagement.WPF";

            EnsureApplicationResources();

            var viewType = Type.GetType(viewTypeName);
            var viewModelType = Type.GetType(viewModelTypeName);

            Assert.NotNull(viewType);
            Assert.NotNull(viewModelType);

            var view = Assert.IsAssignableFrom<FrameworkElement>(Activator.CreateInstance(viewType!));
            view.DataContext = Activator.CreateInstance(viewModelType!);
            view.Width = 1440;
            view.Height = 1080;

            var host = new Border
            {
                Width = 1440,
                Height = 1080,
                Background = (Brush)new BrushConverter().ConvertFromString("#F8F9FF")!,
                Child = view
            };

            var window = new Window
            {
                Width = 1440,
                Height = 1080,
                Content = host,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Left = -10000,
                Top = -10000,
                ShowInTaskbar = false,
                Background = Brushes.White
            };

            try
            {
                window.Show();
                WaitForLayout();

                var repoRoot = FindRepositoryRoot();
                var artifactDirectory = Path.Combine(repoRoot, ".ai", "artifacts");
                Directory.CreateDirectory(artifactDirectory);
                var artifactPath = Path.Combine(artifactDirectory, "forum-post-detail-wpf.png");

                SaveFrameworkElementAsPng(host, artifactPath);

                Assert.True(File.Exists(artifactPath), $"Expected screenshot artifact '{artifactPath}' to exist.");
                Assert.True(new FileInfo(artifactPath).Length > 0, $"Expected screenshot artifact '{artifactPath}' to be non-empty.");
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void Student_dashboard_resources_define_exact_reference_color_tokens()
    {
        var document = LoadStudentDashboardResourceDocument();

        AssertResourceValue(document, "Color", "StudentDashboardColorCanvas", "#F8F9FF");
        AssertResourceValue(document, "Color", "StudentDashboardColorSurface", "#FFFFFF");
        AssertResourceValue(document, "Color", "StudentDashboardColorSurfaceLow", "#EFF4FF");
        AssertResourceValue(document, "Color", "StudentDashboardColorSurfaceContainer", "#E5EEFF");
        AssertResourceValue(document, "Color", "StudentDashboardColorSurfaceHigh", "#DCE9FF");
        AssertResourceValue(document, "Color", "StudentDashboardColorSurfaceHighest", "#D3E4FE");
        AssertResourceValue(document, "Color", "StudentDashboardColorTextPrimary", "#0B1C30");
        AssertResourceValue(document, "Color", "StudentDashboardColorTextSecondary", "#594139");
        AssertResourceValue(document, "Color", "StudentDashboardColorPrimary", "#AB3500");
        AssertResourceValue(document, "Color", "StudentDashboardColorPrimaryContainer", "#FF6B35");
        AssertResourceValue(document, "Color", "StudentDashboardColorSecondary", "#4648D4");
        AssertResourceValue(document, "Color", "StudentDashboardColorSecondaryContainer", "#6063EE");
        AssertResourceValue(document, "Color", "StudentDashboardColorTertiary", "#006C49");
        AssertResourceValue(document, "Color", "StudentDashboardColorTertiaryContainer", "#00AF79");
        AssertResourceValue(document, "Color", "StudentDashboardColorOutline", "#8D7168");
        AssertResourceValue(document, "Color", "StudentDashboardColorOutlineVariant", "#E1BFB5");
        AssertResourceValue(document, "Color", "StudentDashboardColorError", "#BA1A1A");
        AssertResourceValue(document, "Color", "StudentDashboardColorErrorContainer", "#FFDAD6");
        AssertResourceValue(document, "Color", "StudentDashboardColorOnErrorContainer", "#93000A");
    }

    [Fact]
    public void Student_dashboard_resources_define_exact_reference_spacing_radius_and_size_tokens()
    {
        var document = LoadStudentDashboardResourceDocument();

        AssertResourceValue(document, "Double", "StudentDashboardHeaderHeight", "64");
        AssertResourceValue(document, "Double", "StudentDashboardTopBarVerticalPadding", "12");
        AssertResourceValue(document, "Double", "StudentDashboardTopBarHorizontalPaddingDesktop", "32");
        AssertResourceValue(document, "Double", "StudentDashboardPagePaddingDesktop", "24");
        AssertResourceValue(document, "Double", "StudentDashboardPagePaddingMobile", "16");
        AssertResourceValue(document, "Double", "StudentDashboardSectionGap", "20");
        AssertResourceValue(document, "Double", "StudentDashboardGridGap", "16");
        AssertResourceValue(document, "Double", "StudentDashboardTileGap", "12");
        AssertResourceValue(document, "Double", "StudentDashboardCardPadding", "20");
        AssertResourceValue(document, "Double", "StudentDashboardIconCircleSize", "44");
        AssertResourceValue(document, "Double", "StudentDashboardCardValueLarge", "28");
        AssertResourceValue(document, "Double", "StudentDashboardCardValueMedium", "20");
        AssertResourceValue(document, "Double", "StudentDashboardBodyLargeSize", "14");
        AssertResourceValue(document, "Double", "StudentDashboardBodySmallSize", "14");
        AssertResourceValue(document, "Double", "StudentDashboardQuickActionTileWidth", "248");

        AssertResourceValue(document, "CornerRadius", "StudentDashboardCardCornerRadius", "12");
        AssertResourceValue(document, "CornerRadius", "StudentDashboardControlCornerRadius", "8");
        AssertResourceValue(document, "CornerRadius", "StudentDashboardPillCornerRadius", "999");
        AssertResourceValue(document, "CornerRadius", "StudentDashboardStatusBadgeCornerRadius", "8");

        AssertResourceValue(document, "Thickness", "StudentDashboardCardPaddingThickness", "20");
        AssertResourceValue(document, "Thickness", "StudentDashboardQuickActionTilePadding", "16");
        AssertResourceValue(document, "Thickness", "StudentDashboardRefreshButtonPadding", "14,7");
        AssertResourceValue(document, "Thickness", "StudentDashboardStatusPillPadding", "12,4");
    }

    [Fact]
    public void Student_dashboard_text_styles_define_exact_reference_font_sizes_weights_and_line_heights()
    {
        var document = LoadStudentDashboardResourceDocument();

        AssertStyleSetter(document, "StudentDashboardGreetingTextStyle", "FontSize", "28");
        AssertStyleSetter(document, "StudentDashboardGreetingTextStyle", "FontWeight", "SemiBold");
        AssertStyleSetter(document, "StudentDashboardGreetingTextStyle", "LineHeight", "34");
        AssertStyleSetter(document, "StudentDashboardSupportingTextStyle", "FontSize", "14");
        AssertStyleSetter(document, "StudentDashboardSupportingTextStyle", "LineHeight", "20");
        AssertStyleSetter(document, "StudentDashboardCardTitleTextStyle", "FontSize", "15");
        AssertStyleSetter(document, "StudentDashboardCardTitleTextStyle", "FontWeight", "SemiBold");
        AssertStyleSetter(document, "StudentDashboardCardTitleTextStyle", "LineHeight", "22");
        AssertStyleSetter(document, "StudentDashboardMetricValueTextStyle", "FontSize", "28");
        AssertStyleSetter(document, "StudentDashboardMetricValueTextStyle", "FontWeight", "Bold");
        AssertStyleSetter(document, "StudentDashboardMetricValueTextStyle", "LineHeight", "30");
        AssertStyleSetter(document, "StudentDashboardQuickActionTextStyle", "FontSize", "13");
        AssertStyleSetter(document, "StudentDashboardQuickActionTextStyle", "FontWeight", "SemiBold");
        AssertStyleSetter(document, "StudentDashboardQuickActionTextStyle", "LineHeight", "18");
    }


    [Fact]
    public void Student_dashboard_reviewed_greeting_typography_uses_tuned_wpf_weight()
    {
        var document = LoadStudentDashboardResourceDocument();

        AssertStyleSetter(document, "StudentDashboardGreetingTextStyle", "FontWeight", "SemiBold");
    }

    [Fact]
    public void Student_dashboard_resources_define_reference_font_families_shadow_and_border_tokens()
    {
        var document = LoadStudentDashboardResourceDocument();

        AssertResourceValue(document, "FontFamily", "StudentDashboardHeadlineFontFamily", "pack://application:,,,/DormitoryManagement.WPF;component/Assets/Fonts/PlusJakartaSans/#Plus Jakarta Sans");
        AssertResourceValue(document, "FontFamily", "StudentDashboardBodyFontFamily", "pack://application:,,,/DormitoryManagement.WPF;component/Assets/Fonts/Inter/#Inter");
        AssertResourceValue(document, "Double", "StudentDashboardCardBorderThickness", "1");
        AssertResourceValue(document, "Double", "StudentDashboardCardShadowBlur", "12");
        AssertResourceValue(document, "Double", "StudentDashboardCardShadowDepth", "2");
        AssertResourceValue(document, "Double", "StudentDashboardCardShadowOpacity", "0.03");
    }
    [Fact]
    public void Student_dashboard_view_defines_summary_cards_in_reference_order()
    {
        var document = LoadStudentDashboardViewDocument();
        var texts = document.Root!
            .Descendants(WpfNamespace + "TextBlock")
            .Select(element => element.Attribute("Text")?.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToList();

        AssertTextOrder(
            texts!,
            "{Binding RoomCardTitle}",
            "{Binding InvoiceCardTitle}",
            "{Binding TicketCardTitle}",
            "{Binding VehicleCardTitle}");
    }

    [Fact]
    public void Student_dashboard_view_defines_expected_card_icons_and_status_surfaces()
    {
        var document = LoadStudentDashboardViewDocument();
        var packIconKinds = document.Root!
            .Descendants()
            .Where(element => string.Equals(element.Name.LocalName, "PackIconMaterial", StringComparison.Ordinal))
            .Select(element => element.Attribute("Kind")?.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToArray();

        Assert.Contains("Door", packIconKinds);
        Assert.Contains("FileDocumentOutline", packIconKinds);
        Assert.Contains("Headset", packIconKinds);
        Assert.Contains("Bicycle", packIconKinds);

        var borderValues = document.Root!
            .Descendants(WpfNamespace + "Border")
            .Select(element => new
            {
                Background = element.Attribute("Background")?.Value,
                BorderBrush = element.Attribute("BorderBrush")?.Value
            })
            .ToArray();

        Assert.Contains(borderValues, value => value.Background == "{StaticResource StudentDashboardBadgeDangerBackgroundBrush}");
        Assert.Contains(borderValues, value => value.Background == "{StaticResource StudentDashboardBadgeSuccessBackgroundBrush}" && value.BorderBrush == "{StaticResource StudentDashboardBadgeSuccessBorderBrush}");
    }


    [Fact]
    public void Student_dashboard_view_reflows_cards_without_horizontal_overflow_at_narrower_desktop_width()
    {
        RunOnStaThread(() =>
        {
            EnsureApplicationResources();
            var viewModel = CreateStudentDashboardViewModel();
            var view = new StudentDashboardView
            {
                DataContext = viewModel,
                Width = 1024,
                Height = 900
            };
            var window = new Window
            {
                Width = 1024,
                Height = 900,
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
                WaitUntil(() => viewModel.CurrentRoom == "A-101");

                var scrollViewer = FindDescendants<ScrollViewer>(view).OrderByDescending(candidate => candidate.ActualWidth * candidate.ActualHeight).First();
                Assert.True(scrollViewer.ScrollableWidth <= 0.5, $"Expected no horizontal overflow, but ScrollableWidth={scrollViewer.ScrollableWidth}.");

                var roomTitle = FindVisibleTextBlock(view, "Phòng của tôi");
                var ticketTitle = FindVisibleTextBlock(view, "Yêu cầu hỗ trợ");
                var vehicleTitle = FindVisibleTextBlock(view, "Đăng ký xe");

                var roomBounds = GetBoundsRelativeToAncestor(roomTitle, view);
                var ticketBounds = GetBoundsRelativeToAncestor(ticketTitle, view);
                var vehicleBounds = GetBoundsRelativeToAncestor(vehicleTitle, view);

                Assert.True(
                    ticketBounds.Top > roomBounds.Top + 40 || vehicleBounds.Top > roomBounds.Top + 40,
                    $"Expected summary cards to wrap into multiple rows at narrower width. RoomTop={roomBounds.Top}, TicketTop={ticketBounds.Top}, VehicleTop={vehicleBounds.Top}.");
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void Student_dashboard_shell_keeps_sidebar_available_on_student_route()
    {
        var document = LoadShellViewDocument();
        var sidebarPanel = document.Root!
            .Descendants(WpfNamespace + "Border")
            .First(element => string.Equals(element.Attribute(XamlNamespace + "Name")?.Value, "SidebarPanel", StringComparison.Ordinal));
        var sidebarHotspot = document.Root!
            .Descendants(WpfNamespace + "Border")
            .First(element => string.Equals(element.Attribute(XamlNamespace + "Name")?.Value, "SidebarHotspot", StringComparison.Ordinal));

        Assert.Null(sidebarPanel.Attribute("Visibility"));
        Assert.Null(sidebarHotspot.Attribute("Visibility"));
    }

    [Fact]
    public void Student_dashboard_shell_stays_without_horizontal_overflow_when_sidebar_opens()
    {
        RunOnStaThread(() =>
        {
            EnsureApplicationResources();
            var shellViewModel = CreateStudentDashboardShellViewModel(out var dashboardViewModel);
            var view = new ShellView
            {
                DataContext = shellViewModel,
                Width = 1280,
                Height = 900
            };
            var window = new Window
            {
                Width = 1280,
                Height = 900,
                Content = view,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Left = -10000,
                Top = -10000,
                ShowInTaskbar = false,
                Background = Brushes.White
            };

            try
            {
                window.Show();
                WaitForLayout();
                WaitUntil(() => dashboardViewModel.CurrentRoom == "A-101");
                WaitUntil(() => shellViewModel.IsStudentDashboardChrome);

                var showSidebar = typeof(ShellView).GetMethod("ShowSidebar", BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.NotNull(showSidebar);
                showSidebar!.Invoke(view, null);
                WaitForLayout();

                var dashboard = Assert.Single(FindDescendants<StudentDashboardView>(view), candidate => candidate.IsVisible);
                var scrollViewer = Assert.Single(FindDescendants<ScrollViewer>(dashboard), candidate => candidate.IsVisible && candidate.ActualHeight > 0);

                Assert.True(scrollViewer.ScrollableWidth <= 0.5, $"Expected no horizontal overflow with sidebar open, but ScrollableWidth={scrollViewer.ScrollableWidth}.");
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void Student_dashboard_shell_shows_all_eight_dashboard_boxes_without_vertical_scroll_at_standard_desktop_height()
    {
        RunOnStaThread(() =>
        {
            EnsureApplicationResources();
            var shellViewModel = CreateStudentDashboardShellViewModel(out var dashboardViewModel);
            var view = new ShellView
            {
                DataContext = shellViewModel,
                Width = 1440,
                Height = 1080
            };
            var window = new Window
            {
                Width = 1440,
                Height = 1080,
                Content = view,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Left = -10000,
                Top = -10000,
                ShowInTaskbar = false,
                Background = Brushes.White
            };

            try
            {
                window.Show();
                WaitForLayout();
                WaitUntil(() => dashboardViewModel.CurrentRoom == "A-101");
                WaitUntil(() => shellViewModel.IsStudentDashboardChrome);

                var dashboard = Assert.Single(FindDescendants<StudentDashboardView>(view), candidate => candidate.IsVisible);
                var scrollViewer = Assert.Single(FindDescendants<ScrollViewer>(dashboard), candidate => candidate.IsVisible && candidate.ActualHeight > 0);

                Assert.True(scrollViewer.ScrollableHeight <= 0.5, $"Expected all eight dashboard boxes to fit without vertical scroll, but ScrollableHeight={scrollViewer.ScrollableHeight}.");
            }
            finally
            {
                window.Close();
            }
        });
    }
    [Fact]
    public void Student_dashboard_view_writes_review_screenshot_artifact()
    {
        RunOnStaThread(() =>
        {
            EnsureApplicationResources();
            var shellViewModel = CreateStudentDashboardShellViewModel(out var dashboardViewModel);
            var view = new ShellView
            {
                DataContext = shellViewModel,
                Width = 1440,
                Height = 1080
            };
            var window = new Window
            {
                Width = 1440,
                Height = 1080,
                Content = view,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Left = -10000,
                Top = -10000,
                ShowInTaskbar = false,
                Background = Brushes.White
            };

            try
            {
                window.Show();
                WaitForLayout();
                WaitUntil(() => dashboardViewModel.CurrentRoom == "A-101");
                WaitUntil(() => shellViewModel.IsStudentDashboardChrome);

                var repoRoot = FindRepositoryRoot();
                var artifactDirectory = Path.Combine(repoRoot, ".ai", "artifacts");
                Directory.CreateDirectory(artifactDirectory);
                var artifactPath = Path.Combine(artifactDirectory, "student-dashboard-wpf.png");

                SaveFrameworkElementAsPng(view, artifactPath);

                Assert.True(File.Exists(artifactPath), $"Expected screenshot artifact '{artifactPath}' to exist.");
                Assert.True(new FileInfo(artifactPath).Length > 0, $"Expected screenshot artifact '{artifactPath}' to be non-empty.");
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void Student_dashboard_popup_writes_review_screenshot_artifact()
    {
        RunOnStaThread(() =>
        {
            EnsureApplicationResources();
            var viewModel = CreateStudentDashboardPopupViewModel();
            var view = new StudentDashboardView
            {
                DataContext = viewModel,
                Width = 1440,
                Height = 1080
            };
            var window = new Window
            {
                Width = 1440,
                Height = 1080,
                Content = view,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Left = -10000,
                Top = -10000,
                ShowInTaskbar = false,
                Background = Brushes.White
            };

            try
            {
                window.Show();
                WaitForLayout();
                WaitUntil(() => viewModel.CanOpenRoomRegistrationPopup);

                viewModel.OpenRoomRegistrationPopupCommand.Execute(null);

                WaitUntil(() => viewModel.IsRoomRegistrationPopupOpen && viewModel.RoomRegistrationModal is not null);
                WaitUntil(() => viewModel.RoomRegistrationModal!.HasAvailableRooms);

                var texts = GetVisibleStringValues(view);
                Assert.Contains("Đăng ký phòng mới", texts);
                Assert.Contains("Vui lòng chọn thông tin phòng để gửi yêu cầu đăng ký.", texts);
                Assert.Contains("Gửi yêu cầu đăng ký", texts);

                var repoRoot = FindRepositoryRoot();
                var artifactDirectory = Path.Combine(repoRoot, ".ai", "artifacts");
                Directory.CreateDirectory(artifactDirectory);
                var artifactPath = Path.Combine(artifactDirectory, "student-dashboard-room-popup-wpf.png");

                var popupSurface = FindDescendants<Border>(view)
                    .Where(border => border.IsVisible && border.ActualWidth >= 650 && border.ActualHeight >= 500)
                    .OrderByDescending(border => border.ActualWidth * border.ActualHeight)
                    .First();

                SaveFrameworkElementAsPng(popupSurface, artifactPath);

                Assert.True(File.Exists(artifactPath), $"Expected screenshot artifact '{artifactPath}' to exist.");
                Assert.True(new FileInfo(artifactPath).Length > 0, $"Expected screenshot artifact '{artifactPath}' to be non-empty.");
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void Student_dashboard_popup_xaml_matches_reference_copy_and_field_order()
    {
        var xaml = LoadStudentDashboardViewDocument().ToString(SaveOptions.DisableFormatting);

        Assert.Contains("Đăng ký phòng mới", xaml, StringComparison.Ordinal);
        Assert.Contains("Vui lòng chọn thông tin phòng để gửi yêu cầu đăng ký.", xaml, StringComparison.Ordinal);
        Assert.Contains("Khu vực", xaml, StringComparison.Ordinal);
        Assert.Contains("Tầng", xaml, StringComparison.Ordinal);
        Assert.Contains("Loại phòng", xaml, StringComparison.Ordinal);
        Assert.Contains("Số phòng", xaml, StringComparison.Ordinal);
        Assert.Contains("Thời hạn hợp đồng", xaml, StringComparison.Ordinal);
        Assert.Contains("Ghi chú", xaml, StringComparison.Ordinal);
        Assert.Contains("Tôi đồng ý với nội quy KTX và cam kết thanh toán đúng hạn.", xaml, StringComparison.Ordinal);
        Assert.Contains("Hủy", xaml, StringComparison.Ordinal);
        Assert.Contains("Gửi yêu cầu đăng ký", xaml, StringComparison.Ordinal);

        AssertStringOrder(
            xaml,
            "Khu vực",
            "Tầng",
            "Loại phòng",
            "Số phòng",
            "Thời hạn hợp đồng",
            "Ghi chú",
            "Tôi đồng ý với nội quy KTX và cam kết thanh toán đúng hạn.");
    }

    [Fact]
    public void Student_dashboard_popup_xaml_removes_legacy_visible_registration_controls()
    {
        var xaml = LoadStudentDashboardViewDocument().ToString(SaveOptions.DisableFormatting);

        Assert.DoesNotContain("GenderOptions", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("PriceSortOptions", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("IncludesInternet", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("Kèm internet", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("<ListBox ItemsSource=\"{Binding AvailableRooms}\"", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("Chọn phòng", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("Xác nhận đăng ký", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("Popup Đăng ký phòng - KTX Lumina", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void Student_dashboard_popup_resources_define_reference_shell_tokens()
    {
        var document = LoadStudentDashboardResourceDocument();

        AssertResourceValue(document, "Double", "StudentDashboardPopupWidth", "672");
        AssertResourceValue(document, "Double", "StudentDashboardPopupMaxHeight", "760");
        AssertResourceValue(document, "Double", "StudentDashboardPopupShadowBlur", "50");
        AssertResourceValue(document, "Double", "StudentDashboardPopupShadowDepth", "8");
        AssertResourceValue(document, "Double", "StudentDashboardPopupShadowOpacity", "0.12");
        AssertResourceValue(document, "CornerRadius", "StudentDashboardPopupCornerRadius", "24");
        AssertResourceValue(document, "Thickness", "StudentDashboardPopupMargin", "16");
        AssertStyleSetter(document, "StudentDashboardPopupBorderStyle", "CornerRadius", "{StaticResource StudentDashboardPopupCornerRadius}");
        AssertStyleSetter(document, "StudentDashboardPopupBorderStyle", "MaxWidth", "{StaticResource StudentDashboardPopupWidth}");
    }

    [Fact]
    public void Student_dashboard_popup_xaml_uses_shared_vehicle_dropdown_style()
    {
        var xaml = LoadStudentDashboardViewDocument().ToString(SaveOptions.DisableFormatting);

        Assert.Equal(5, xaml.Split("Style=\"{StaticResource MaterialDesignOutlinedComboBox}\"", StringSplitOptions.None).Length - 1);
        Assert.Contains("xmlns:materialDesign=\"http://materialdesigninxaml.net/winfx/xaml/themes\"", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void Student_dashboard_popup_stays_within_bounds_in_compact_window()
    {
        RunOnStaThread(() =>
        {
            EnsureApplicationResources();
            var viewModel = CreateStudentDashboardPopupViewModel();
            var view = new StudentDashboardView
            {
                DataContext = viewModel,
                Width = 920,
                Height = 760
            };
            var window = new Window
            {
                Width = 920,
                Height = 760,
                Content = view,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Left = -10000,
                Top = -10000,
                ShowInTaskbar = false,
                Background = Brushes.White
            };

            try
            {
                window.Show();
                WaitForLayout();
                WaitUntil(() => viewModel.CanOpenRoomRegistrationPopup);

                viewModel.OpenRoomRegistrationPopupCommand.Execute(null);

                WaitUntil(() => viewModel.IsRoomRegistrationPopupOpen && viewModel.RoomRegistrationModal is not null);

                var popupSurface = FindDescendants<Border>(view)
                    .Where(border => border.IsVisible && border.ActualWidth >= 320 && border.ActualHeight >= 420)
                    .OrderByDescending(border => border.ActualWidth * border.ActualHeight)
                    .First();
                var popupBounds = GetBoundsRelativeToAncestor(popupSurface, view);

                Assert.True(popupBounds.Left >= 0);
                Assert.True(popupBounds.Top >= 0);
                Assert.True(popupBounds.Right <= view.ActualWidth);
                Assert.True(popupBounds.Bottom <= view.ActualHeight);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void Forum_home_view_renders_activity_popup_from_activity_header_offscreen()
    {
        RunOnStaThread(() =>
        {
            var viewModel = new ForumHomeViewModel();
            viewModel.OpenActivityQuickAddCommand.Execute(null);

            var view = new ForumHomeView
            {
                DataContext = viewModel,
                Width = 1600,
                Height = 1000
            };
            var window = new Window
            {
                Width = 1600,
                Height = 1000,
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

                var texts = GetVisibleStringValues(view);

                Assert.Contains("Create New Activity", texts);
                Assert.Contains("Activity Name", texts);
                Assert.Contains("Category", texts);
                Assert.Contains("Sports", texts);
                Assert.Contains("Music", texts);
                Assert.Contains("Study Group", texts);
                Assert.Contains("Volunteer", texts);
                Assert.Contains("Date", texts);
                Assert.Contains("Time", texts);
                Assert.Contains("Location", texts);
                Assert.Contains("Description", texts);
                Assert.Contains("Optional", texts);
                Assert.Contains("Cancel", texts);
                Assert.Contains("Create Activity", texts);
                Assert.DoesNotContain("Tiêu đề bài đăng *", texts);
                Assert.DoesNotContain("Nội dung chi tiết *", texts);
                Assert.DoesNotContain("Đăng bài ngay", texts);

                var bitmap = new RenderTargetBitmap(1600, 1000, 96, 96, PixelFormats.Pbgra32);
                var visual = new DrawingVisual();
                using (var drawingContext = visual.RenderOpen())
                {
                    drawingContext.DrawRectangle(Brushes.White, null, new Rect(0, 0, 1600, 1000));
                    drawingContext.DrawRectangle(new VisualBrush(view), null, new Rect(0, 0, 1600, 1000));
                }

                bitmap.Render(visual);

                var artifactPath = Path.Combine(FindRepositoryRoot(), ".ai", "artifacts", "forum-activity-popup-wpf.png");
                Directory.CreateDirectory(Path.GetDirectoryName(artifactPath)!);

                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));

                using var stream = File.Create(artifactPath);
                encoder.Save(stream);

                Assert.True(new FileInfo(artifactPath).Length > 0, $"Expected activity popup screenshot artifact '{artifactPath}' to be non-empty.");
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void Forum_home_xaml_defines_activity_popup_contract_bindings()
    {
        var xaml = LoadForumHomeViewDocument().ToString(SaveOptions.DisableFormatting);

        Assert.Contains("ShowActivityDialog", xaml, StringComparison.Ordinal);
        Assert.Contains("ActivityDialogTitle", xaml, StringComparison.Ordinal);
        Assert.Contains("OpenActivityQuickAddCommand", xaml, StringComparison.Ordinal);
        Assert.Contains("SelectActivityCategoryCommand", xaml, StringComparison.Ordinal);
        Assert.Contains("SubmitActivityPreviewCommand", xaml, StringComparison.Ordinal);
        Assert.Contains("CloseActivityDialogCommand", xaml, StringComparison.Ordinal);
        Assert.Contains("Create New Activity", xaml, StringComparison.Ordinal);
        Assert.Contains("Create Activity", xaml, StringComparison.Ordinal);
        Assert.Contains("Cancel", xaml, StringComparison.Ordinal);
    }
    [Fact]
    public void Compiled_resources_include_vehicle_registration_view_and_tokens()
    {
        var keys = GetCompiledResourceKeys();

        Assert.Contains("views/vehicles/vehicleregistrationview.baml", keys);
        Assert.Contains("resources/vehicleregistration.baml", keys);
    }

    [Fact]
    public void Vehicle_registration_resources_define_compact_recovery_typography_and_geometry_tokens()
    {
        var document = LoadVehicleRegistrationResourceDocument();

        AssertResourceValue(document, "Double", "VehicleRegistrationSectionGap", "20");
        AssertResourceValue(document, "Double", "VehicleRegistrationCardPaddingDesktop", "24");
        AssertResourceValue(document, "GridLength", "VehicleRegistrationPaymentColumnWidth", "320");
        AssertResourceValue(document, "Double", "VehicleRegistrationQrFrameSize", "168");
        AssertResourceValue(document, "Double", "VehicleRegistrationQrCodeSize", "136");
        AssertResourceValue(document, "Double", "VehicleRegistrationPageTitleSize", "30");
        AssertResourceValue(document, "Double", "VehicleRegistrationSectionTitleSize", "22");
        AssertResourceValue(document, "Double", "VehicleRegistrationHelperTextSize", "12");
        AssertResourceValue(document, "Double", "VehicleRegistrationPriceTextSize", "22");
        AssertResourceValue(document, "Double", "VehicleRegistrationButtonMinWidth", "120");
        AssertResourceValue(document, "Double", "VehicleRegistrationVerifiedBadgeMaxWidth", "200");
        AssertResourceValue(document, "Double", "VehicleRegistrationVerifiedBadgeIconSize", "12");
        AssertResourceValue(document, "Double", "VehicleRegistrationVerifiedBadgeTextSize", "11");

        AssertResourceValue(document, "Thickness", "VehicleRegistrationCardPadding", "24");
        AssertResourceValue(document, "Thickness", "VehicleRegistrationSectionDividerMargin", "0,14,0,20");
        AssertResourceValue(document, "Thickness", "VehicleRegistrationPrimaryButtonPadding", "20,0");
        AssertResourceValue(document, "Thickness", "VehicleRegistrationVerifiedBadgePadding", "10,4");

        AssertStyleSetter(document, "VehicleRegistrationPageTitleTextStyle", "FontSize", "{StaticResource VehicleRegistrationPageTitleSize}");
        AssertStyleSetter(document, "VehicleRegistrationSectionTitleTextStyle", "FontSize", "{StaticResource VehicleRegistrationSectionTitleSize}");
        AssertStyleSetter(document, "VehicleRegistrationHelperTextStyle", "FontSize", "{StaticResource VehicleRegistrationHelperTextSize}");
        AssertStyleSetter(document, "VehicleRegistrationPriceTextStyle", "FontSize", "{StaticResource VehicleRegistrationPriceTextSize}");
        AssertStyleSetter(document, "VehicleRegistrationPrimaryButtonStyle", "MinWidth", "{StaticResource VehicleRegistrationButtonMinWidth}");
        AssertStyleSetter(document, "VehicleRegistrationPrimaryButtonStyle", "Height", "{StaticResource VehicleRegistrationPrimaryButtonHeight}");

        var buttonShadow = document.Root!
            .Elements(WpfNamespace + "DropShadowEffect")
            .First(element => string.Equals(element.Attribute(XamlNamespace + "Key")?.Value, "VehicleRegistrationButtonShadowEffect", StringComparison.Ordinal));

        Assert.Equal("12", buttonShadow.Attribute("BlurRadius")?.Value);
        Assert.Equal("0.14", buttonShadow.Attribute("Opacity")?.Value);
        Assert.Equal("2", buttonShadow.Attribute("ShadowDepth")?.Value);
    }

    [Fact]
    public void Shared_topbar_xaml_uses_dashboard_chrome_for_all_non_forum_routes_and_avatar_opens_profile()
    {
        var document = LoadTopBarDocument();
        var sharedChromeBorder = document.Root!
            .Descendants(WpfNamespace + "Border")
            .First(element => string.Equals(element.Attribute("Visibility")?.Value, "{Binding IsSharedTopBarChrome, Converter={StaticResource BoolToVisibility}}", StringComparison.Ordinal));
        var xaml = sharedChromeBorder.ToString(SaveOptions.DisableFormatting);

        Assert.Contains("DormManagement", xaml);
        Assert.Contains("CurrentPageTitle", xaml);
        Assert.Contains("CommandParameter=\"StudentProfile\"", xaml);
        Assert.DoesNotContain("Lumina Community", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("Content=\"Dashboard\"", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("Content=\"My Room\"", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("Content=\"Invoices\"", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("Content=\"Vehicles\"", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("Content=\"Forum\"", xaml, StringComparison.Ordinal);
    }
[Fact]
    public void Vehicle_registration_view_removes_payment_blob_and_keeps_compact_two_column_contract()
    {
        var xaml = LoadVehicleRegistrationViewDocument().ToString(SaveOptions.DisableFormatting);

        Assert.DoesNotContain("#33FFB59D", xaml, StringComparison.Ordinal);
        Assert.Contains("Width=\"{StaticResource VehicleRegistrationPaymentColumnWidth}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Width=\"{StaticResource VehicleRegistrationQrFrameSize}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Padding=\"{StaticResource VehicleRegistrationVerifiedBadgePadding}\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"Ngày đăng ký\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"Biển số xe\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"Thời hạn\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"Tổng tiền\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"Trạng thái\"", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void Vehicle_registration_view_can_render_offscreen_and_capture_recovery_artifact()
    {
        RunOnStaThread(() =>
        {
            EnsureApplicationResources();
            var viewModel = new VehicleRegistrationViewModel(new VehicleRegistrationStubVehicleService());
            var view = new VehicleRegistrationView
            {
                DataContext = viewModel,
                Width = 1440,
                Height = 1080
            };
            var window = new Window
            {
                Width = 1440,
                Height = 1080,
                Content = view,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Left = -10000,
                Top = -10000,
                ShowInTaskbar = false,
                Background = Brushes.White
            };

            try
            {
                window.Show();
                WaitForLayout();
                WaitUntil(() => viewModel.HistoryReviewRows.Count == 3);

                var repoRoot = FindRepositoryRoot();
                var artifactPath = Path.Combine(repoRoot, ".ai", "artifacts", "vehicles-qa-recovered.png");
                SaveFrameworkElementAsPng(view, artifactPath);

                Assert.True(File.Exists(artifactPath));
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void Vehicle_registration_view_reflows_without_horizontal_overflow_at_narrower_desktop_width()
    {
        RunOnStaThread(() =>
        {
            EnsureApplicationResources();
            var viewModel = new VehicleRegistrationViewModel(new VehicleRegistrationStubVehicleService());
            var view = new VehicleRegistrationView
            {
                DataContext = viewModel,
                Width = 1024,
                Height = 900
            };
            var window = new Window
            {
                Width = 1024,
                Height = 900,
                Content = view,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Left = -10000,
                Top = -10000,
                ShowInTaskbar = false,
                Background = Brushes.White
            };

            try
            {
                window.Show();
                WaitForLayout();
                WaitUntil(() => viewModel.HistoryReviewRows.Count == 3);

                var scrollViewer = FindDescendants<ScrollViewer>(view).Where(candidate => candidate.IsVisible && candidate.ActualHeight > 0).OrderByDescending(candidate => candidate.ActualWidth * candidate.ActualHeight).First();
                Assert.True(scrollViewer.ScrollableWidth <= 0.5, $"Expected no horizontal overflow, but ScrollableWidth={scrollViewer.ScrollableWidth}.");

                var registrationTitle = FindVisibleTextBlock(view, "Thông tin đăng ký");
                var paymentTitle = FindVisibleTextBlock(view, "Thanh toán chuyển khoản");
                var paymentBounds = GetBoundsRelativeToAncestor(paymentTitle, view);
                var registrationBounds = GetBoundsRelativeToAncestor(registrationTitle, view);

                Assert.True(paymentBounds.Right <= view.ActualWidth + 0.5, $"Expected payment card within viewport. Right={paymentBounds.Right}, ViewWidth={view.ActualWidth}.");
                Assert.True(paymentBounds.Left > registrationBounds.Left, "Expected payment card to remain positioned after the registration card.");
            }
            finally
            {
                window.Close();
            }
        });
    }
    private static XDocument LoadMainWindowDocument()
    {
        var repoRoot = FindRepositoryRoot();
        var viewPath = Path.Combine(repoRoot, "src", "DormitoryManagement.WPF", "Views", "Shared", "MainWindow.xaml");

        Assert.True(File.Exists(viewPath), $"Expected main window view '{viewPath}' to exist.");
        return XDocument.Load(viewPath);
    }

    private static XDocument LoadShellViewDocument()
    {
        var repoRoot = FindRepositoryRoot();
        var viewPath = Path.Combine(repoRoot, "src", "DormitoryManagement.WPF", "Views", "Shared", "ShellView.xaml");

        Assert.True(File.Exists(viewPath), $"Expected shell view '{viewPath}' to exist.");
        return XDocument.Load(viewPath);
    }


    private static XDocument LoadTopBarDocument()
    {
        var repoRoot = FindRepositoryRoot();
        var viewPath = Path.Combine(repoRoot, "src", "DormitoryManagement.WPF", "Views", "Shared", "TopBar.xaml");

        Assert.True(File.Exists(viewPath), $"Expected top bar view '{viewPath}' to exist.");
        return XDocument.Load(viewPath);
    }

    private static XDocument LoadVehicleRegistrationViewDocument()
    {
        var repoRoot = FindRepositoryRoot();
        var viewPath = Path.Combine(repoRoot, "src", "DormitoryManagement.WPF", "Views", "Vehicles", "VehicleRegistrationView.xaml");

        Assert.True(File.Exists(viewPath), $"Expected vehicle registration view '{viewPath}' to exist.");
        return XDocument.Load(viewPath);
    }

    private static XDocument LoadVehicleRegistrationResourceDocument()
    {
        var repoRoot = FindRepositoryRoot();
        var viewPath = Path.Combine(repoRoot, "src", "DormitoryManagement.WPF", "Resources", "VehicleRegistration.xaml");

        Assert.True(File.Exists(viewPath), $"Expected vehicle registration resources '{viewPath}' to exist.");
        return XDocument.Load(viewPath);
    }

    [Fact]
    public void Compiled_resources_include_login_view_and_tokens()
    {
        var keys = GetCompiledResourceKeys();

        Assert.Contains("views/auth/loginview.baml", keys);
        Assert.Contains("resources/loginpage.baml", keys);
    }

    [Fact]
    public void Login_page_asset_directories_exist_and_are_not_empty()
    {
        var repoRoot = FindRepositoryRoot();
        var interDir = Path.Combine(repoRoot, "src", "DormitoryManagement.WPF", "Assets", "Fonts", "Inter");
        var plusJakartaDir = Path.Combine(repoRoot, "src", "DormitoryManagement.WPF", "Assets", "Fonts", "PlusJakartaSans");
        var imagesDir = Path.Combine(repoRoot, "src", "DormitoryManagement.WPF", "Assets", "Images", "Login");

        AssertDirectoryHasFiles(interDir);
        AssertDirectoryHasFiles(plusJakartaDir);
        AssertDirectoryHasFiles(imagesDir);
        Assert.True(File.Exists(Path.Combine(imagesDir, "hero-image.png")));
    }

    [Fact]
    public void Login_page_resources_define_expected_inter_and_plus_jakarta_font_keys()
    {
        var document = LoadLoginPageResourceDocument();
        var fontFamilies = document.Root!
            .Elements(WpfNamespace + "FontFamily")
            .ToDictionary(
                element => element.Attribute(XamlNamespace + "Key")?.Value ?? string.Empty,
                element => element.Value.Trim(),
                StringComparer.Ordinal);

        Assert.Equal(
            "pack://application:,,,/DormitoryManagement.WPF;component/Assets/Fonts/PlusJakartaSans/#Plus Jakarta Sans",
            fontFamilies["LoginPageHeadlineFontFamily"]);
        Assert.Equal(
            "pack://application:,,,/DormitoryManagement.WPF;component/Assets/Fonts/Inter/#Inter",
            fontFamilies["LoginPageBodyFontFamily"]);
    }

    [Fact]
    public void Login_page_resources_define_exact_reference_color_tokens()
    {
        var document = LoadLoginPageResourceDocument();

        AssertResourceValue(document, "Color", "LoginPageColorBackground", "#F8F9FF");
        AssertResourceValue(document, "Color", "LoginPageColorSurface", "#FFFFFF");
        AssertResourceValue(document, "Color", "LoginPageColorSurfaceLow", "#EFF4FF");
        AssertResourceValue(document, "Color", "LoginPageColorSurfaceContainer", "#E5EEFF");
        AssertResourceValue(document, "Color", "LoginPageColorSurfaceHigh", "#DCE9FF");
        AssertResourceValue(document, "Color", "LoginPageColorSurfaceHighest", "#D3E4FE");
        AssertResourceValue(document, "Color", "LoginPageColorOutline", "#8D7168");
        AssertResourceValue(document, "Color", "LoginPageColorOutlineVariant", "#E1BFB5");
        AssertResourceValue(document, "Color", "LoginPageColorTextPrimary", "#0B1C30");
        AssertResourceValue(document, "Color", "LoginPageColorTextSecondary", "#594139");
        AssertResourceValue(document, "Color", "LoginPageColorPrimary", "#AB3500");
        AssertResourceValue(document, "Color", "LoginPageColorPrimaryContainer", "#FF6B35");
    }

    [Fact]
    public void Login_page_resources_define_exact_reference_spacing_and_radius_tokens()
    {
        var document = LoadLoginPageResourceDocument();

        AssertResourceValue(document, "Double", "LoginPageHeadlineLargeSize", "32");
        AssertResourceValue(document, "Double", "LoginPageHeadlineMediumSize", "24");
        AssertResourceValue(document, "Double", "LoginPageHeadlineSmallSize", "18");
        AssertResourceValue(document, "Double", "LoginPageBodyLargeSize", "16");
        AssertResourceValue(document, "Double", "LoginPageBodyMediumSize", "14");
        AssertResourceValue(document, "Double", "LoginPageLabelMediumSize", "14");
        AssertResourceValue(document, "Double", "LoginPageLabelSmallSize", "12");
        AssertResourceValue(document, "Double", "LoginPageHeaderHeight", "76");
        AssertResourceValue(document, "Double", "LoginPageMarginDesktop", "32");
        AssertResourceValue(document, "Double", "LoginPageMarginMobile", "16");
        AssertResourceValue(document, "Double", "LoginPageGutter", "24");
        AssertResourceValue(document, "Double", "LoginPageContainerMaxWidth", "1440");
        AssertResourceValue(document, "Double", "LoginPageStackSmall", "8");
        AssertResourceValue(document, "Double", "LoginPageStackMedium", "16");
        AssertResourceValue(document, "Double", "LoginPageStackLarge", "24");

        AssertResourceValue(document, "CornerRadius", "LoginPageControlCornerRadius", "8");
        AssertResourceValue(document, "CornerRadius", "LoginPageCardCornerRadius", "16");
        AssertResourceValue(document, "CornerRadius", "LoginPageOverlayCornerRadius", "24");
        AssertResourceValue(document, "CornerRadius", "LoginPagePillCornerRadius", "9999");

        AssertResourceValue(document, "Thickness", "LoginPageInputPadding", "40,12,12,12");
        AssertResourceValue(document, "Thickness", "LoginPagePasswordInputPadding", "40,12,40,12");
        AssertResourceValue(document, "Thickness", "LoginPageSectionSpacing", "0,0,0,24");
    }

    [Fact]
    public void Login_view_renders_required_reference_text_when_hosted_offscreen()
    {
        RunOnStaThread(() =>
        {
            EnsureApplicationResources();
            var view = CreateLoginView();
            var window = new Window
            {
                Width = 1440,
                Height = 900,
                Content = view,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Left = -10000,
                Top = -10000,
                ShowInTaskbar = false,
                Background = Brushes.White
            };

            try
            {
                window.Show();
                WaitForLayout();

                var texts = GetVisibleStringValues(view);
                string[] requiredTexts =
                [
                    "DormManagement",
                    "Chào mừng trở về",
                    "Truy cập bảng điều khiển ký túc xá của bạn để quản lý cài đặt phòng, xem các sự kiện cộng đồng và kết nối với bạn bè.",
                    "Đăng nhập",
                    "Nhập thông tin sinh viên của bạn để truy cập bảng điều khiển.",
                    "Mã sinh viên",
                    "VD: 20248901",
                    "Mật khẩu",
                    "Quên mật khẩu?",
                    "••••••••",
                    "Ghi nhớ đăng nhập",
                    "Đăng nhập vào Bảng điều khiển",
                    "Bạn mới biết đến DormManagement?",
                    "Đăng ký tại đây",
                    "Chính sách bảo mật",
                    "Điều khoản dịch vụ",
                    "Hỗ trợ",
                    "© 2026 DormManagement. Bảo lưu mọi quyền."
                ];

                foreach (var requiredText in requiredTexts)
                {
                    Assert.Contains(requiredText, texts);
                }
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void Login_view_renders_modules_in_expected_sequence()
    {
        RunOnStaThread(() =>
        {
            EnsureApplicationResources();
            var view = CreateLoginView();
            var window = new Window
            {
                Width = 1440,
                Height = 900,
                Content = view,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Left = -10000,
                Top = -10000,
                ShowInTaskbar = false,
                Background = Brushes.White
            };

            try
            {
                window.Show();
                WaitForLayout();

                var texts = GetVisibleStringSequence(view);
                AssertTextOrder(
                    texts,
                    "DormManagement",
                    "Chào mừng trở về",
                    "Đăng nhập",
                    "Mã sinh viên",
                    "Mật khẩu",
                    "Ghi nhớ đăng nhập",
                    "Đăng nhập vào Bảng điều khiển",
                    "Bạn mới biết đến DormManagement?",
                    "Chính sách bảo mật",
                    "© 2026 DormManagement. Bảo lưu mọi quyền.");
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void Login_view_reflows_without_horizontal_overflow_at_narrower_desktop_width()
    {
        RunOnStaThread(() =>
        {
            EnsureApplicationResources();
            var view = CreateLoginView();
            var window = new Window
            {
                Width = 1024,
                Height = 920,
                Content = view,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Left = -10000,
                Top = -10000,
                ShowInTaskbar = false,
                Background = Brushes.White
            };

            try
            {
                window.Show();
                WaitForLayout();

                var scrollViewer = FindDescendants<ScrollViewer>(view).OrderByDescending(candidate => candidate.ActualWidth * candidate.ActualHeight).First();
                Assert.True(scrollViewer.ScrollableWidth <= 0.5, $"Expected no horizontal overflow, but ScrollableWidth={scrollViewer.ScrollableWidth}.");

                var submitButton = FindVisibleTextBlock(view, "Đăng nhập vào Bảng điều khiển");
                var footerLink = FindVisibleTextBlock(view, "Chính sách bảo mật");
                var submitBounds = GetBoundsRelativeToAncestor(submitButton, view);
                var footerBounds = GetBoundsRelativeToAncestor(footerLink, view);

                Assert.True(submitBounds.Right <= view.ActualWidth + 0.5, $"Expected submit button within viewport. Right={submitBounds.Right}, ViewWidth={view.ActualWidth}.");
                Assert.True(footerBounds.Right <= view.ActualWidth + 0.5, $"Expected footer link within viewport. Right={footerBounds.Right}, ViewWidth={view.ActualWidth}.");
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void Login_view_writes_review_screenshot_artifact()
    {
        RunOnStaThread(() =>
        {
            EnsureApplicationResources();
            var view = CreateLoginView();
            var window = new Window
            {
                Width = 1440,
                Height = 900,
                Content = view,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Left = -10000,
                Top = -10000,
                ShowInTaskbar = false,
                Background = Brushes.White
            };

            try
            {
                window.Show();
                WaitForLayout();

                var repoRoot = FindRepositoryRoot();
                var artifactDirectory = Path.Combine(repoRoot, ".ai", "artifacts");
                Directory.CreateDirectory(artifactDirectory);
                var artifactPath = Path.Combine(artifactDirectory, "login-wpf.png");

                SaveFrameworkElementAsPng(view, artifactPath);

                Assert.True(File.Exists(artifactPath), $"Expected screenshot artifact '{artifactPath}' to exist.");
                Assert.True(new FileInfo(artifactPath).Length > 0, $"Expected screenshot artifact '{artifactPath}' to be non-empty.");
            }
            finally
            {
                window.Close();
            }
        });
    }

    private static XDocument LoadLoginPageResourceDocument()
    {
        var repoRoot = FindRepositoryRoot();
        var resourcePath = Path.Combine(repoRoot, "src", "DormitoryManagement.WPF", "Resources", "LoginPage.xaml");

        Assert.True(File.Exists(resourcePath), $"Expected login-page resource dictionary '{resourcePath}' to exist.");
        return XDocument.Load(resourcePath);
    }

    private static LoginView CreateLoginView()
    {
        return new LoginView
        {
            DataContext = new LoginViewModel(
                new LoginStubAuthService(),
                new RecordingNavigationService(),
                new SessionState(),
                new StubRememberedLoginService(),
                new LoginStubPrefillState()),
            Width = 1440,
            Height = 900
        };
    }
    private static XDocument LoadStudentDashboardViewDocument()
    {
        var repoRoot = FindRepositoryRoot();
        var viewPath = Path.Combine(repoRoot, "src", "DormitoryManagement.WPF", "Views", "Dashboard", "StudentDashboardView.xaml");

        Assert.True(File.Exists(viewPath), $"Expected student-dashboard view '{viewPath}' to exist.");
        return XDocument.Load(viewPath);
    }
    private static XDocument LoadStudentDashboardResourceDocument()
    {
        var repoRoot = FindRepositoryRoot();
        var resourcePath = Path.Combine(repoRoot, "src", "DormitoryManagement.WPF", "Resources", "StudentDashboard.xaml");

        Assert.True(File.Exists(resourcePath), $"Expected student-dashboard resource dictionary '{resourcePath}' to exist.");
        return XDocument.Load(resourcePath);
    }
    [Fact]
    public void Register_view_xaml_uses_shared_brandmark_wpfui_inputs_and_library_checkbox()
    {
        var xaml = LoadRegisterViewDocument().ToString(SaveOptions.DisableFormatting);

        Assert.Contains("xmlns:ui=\"http://schemas.lepo.co/wpfui/2022/xaml\"", xaml, StringComparison.Ordinal);
        Assert.Contains("<ui:ThemesDictionary Theme=\"Light\" />", xaml, StringComparison.Ordinal);
        Assert.Contains("<ui:ControlsDictionary />", xaml, StringComparison.Ordinal);
        Assert.Contains("<ui:CalendarDatePicker", xaml, StringComparison.Ordinal);
        Assert.Contains("Kind=\"OfficeBuildingOutline\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Style=\"{StaticResource MaterialDesignCheckBox}\"", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void Register_view_reflows_without_horizontal_overflow_at_narrower_desktop_width()
    {
        RunOnStaThread(() =>
        {
            EnsureApplicationResources();
            var view = CreateRegisterView();
            var window = new Window
            {
                Width = 1040,
                Height = 960,
                Content = view,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Left = -10000,
                Top = -10000,
                ShowInTaskbar = false,
                Background = Brushes.White
            };

            try
            {
                window.Show();
                WaitForLayout();

                var scrollViewers = FindDescendants<ScrollViewer>(view)
                    .Where(scrollViewer => scrollViewer.IsVisible)
                    .ToArray();

                Assert.NotEmpty(scrollViewers);
                Assert.All(scrollViewers, scrollViewer => Assert.True(
                    scrollViewer.ScrollableWidth <= 0.5,
                    $"Expected no horizontal overflow, but '{scrollViewer.Name}' reported ScrollableWidth={scrollViewer.ScrollableWidth}."));

                var texts = GetVisibleStringValues(view);
                Assert.Contains("Đăng ký tài khoản", texts);
                                Assert.Contains("Đăng nhập", texts);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void Register_view_keeps_login_handoff_text_baseline_aligned()
    {
        RunOnStaThread(() =>
        {
            EnsureApplicationResources();
            var view = CreateRegisterView();
            var window = new Window
            {
                Width = 1320,
                Height = 960,
                Content = view,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Left = -10000,
                Top = -10000,
                ShowInTaskbar = false,
                Background = Brushes.White
            };

            try
            {
                window.Show();
                WaitForLayout();

                var existingAccountText = FindVisibleTextBlock(view, "Đã có tài khoản?");
                var loginText = FindVisibleTextBlock(view, "Đăng nhập");
                var existingBounds = GetBoundsRelativeToAncestor(existingAccountText, view);
                var loginBounds = GetBoundsRelativeToAncestor(loginText, view);
                var topDelta = Math.Abs(existingBounds.Top - loginBounds.Top);
                var centerDelta = Math.Abs(existingBounds.Top + (existingBounds.Height / 2d) - (loginBounds.Top + (loginBounds.Height / 2d)));

                Assert.True(topDelta <= 3, $"Expected login handoff text to share the same top alignment. Delta={topDelta}.");
                Assert.True(centerDelta <= 3, $"Expected login handoff text to share the same visual center. Delta={centerDelta}.");
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void Register_view_keeps_terms_copy_aligned_with_checkbox()
    {
        RunOnStaThread(() =>
        {
            EnsureApplicationResources();
            var view = CreateRegisterView();
            var window = new Window
            {
                Width = 1320,
                Height = 960,
                Content = view,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Left = -10000,
                Top = -10000,
                ShowInTaskbar = false,
                Background = Brushes.White
            };

            try
            {
                window.Show();
                WaitForLayout();

                var checkbox = FindDescendants<CheckBox>(view).Single(candidate => candidate.IsVisible);
                var termsContainer = Assert.IsType<Grid>(checkbox.Parent);
                var termsCopy = termsContainer.Children.OfType<TextBlock>().Single();
                var checkboxBounds = GetBoundsRelativeToAncestor(checkbox, view);
                var textBounds = GetBoundsRelativeToAncestor(termsCopy, view);
                var centerDelta = Math.Abs(checkboxBounds.Top + (checkboxBounds.Height / 2d) - (textBounds.Top + (textBounds.Height / 2d)));

                Assert.True(centerDelta <= 6, $"Expected terms copy to stay visually aligned with the checkbox. Delta={centerDelta}.");
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void Register_view_shows_selected_date_of_birth_preview()
    {
        RunOnStaThread(() =>
        {
            EnsureApplicationResources();
            var viewModel = CreateRegisterViewModel();
            viewModel.DateOfBirth = new DateTime(2004, 1, 1);
            var view = CreateRegisterView(viewModel);
            var window = new Window
            {
                Width = 1320,
                Height = 960,
                Content = view,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Left = -10000,
                Top = -10000,
                ShowInTaskbar = false,
                Background = Brushes.White
            };

            try
            {
                window.Show();
                WaitForLayout();

                var previewText = FindVisibleTextBlock(view, "Đã chọn: 01/01/2004");

                Assert.True(previewText.IsVisible);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void Register_view_keeps_otp_actions_visible_in_same_shell_after_send_code()
    {
        RunOnStaThread(() =>
        {
            EnsureApplicationResources();
            var viewModel = CreateRegisterViewModel();
            PopulateValidRegisterForm(viewModel);
            viewModel.AcceptsTerms = true;
            viewModel.RegisterCommand.Execute(null);
            WaitUntil(() => viewModel.IsOtpStep);

            var view = CreateRegisterView(viewModel);
            var window = new Window
            {
                Width = 1320,
                Height = 960,
                Content = view,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Left = -10000,
                Top = -10000,
                ShowInTaskbar = false,
                Background = Brushes.White
            };

            try
            {
                window.Show();
                WaitForLayout();

                var texts = GetVisibleStringValues(view);
                Assert.Contains("Xác minh email", texts);
                Assert.Contains("Nhập mã xác minh gồm 6 chữ số được gửi tới email của bạn để hoàn tất đăng ký.", texts);
                Assert.Contains("Xác minh", texts);
                Assert.Contains("Gửi lại", texts);
                Assert.Contains("Đăng nhập", texts);
            }
            finally
            {
                window.Close();
            }
        });
    }
    private static HashSet<string> GetCompiledResourceKeys()
    {
        var assembly = typeof(MainWindow).Assembly;
        using var stream = assembly.GetManifestResourceStream("DormitoryManagement.WPF.g.resources");

        Assert.NotNull(stream);
        using var reader = new ResourceReader(stream);
        return reader.Cast<System.Collections.DictionaryEntry>()
            .Select(entry => Assert.IsType<string>(entry.Key))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static void AssertDirectoryHasFiles(string path)
    {
        Assert.True(Directory.Exists(path), $"Expected asset directory '{path}' to exist.");
        Assert.True(Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories).Any(), $"Expected asset directory '{path}' to contain bundled files.");
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

    private static XDocument LoadRegisterViewDocument()
    {
        var repoRoot = FindRepositoryRoot();
        var viewPath = Path.Combine(repoRoot, "src", "DormitoryManagement.WPF", "Views", "Auth", "RegisterView.xaml");

        Assert.True(File.Exists(viewPath), $"Expected register view '{viewPath}' to exist.");
        return XDocument.Load(viewPath);
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

    private static XDocument LoadForumHomeResourceDocument()
    {
        var repoRoot = FindRepositoryRoot();
        var resourcePath = Path.Combine(repoRoot, "src", "DormitoryManagement.WPF", "Resources", "ForumHome.xaml");

        Assert.True(File.Exists(resourcePath), $"Expected forum-home resource dictionary '{resourcePath}' to exist.");
        return XDocument.Load(resourcePath);
    }

    private static XDocument LoadForumHomeViewDocument()
    {
        var repoRoot = FindRepositoryRoot();
        var viewPath = Path.Combine(repoRoot, "src", "DormitoryManagement.WPF", "Views", "Forum", "ForumHomeView.xaml");

        Assert.True(File.Exists(viewPath), $"Expected forum-home view '{viewPath}' to exist.");
        return XDocument.Load(viewPath);
    }

    private static void AssertFontSetter(XDocument document, string styleKey, string expectedFontValue)
    {
        AssertStyleSetter(document, styleKey, "FontFamily", expectedFontValue);
    }

    private static void AssertStyleSetter(XDocument document, string styleKey, string propertyName, string expectedValue)
    {
        var style = document.Root!
            .Elements(WpfNamespace + "Style")
            .FirstOrDefault(element => string.Equals(element.Attribute(XamlNamespace + "Key")?.Value, styleKey, StringComparison.Ordinal));

        Assert.NotNull(style);
        var resolvedStyle = style!;

        var fontSetter = resolvedStyle.Elements(WpfNamespace + "Setter")
            .FirstOrDefault(element => string.Equals(element.Attribute("Property")?.Value, propertyName, StringComparison.Ordinal));

        Assert.NotNull(fontSetter);
        Assert.Equal(expectedValue, fontSetter.Attribute("Value")?.Value);
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

    private static HashSet<string> GetVisibleStringValues(DependencyObject root)
    {
        return GetVisibleStringSequence(root)
            .Where(text => !string.IsNullOrWhiteSpace(text))
            .ToHashSet(StringComparer.Ordinal);
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

    private static TextBlock FindVisibleTextBlock(DependencyObject root, string text)
    {
        return FindDescendants<TextBlock>(root)
            .FirstOrDefault(textBlock => textBlock.IsVisible && string.Equals(textBlock.Text, text, StringComparison.Ordinal))
            ?? throw new Xunit.Sdk.XunitException($"Expected visible TextBlock with text '{text}'.");
    }

    private static Rect GetBoundsRelativeToAncestor(FrameworkElement element, Visual ancestor)
    {
        var transform = element.TransformToAncestor(ancestor);
        return transform.TransformBounds(new Rect(0, 0, element.ActualWidth, element.ActualHeight));
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

    private static System.Windows.Application EnsureApplicationResources()
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

    private static StudentDashboardViewModel CreateStudentDashboardViewModel() =>
        new(
            new StubScopeFactory(new StubDashboardService(new StudentDashboardDto
            {
                CurrentRoom = "A-101",
                OutstandingDebt = 810000m,
                OpenTickets = 1,
                UnreadNotifications = 3
            })),
            new StubCurrentUser(),
            new RecordingNavigationService());

    private static StudentDashboardViewModel CreateStudentDashboardPopupViewModel() =>
        new(
            new StubScopeFactory(
                new StubDashboardService(new StudentDashboardDto
                {
                    RoomCardDisplayMode = "Empty",
                    RoomCardStatusText = "Chưa phân phòng",
                    CanOpenRoomRegistrationPopup = true
                }),
                new StubRegistrationService(),
                new StubRoomService([
                    new RoomDto
                    {
                        Id = Guid.NewGuid(),
                        BuildingName = "Khu A",
                        FloorNumber = 1,
                        RoomNumber = "101",
                        Capacity = 4,
                        AvailableSlots = 2,
                        MonthlyPrice = 750000m,
                        GenderType = RoomGenderType.Male
                    }
                ])),
            new StubCurrentUser(),
            new RecordingNavigationService());

    private static ShellViewModel CreateStudentDashboardShellViewModel(out StudentDashboardViewModel dashboardViewModel)
    {
        var navigationStore = new NavigationStore();
        dashboardViewModel = CreateStudentDashboardViewModel();
        var shellViewModel = new ShellViewModel(
            navigationStore,
            new RecordingNavigationService(),
            new StubCurrentUser(),
            new StubSessionService(),
            new StubRememberedLoginService(),
            new ThrowingScopeFactory(),
            new SessionState());
        navigationStore.CurrentViewModel = dashboardViewModel;
        return shellViewModel;
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

    private static RegisterView CreateRegisterView(RegisterViewModel? viewModel = null)
    {
        return new RegisterView
        {
            DataContext = viewModel ?? CreateRegisterViewModel(),
            Width = 1320,
            Height = 960
        };
    }

    private static RegisterViewModel CreateRegisterViewModel(IAccountRegistrationService? service = null)
    {
        return new RegisterViewModel(
            service ?? new RegisterStubRegistrationService(),
            new RecordingNavigationService(),
            new LoginStubPrefillState());
    }

    private static void PopulateValidRegisterForm(RegisterViewModel viewModel)
    {
        viewModel.FullName = "Nguyễn Văn A";
        viewModel.StudentCode = "20230001";
        viewModel.Username = "nguyenvana";
        viewModel.DateOfBirth = new DateTime(2004, 1, 1);
        viewModel.SelectedGender = "Nam";
        viewModel.PhoneNumber = "0901234567";
        viewModel.Email = "student@example.edu.vn";
        viewModel.Password = "123456";
        viewModel.ConfirmPassword = "123456";
    }
    private sealed class VehicleRegistrationStubVehicleService : IVehicleService
    {
        public Task<IReadOnlyList<VehicleRegistrationDto>> GetCurrentStudentVehicleRegistrationsAsync(DateTime? asOfDate = null, CancellationToken ct = default)
        {
            IReadOnlyList<VehicleRegistrationDto> registrations =
            [
                new VehicleRegistrationDto
                {
                    Id = Guid.NewGuid(),
                    LicensePlate = "59A1-23456",
                    NormalizedPlate = "59A1-23456",
                    MonthCount = 3,
                    Amount = 120000m,
                    Status = VehicleStatus.Approved,
                    StatusText = "Đã duyệt",
                    RegisteredAt = new DateTime(2023, 10, 15)
                },
                new VehicleRegistrationDto
                {
                    Id = Guid.NewGuid(),
                    LicensePlate = "59A1-23456",
                    NormalizedPlate = "59A1-23456",
                    MonthCount = 1,
                    Amount = 40000m,
                    Status = VehicleStatus.Expired,
                    StatusText = "Hết hạn",
                    RegisteredAt = new DateTime(2023, 9, 10)
                },
                new VehicleRegistrationDto
                {
                    Id = Guid.NewGuid(),
                    LicensePlate = "59A1-23456",
                    NormalizedPlate = "59A1-23456",
                    MonthCount = 1,
                    Amount = 40000m,
                    Status = VehicleStatus.Expired,
                    StatusText = "Hết hạn",
                    RegisteredAt = new DateTime(2023, 8, 10)
                }
            ];

            return Task.FromResult(registrations);
        }

        public Task<VehicleRegistrationDto> RegisterVehicleAsync(CreateVehicleRegistrationRequest request, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task ApproveVehicleAsync(Guid registrationId, CancellationToken ct = default) => Task.CompletedTask;
        public Task RejectVehicleAsync(Guid registrationId, string reason, CancellationToken ct = default) => Task.CompletedTask;
        public Task CancelVehicleAsync(Guid registrationId, CancellationToken ct = default) => Task.CompletedTask;
    }

    private sealed class LoginStubAuthService : IAuthService
    {
        public Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken ct = default) =>
            Task.FromResult(LoginResult.Failed("noop", LoginFailureReason.InvalidPassword));

        public Task LogoutAsync(CancellationToken ct = default) => Task.CompletedTask;
        public Task ChangePasswordAsync(ChangePasswordRequest request, CancellationToken ct = default) => Task.CompletedTask;
    }

    private sealed class LoginStubPrefillState : ILoginPrefillState
    {
        public string? ConsumeEmail() => null;
        public void SetEmail(string email) { }
    }
    private sealed class StubDashboardService : IDashboardService
    {
        private readonly StudentDashboardDto _dashboard;

        public StubDashboardService(StudentDashboardDto dashboard)
        {
            _dashboard = dashboard;
        }

        public Task<AdminDashboardDto> GetAdminDashboardAsync(CancellationToken ct = default) => throw new NotSupportedException();
        public Task<StudentDashboardDto> GetStudentDashboardAsync(Guid studentId, CancellationToken ct = default) => Task.FromResult(_dashboard);
        public Task<IReadOnlyList<ChartPointDto>> GetRevenueByMonthAsync(int year, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<IReadOnlyList<RoomOccupancyDto>> GetRoomOccupancyAsync(CancellationToken ct = default) => throw new NotSupportedException();
        public Task<DebtSummaryDto> GetDebtSummaryAsync(CancellationToken ct = default) => throw new NotSupportedException();
        public Task<IReadOnlyList<DashboardRegistrationDto>> GetPendingRegistrationsAsync(CancellationToken ct = default) => throw new NotSupportedException();
        public Task<IReadOnlyList<DashboardInvoiceDto>> GetOverdueInvoicesAsync(CancellationToken ct = default) => throw new NotSupportedException();
        public Task<IReadOnlyList<DashboardTicketDto>> GetOpenSupportTicketsAsync(CancellationToken ct = default) => throw new NotSupportedException();
        public Task<IReadOnlyList<DashboardActivityDto>> GetRecentActivitiesAsync(CancellationToken ct = default) => throw new NotSupportedException();
    }

    private sealed class StubScopeFactory : IServiceScopeFactory
    {
        private readonly IDashboardService _dashboardService;
        private readonly IRoomRegistrationService? _registrationService;
        private readonly IRoomService? _roomService;

        public StubScopeFactory(IDashboardService dashboardService, IRoomRegistrationService? registrationService = null, IRoomService? roomService = null)
        {
            _dashboardService = dashboardService;
            _registrationService = registrationService;
            _roomService = roomService;
        }

        public IServiceScope CreateScope() => new StubScope(_dashboardService, _registrationService, _roomService);
    }

    private sealed class StubScope : IServiceScope
    {
        public StubScope(IDashboardService dashboardService, IRoomRegistrationService? registrationService, IRoomService? roomService)
        {
            ServiceProvider = new StubServiceProvider(dashboardService, registrationService, roomService);
        }

        public IServiceProvider ServiceProvider { get; }
        public void Dispose() { }
    }

    private sealed class StubServiceProvider : IServiceProvider
    {
        private readonly IDashboardService _dashboardService;
        private readonly IRoomRegistrationService? _registrationService;
        private readonly IRoomService? _roomService;

        public StubServiceProvider(IDashboardService dashboardService, IRoomRegistrationService? registrationService, IRoomService? roomService)
        {
            _dashboardService = dashboardService;
            _registrationService = registrationService;
            _roomService = roomService;
        }

        public object? GetService(Type serviceType) => serviceType == typeof(IDashboardService)
            ? _dashboardService
            : serviceType == typeof(RoomRegistrationViewModel) && _registrationService is not null && _roomService is not null
                ? new RoomRegistrationViewModel(_registrationService, _roomService)
                : null;
    }

    private sealed class StubRegistrationService : IRoomRegistrationService
    {
        public Task<Guid> CreateRegistrationAsync(CreateRoomRegistrationRequest request, CancellationToken ct = default) => Task.FromResult(Guid.NewGuid());
        public Task<IReadOnlyList<RoomRegistrationDto>> GetPendingRegistrationsAsync(CancellationToken ct = default) => Task.FromResult<IReadOnlyList<RoomRegistrationDto>>(Array.Empty<RoomRegistrationDto>());
        public Task<IReadOnlyList<RoomRegistrationDto>> GetCurrentStudentRegistrationsAsync(CancellationToken ct = default) => Task.FromResult<IReadOnlyList<RoomRegistrationDto>>(Array.Empty<RoomRegistrationDto>());
        public Task ApproveRegistrationAsync(ApproveRoomRegistrationRequest request, CancellationToken ct = default) => Task.CompletedTask;
        public Task RejectRegistrationAsync(RejectRoomRegistrationRequest request, CancellationToken ct = default) => Task.CompletedTask;
        public Task CancelRegistrationAsync(Guid registrationId, CancellationToken ct = default) => Task.CompletedTask;
    }

    private sealed class StubRoomService : IRoomService
    {
        private readonly IReadOnlyList<RoomDto> _rooms;

        public StubRoomService(IReadOnlyList<RoomDto> rooms)
        {
            _rooms = rooms;
        }

        public Task<PagedResult<RoomDto>> GetRoomsAsync(RoomFilterRequest? request = null, CancellationToken ct = default) => Task.FromResult(PagedResult<RoomDto>.Empty());
        public Task<IReadOnlyList<RoomDto>> GetAvailableRoomsAsync(RoomFilterRequest? request = null, CancellationToken ct = default) => Task.FromResult(_rooms);
        public Task<RoomDto> CreateRoomAsync(CreateRoomRequest request, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<RoomDto> UpdateRoomAsync(Guid roomId, CreateRoomRequest request, CancellationToken ct = default) => throw new NotSupportedException();
        public Task ChangeRoomStatusAsync(Guid roomId, RoomStatus status, CancellationToken ct = default) => Task.CompletedTask;
    }

    private sealed class RegisterStubRegistrationService : IAccountRegistrationService
    {
        public Task<StartAccountRegistrationResult> StartStudentAccountRegistrationAsync(RegisterAccountRequest request, CancellationToken ct = default)
        {
            return Task.FromResult(StartAccountRegistrationResult.Success(Guid.NewGuid(), "s****@example.edu.vn", DateTime.UtcNow.AddMinutes(5), DateTime.UtcNow.AddMinutes(1)));
        }

        public Task<RegisterAccountResult> VerifyStudentAccountOtpAsync(Guid pendingRegistrationId, string otpCode, CancellationToken ct = default)
        {
            return Task.FromResult(RegisterAccountResult.Success(Guid.NewGuid(), Guid.NewGuid()));
        }

        public Task<StartAccountRegistrationResult> ResendStudentAccountOtpAsync(Guid pendingRegistrationId, CancellationToken ct = default)
        {
            return Task.FromResult(StartAccountRegistrationResult.Success(pendingRegistrationId, "s****@example.edu.vn", DateTime.UtcNow.AddMinutes(5), DateTime.UtcNow.AddMinutes(1)));
        }
    }
    private sealed class RecordingNavigationService : INavigationService
    {
        public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase { }
    }

    private sealed class StubSessionService : ISessionService
    {
        public CurrentUserDto? CurrentUser { get; private set; }
        public void SetCurrentUser(CurrentUserDto user) => CurrentUser = user;
        public void Clear() => CurrentUser = null;
    }

    private sealed class StubRememberedLoginService : IRememberedLoginService
    {
        public RememberedLoginState Load() => RememberedLoginState.Empty;
        public void SaveFullLogin(string emailOrStudentCode, string password) { }
        public void SaveEmailOnly(string emailOrStudentCode) { }
        public void DowngradeToEmailOnly(string emailOrStudentCode) { }
        public void Clear() { }
    }

    private sealed class ThrowingScopeFactory : IServiceScopeFactory
    {
        public IServiceScope CreateScope() => throw new NotSupportedException();
    }

    private sealed class StubCurrentUser : ICurrentUserService
    {
        public StubCurrentUser()
        {
            CurrentUser = new CurrentUserDto
            {
                UserId = Guid.NewGuid(),
                Username = "student",
                Email = "student@ktx.local",
                FullName = "Nguyễn Văn A",
                RoleName = "Student",
                StudentId = Guid.NewGuid()
            };
        }

        public CurrentUserDto? CurrentUser { get; }
        public Guid? UserId => CurrentUser?.UserId;
        public string? UserName => CurrentUser?.Username;
        public string? Email => CurrentUser?.Email;
        public string? FullName => CurrentUser?.FullName;
        public IReadOnlyCollection<string> Roles => ["Student"];
        public bool IsAuthenticated => true;
        public bool IsInRole(string roleName) => string.Equals(roleName, "Student", StringComparison.OrdinalIgnoreCase);
    }
    private static void AssertTextOrder(IList<string> texts, params string[] orderedValues)
    {
        var lastIndex = -1;
        for (var i = 0; i < orderedValues.Length; i++)
        {
            var value = orderedValues[i];
            var index = texts.IndexOf(value);
            Assert.True(index >= 0, $"Expected to render '{value}' in offscreen forum-home review tree.");
            if (i > 0)
            {
                Assert.True(index > lastIndex, $"Expected '{value}' to appear after '{orderedValues[i - 1]}'.");
            }

            lastIndex = index;
        }
    }

    private static void AssertStringOrder(string text, params string[] orderedValues)
    {
        var lastIndex = -1;
        foreach (var value in orderedValues)
        {
            var index = text.IndexOf(value, StringComparison.Ordinal);
            Assert.True(index >= 0, $"Expected to find '{value}' in XAML content.");
            Assert.True(index > lastIndex, $"Expected '{value}' to appear after the previous popup contract string.");
            lastIndex = index;
        }
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

    private static readonly XNamespace WpfNamespace = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
    private static readonly XNamespace XamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";
}














































