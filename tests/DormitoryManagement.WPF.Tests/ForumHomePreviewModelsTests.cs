using DormitoryManagement.WPF.ViewModels.Forum;

namespace DormitoryManagement.WPF.Tests;

public sealed class ForumHomePreviewModelsTests
{
    [Fact]
    public void Preview_models_expose_panel_and_fidelity_contract()
    {
        var post = new ForumHomePostCard(
            id: "post-1",
            coverAssetPath: null,
            useFallbackVisual: true,
            overlayTitle: null,
            overlaySubtitle: null,
            priorityLabel: null,
            badges:
            [
                new ForumHomeBadge("Thông báo", "primary")
            ],
            title: "Thông báo lịch bảo trì điện nước khu B",
            excerpt: "Ban quản lý KTX xin thông báo...",
            authorName: "Ban Quản Lý",
            authorAvatarLabel: "BQL",
            authorSubtitle: "Văn phòng KTX",
            relativeTimeText: "2 giờ trước",
            viewCountText: "1.2k",
            likeCountText: "45",
            commentCountText: "12",
            areaLabelText: "Khu vực",
            areaOverlayEmphasis: null,
            authorRoleText: "Văn phòng KTX",
            isImportant: true,
            isPreviewExpanded: false,
            fallbackBadgeText: "Không có ảnh",
            fallbackAccentTone: "primary");

        var activity = new ForumHomeActivityItem(
            id: "activity-1",
            monthText: "Th5",
            dayText: "12",
            title: "Giải bóng đá sinh viên nam KTX",
            locationText: "Sân cỏ nhân tạo A",
            accentTone: "primary",
            participantText: "42 người tham gia",
            isPreviewJoined: false,
            actionLabel: "Xem chi tiết");

        var contact = new ForumHomeEmergencyContactItem(
            id: "contact-1",
            name: "Phòng Bảo vệ KTX",
            description: "Hỗ trợ an ninh",
            phoneText: "024 38xxx 111",
            accentTone: "primary",
            iconKind: "Security",
            availabilityText: "24/7",
            previewDialState: "ready",
            actionLabel: "Gọi thử");

        var panel = new ForumHomePreviewPanel(
            panelType: "notifications",
            title: "Trung tâm thông báo",
            items:
            [
                new ForumHomePreviewPanelItem(
                    key: "notif-1",
                    title: "BQL Khu B phản hồi góp ý của bạn",
                    description: "Ý kiến về thời gian thu dọn rác đã được tiếp nhận...",
                    metaText: "20 phút trước",
                    accentTone: "primary",
                    isUnread: true)
            ],
            isOpen: true,
            emptyStateText: "Không có thông báo mới nào!");

        var exception = new ForumHomeFidelityException(
            area: "Header blur",
            referenceBehavior: "backdrop-blur-sm",
            deliveredBehavior: "shadow-only",
            rationale: "WPF has no equivalent backdrop blur",
            reviewerStatus: "Accepted");

        var state = new ForumHomePreviewState(
            headerUserSummary: new ForumHomeUserSummary("Nguyễn Văn A", "Sinh viên nội trú", "NVA", "Phòng 402 - Khu B", showChevron: true),
            topNavItems:
            [
                new ForumHomeNavItem("home", "Trang chủ", "Home", isSelected: true, countText: null)
            ],
            selectedTopNavKey: "home",
            categoryItems:
            [
                new ForumHomeNavItem("news", "Tin tức chung", "Bullhorn", isSelected: true, countText: "12")
            ],
            selectedCategoryKey: "news",
            popularTags:
            [
                new ForumHomeTagChip("tag-1", "#thongbao", "primary", isSelected: false, countText: "8")
            ],
            selectedTagKey: string.Empty,
            areaShortcuts:
            [
                new ForumHomeAreaShortcut("area-b", "B", "Khu B", "HomeCity", "Tòa ở đông", isSelected: true, countText: "24")
            ],
            selectedAreaKey: "area-b",
            searchText: string.Empty,
            searchPlaceholder: "Tìm kiếm tin tức, thông báo...",
            brandText: "DMForum",
            composePrompt: "Bạn có thông tin gì muốn chia sẻ với mọi người?",
            postCards: [post],
            activityItems: [activity],
            emergencyContacts: [contact],
            previewPanels: [panel],
            openPreviewPanelKey: "notifications",
            fidelityExceptions: [exception],
            isUsingPreviewData: true,
            statusMessage: null);

        Assert.Equal("DMForum", state.BrandText);
        Assert.Equal("Tìm kiếm tin tức, thông báo...", state.SearchPlaceholder);
        Assert.True(state.HasOpenPreviewPanel);
        Assert.Single(state.PreviewPanels);
        Assert.Single(state.FidelityExceptions);
        Assert.True(post.IsImportant);
        Assert.False(post.ShowAreaOverlay);
        Assert.Equal("42 người tham gia", activity.ParticipantText);
        Assert.Equal("ready", contact.PreviewDialState);
    }
}
