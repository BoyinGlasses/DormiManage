using System.Collections;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using DormitoryManagement.WPF.Common;
using DormitoryManagement.WPF.Navigation;
using DormitoryManagement.WPF.ViewModels.Forum;

namespace DormitoryManagement.WPF.Tests;

public sealed class ForumHomeViewModelTests
{
    private const string ViewModelTypeName = "DormitoryManagement.WPF.ViewModels.Forum.ForumHomeViewModel, DormitoryManagement.WPF";

    [Fact]
    public void Constructor_exposes_preview_state_contract()
    {
        var viewModel = CreateViewModel();

        AssertCollectionProperty(viewModel, "TopNavItems", minimumCount: 1);
        AssertCollectionProperty(viewModel, "CategoryItems", minimumCount: 1);
        AssertCollectionProperty(viewModel, "TagChips", minimumCount: 1);
        AssertCollectionProperty(viewModel, "AreaShortcuts", minimumCount: 1);
        AssertCollectionProperty(viewModel, "FeedCards", minimumCount: 2);
        AssertCollectionProperty(viewModel, "ActivityItems", minimumCount: 1);
        AssertCollectionProperty(viewModel, "EmergencyContacts", minimumCount: 1);
        Assert.True(GetRequiredProperty<bool>(viewModel, "IsUsingPreviewData"));
        Assert.False(GetRequiredProperty<bool>(viewModel, "HasSearchInput"));
        Assert.Null(GetReferenceProperty(viewModel, "StatusMessage"));
    }

    [Fact]
    public void Default_phase1_preview_state_hides_phase2_only_surfaces()
    {
        var viewModel = CreateViewModel();

        Assert.False(GetRequiredProperty<bool>(viewModel, "HasActivePreviewFilters"));
        Assert.False(GetRequiredProperty<bool>(viewModel, "HasEmptyResults"));
        Assert.False(GetRequiredProperty<bool>(viewModel, "HasOpenPreviewPanel"));
        Assert.False(GetRequiredProperty<bool>(viewModel, "HasEmergencyContactPreview"));
        Assert.False(GetRequiredProperty<bool>(viewModel, "IsComposeDialogOpen"));
        Assert.False(GetRequiredProperty<bool>(viewModel, "ShowComposeDialog"));
        Assert.Null(GetReferenceProperty(viewModel, "OpenPreviewPanelKey"));
        Assert.Null(GetReferenceProperty(viewModel, "SelectedEmergencyContact"));
        Assert.All(GetCollectionItems(viewModel, "PreviewPanels"), panel => Assert.False(GetRequiredProperty<bool>(panel, "IsOpen")));
    }

    [Fact]
    public void Constructor_exposes_exact_ref_header_navigation_and_search_text()
    {
        var viewModel = CreateViewModel();

        Assert.Equal("DMForum", GetRequiredProperty<string>(viewModel, "BrandText"));
        Assert.Equal("Tìm kiếm tin tức, thông báo...", GetRequiredProperty<string>(viewModel, "SearchPlaceholder"));

        var userSummary = GetReferenceProperty(viewModel, "HeaderUserSummary");

        Assert.NotNull(userSummary);
        Assert.Equal("Nguyễn Văn A", GetRequiredProperty<string>(userSummary, "DisplayName"));
        Assert.Equal("Phòng 402 - Khu B", GetRequiredProperty<string>(userSummary, "RoomLabel"));

        Assert.Equal(
        [
            "home",
            "calendar",
            "groups",
            "campaign",
            "support"
        ],
        GetCollectionItems(viewModel, "TopNavItems").Select(item => GetRequiredProperty<string>(item, "Key")).ToArray());
    }

    [Fact]
    public void Constructor_exposes_exact_ref_category_tag_and_area_text()
    {
        var viewModel = CreateViewModel();

        Assert.Equal(
        [
            "Tin tức chung",
            "CLB & Sự kiện",
            "Mua bán & Trao đổi",
            "Góp ý & Khiếu nại"
        ],
        GetCollectionItems(viewModel, "CategoryItems").Select(item => GetRequiredProperty<string>(item, "Label")).ToArray());

        Assert.Equal(
        [
            "#thongbao",
            "#clbsukien",
            "#passdo",
            "#cangtin",
            "#thethao",
            "#matdien"
        ],
        GetCollectionItems(viewModel, "TagChips").Select(item => GetRequiredProperty<string>(item, "Text")).ToArray());

        Assert.Equal(
        [
            "Khu A",
            "Khu B",
            "Nhà ăn & Dịch vụ"
        ],
        GetCollectionItems(viewModel, "AreaShortcuts").Select(item => GetRequiredProperty<string>(item, "Title")).ToArray());
    }

    [Fact]
    public void Constructor_exposes_exact_ref_compose_prompt_and_primary_feed_cards()
    {
        var viewModel = CreateViewModel();

        Assert.Equal("Bạn có thông tin gì muốn chia sẻ với mọi người?", GetRequiredProperty<string>(viewModel, "ComposePrompt"));

        var feedCards = GetCollectionItems(viewModel, "FeedCards");

        Assert.True(feedCards.Count >= 3);

        AssertFeedCard(
            feedCards[0],
            expectedId: "study-refresh",
            expectedTitle: "Thông báo lịch bảo trì điện nước khu B",
            expectedExcerpt: "Ban quản lý KTX xin thông báo: Từ 8h00 đến 11h30 sáng thứ 7 tuần này, khu B sẽ tạm ngưng cấp điện, nước để tiến hành bảo trì hệ thống định kỳ. Mong các bạn sinh viên chủ động sắp xếp...",
            expectedAuthor: "Ban Quản Lý",
            expectedTime: "2 giờ trước",
            expectedViews: "1.2k",
            expectedLikes: "45",
            expectedComments: "12");

        AssertFeedCard(
            feedCards[1],
            expectedId: "club-rehearsal",
            expectedTitle: "Tuyển thành viên CLB Tình nguyện KTX",
            expectedExcerpt: "CLB Tình nguyện đang tìm kiếm các bạn sinh viên nhiệt huyết tham gia chiến dịch 'Mùa hè xanh' tại khuôn viên kí túc xá. Nếu bạn yêu thích các hoạt động cộng đồng và muốn đóng góp sức trẻ, hãy tham gia cùng chúng mình.",
            expectedAuthor: "Đội Tình Nguyện",
            expectedTime: "Hôm qua",
            expectedViews: "120",
            expectedLikes: "24",
            expectedComments: "5");

        AssertFeedCard(
            feedCards[2],
            expectedId: "quiet-hours",
            expectedTitle: "Review quán cơm mới mở cạnh căng tin khu C",
            expectedExcerpt: "Hôm nay mình mới thử quán cơm bình dân mới khai trương ở căng tin khu C. Suất 30k mà được khá nhiều thịt, có cả canh chua và trà đá miễn phí. Cô chủ quán rất nhiệt tình...",
            expectedAuthor: "Lan Anh",
            expectedTime: "3 ngày trước",
            expectedViews: "850",
            expectedLikes: "120",
            expectedComments: "34");
    }

    [Fact]
    public void Constructor_exposes_exact_ref_activity_and_emergency_text()
    {
        var viewModel = CreateViewModel();

        Assert.Equal(
        [
            "Giải bóng đá sinh viên nam KTX",
            "Đêm nhạc Acoustic gây quỹ"
        ],
        GetCollectionItems(viewModel, "ActivityItems").Select(item => GetRequiredProperty<string>(item, "Title")).ToArray());

        Assert.Equal(
        [
            "Sân cỏ nhân tạo A",
            "Sân sinh hoạt chung"
        ],
        GetCollectionItems(viewModel, "ActivityItems").Select(item => GetRequiredProperty<string>(item, "Subtitle")).ToArray());

        Assert.Equal(
        [
            "Phòng Bảo vệ KTX",
            "Trạm y tế Tòa nhà C",
            "Văn phòng Ban Quản lý"
        ],
        GetCollectionItems(viewModel, "EmergencyContacts").Select(item => GetRequiredProperty<string>(item, "Title")).ToArray());

        Assert.Equal(
        [
            "024 38xxx 111",
            "024 38xxx 115",
            "024 38xxx 222"
        ],
        GetCollectionItems(viewModel, "EmergencyContacts").Select(item => GetRequiredProperty<string>(item, "ContactText")).ToArray());
    }

    [Fact]
    public void Search_text_updates_local_preview_state_only()
    {
        var viewModel = CreateViewModel();

        SetRequiredProperty(viewModel, "SearchText", "tình nguyện");

        Assert.Equal("tình nguyện", GetRequiredProperty<string>(viewModel, "SearchText"));
        Assert.True(GetRequiredProperty<bool>(viewModel, "HasSearchInput"));
        Assert.True(GetRequiredProperty<bool>(viewModel, "IsUsingPreviewData"));
        Assert.Equal(
        [
            "club-rehearsal"
        ],
        GetCollectionItems(viewModel, "FeedCards").Select(card => GetRequiredProperty<string>(card, "Id")).ToArray());
        Assert.Equal("Tìm kiếm: tình nguyện.", GetRequiredProperty<string>(viewModel, "StatusMessage"));
    }

    [Fact]
    public void Search_with_no_matches_exposes_empty_state_and_reset_path()
    {
        var viewModel = CreateViewModel();

        SetRequiredProperty(viewModel, "SearchText", "khong-co-ket-qua");

        Assert.True(GetRequiredProperty<bool>(viewModel, "HasSearchInput"));
        Assert.True(GetRequiredProperty<bool>(viewModel, "HasActivePreviewFilters"));
        Assert.True(GetRequiredProperty<bool>(viewModel, "HasEmptyResults"));
        Assert.Empty(GetCollectionItems(viewModel, "FeedCards"));
        Assert.Equal("Không tìm thấy bài viết phù hợp.", GetRequiredProperty<string>(viewModel, "StatusMessage"));

        var resetCommand = GetRequiredProperty<ICommand>(viewModel, "ResetFiltersCommand");
        Assert.True(resetCommand.CanExecute(null));
    }

    [Fact]
    public void Placeholder_commands_toggle_local_feedback_without_live_mutation()
    {
        var viewModel = CreateViewModel();
        var firstFeedCard = GetFirstCollectionItem(viewModel, "FeedCards");

        var initialLikeState = GetRequiredProperty<bool>(firstFeedCard, "IsLiked");

        ExecuteRequiredCommand(viewModel, "ComposeCommand");
        var composeStatus = GetRequiredProperty<string>(viewModel, "StatusMessage");
        Assert.False(string.IsNullOrWhiteSpace(composeStatus));
        Assert.True(GetRequiredProperty<bool>(viewModel, "IsComposeDialogOpen"));

        ExecuteRequiredCommand(viewModel, "OpenNotificationCenterCommand");
        var notificationStatus = GetRequiredProperty<string>(viewModel, "StatusMessage");
        Assert.False(string.IsNullOrWhiteSpace(notificationStatus));

        ExecuteRequiredCommand(viewModel, "OpenMessagesCommand");
        var messageStatus = GetRequiredProperty<string>(viewModel, "StatusMessage");
        Assert.False(string.IsNullOrWhiteSpace(messageStatus));

        ExecuteRequiredCommand(viewModel, "ToggleLikeCommand", firstFeedCard);
        Assert.Equal(!initialLikeState, GetRequiredProperty<bool>(firstFeedCard, "IsLiked"));
        Assert.True(GetRequiredProperty<bool>(viewModel, "IsUsingPreviewData"));
    }

    [Fact]
    public void Selection_commands_refresh_visual_modules_deterministically()
    {
        var categoryViewModel = CreateViewModel();
        var eventsCategory = FindCollectionItemByProperty(categoryViewModel, "CategoryItems", "Key", "events");

        ExecuteRequiredCommand(categoryViewModel, "SelectCategoryCommand", eventsCategory);

        Assert.Equal(
        [
            "club-rehearsal"
        ],
        GetCollectionItems(categoryViewModel, "FeedCards").Select(card => GetRequiredProperty<string>(card, "Id")).ToArray());
        AssertSelectedState(categoryViewModel, "CategoryItems", "Key", "events");
        Assert.Equal("Category: CLB & Sự kiện.", GetRequiredProperty<string>(categoryViewModel, "StatusMessage"));

        var tagViewModel = CreateViewModel();
        var quietStudyTag = FindCollectionItemByProperty(tagViewModel, "TagChips", "Key", "quiet-study");

        ExecuteRequiredCommand(tagViewModel, "SelectTagCommand", quietStudyTag);

        Assert.Equal(
        [
            "study-refresh",
            "quiet-hours"
        ],
        GetCollectionItems(tagViewModel, "FeedCards").Select(card => GetRequiredProperty<string>(card, "Id")).ToArray());
        AssertSelectedState(tagViewModel, "TagChips", "Key", "quiet-study");
        Assert.Equal(3, GetCollectionItems(tagViewModel, "EmergencyContacts").Count);
        Assert.Equal("Tag: #clbsukien.", GetRequiredProperty<string>(tagViewModel, "StatusMessage"));

        var areaViewModel = CreateViewModel();
        var northWingArea = FindCollectionItemByProperty(areaViewModel, "AreaShortcuts", "Key", "north-wing");

        ExecuteRequiredCommand(areaViewModel, "SelectAreaCommand", northWingArea);

        Assert.Equal(
        [
            "study-refresh",
            "maintenance-slot"
        ],
        GetCollectionItems(areaViewModel, "FeedCards").Select(card => GetRequiredProperty<string>(card, "Id")).ToArray());
        AssertSelectedState(areaViewModel, "AreaShortcuts", "Key", "north-wing");
        Assert.Equal("Area: Khu B.", GetRequiredProperty<string>(areaViewModel, "StatusMessage"));

        var navViewModel = CreateViewModel();
        var supportNav = FindCollectionItemByProperty(navViewModel, "TopNavItems", "Key", "support");

        ExecuteRequiredCommand(navViewModel, "SelectTopNavCommand", supportNav);

        var fallbackCards = GetCollectionItems(navViewModel, "FeedCards");

        Assert.Equal(
        [
            "move-in-guide",
            "maintenance-slot"
        ],
        fallbackCards.Select(card => GetRequiredProperty<string>(card, "Id")).ToArray());
        Assert.All(fallbackCards, card => Assert.True(GetRequiredProperty<bool>(card, "UseFallbackVisual")));
        AssertSelectedState(navViewModel, "TopNavItems", "Key", "support");
        Assert.Equal(2, GetCollectionItems(navViewModel, "ActivityItems").Count);
        Assert.Equal(3, GetCollectionItems(navViewModel, "EmergencyContacts").Count);
        Assert.Equal("Preview lane: Hỗ trợ.", GetRequiredProperty<string>(navViewModel, "StatusMessage"));
    }

    [Fact]
    public void Preview_state_exposes_long_text_fallback_and_support_metadata()
    {
        var viewModel = CreateViewModel();
        var feedCards = GetCollectionItems(viewModel, "FeedCards");
        var areaShortcuts = GetCollectionItems(viewModel, "AreaShortcuts");
        var activityItems = GetCollectionItems(viewModel, "ActivityItems");
        var emergencyContacts = GetCollectionItems(viewModel, "EmergencyContacts");

        Assert.All(areaShortcuts, shortcut => Assert.True(string.IsNullOrWhiteSpace(GetReferenceProperty(shortcut, "Subtitle") as string)));

        var longTitleCard = FindCollectionItemByProperty(viewModel, "FeedCards", "Id", "quiet-hours");
        Assert.True(GetRequiredProperty<string>(longTitleCard, "Title").Length > 40);
        Assert.False(string.IsNullOrWhiteSpace(GetRequiredProperty<string>(longTitleCard, "AuthorAvatarLabel")));

        var fallbackCard = feedCards.First(card => GetRequiredProperty<bool>(card, "UseFallbackVisual"));
        Assert.False(string.IsNullOrWhiteSpace(GetReferenceProperty(fallbackCard, "FallbackBadgeText") as string));
        Assert.False(string.IsNullOrWhiteSpace(GetReferenceProperty(fallbackCard, "FallbackAccentTone") as string));

        Assert.All(activityItems, item => Assert.False(string.IsNullOrWhiteSpace(GetReferenceProperty(item, "Subtitle") as string)));
        Assert.All(emergencyContacts, item =>
        {
            Assert.False(string.IsNullOrWhiteSpace(GetReferenceProperty(item, "Title") as string));
            Assert.False(string.IsNullOrWhiteSpace(GetReferenceProperty(item, "ContactText") as string));
            Assert.True(string.IsNullOrWhiteSpace(GetReferenceProperty(item, "AvailabilityText") as string));
        });
    }

    [Fact]
    public void Message_notification_and_profile_commands_toggle_dismissible_preview_panels()
    {
        var viewModel = CreateViewModel();

        AssertCollectionProperty(viewModel, "PreviewPanels", minimumCount: 3);
        Assert.False(GetRequiredProperty<bool>(viewModel, "HasOpenPreviewPanel"));

        ExecuteRequiredCommand(viewModel, "OpenMessagesCommand");
        Assert.Equal("messages", GetRequiredProperty<string>(viewModel, "OpenPreviewPanelKey"));
        Assert.True(GetRequiredProperty<bool>(viewModel, "HasOpenPreviewPanel"));
        AssertPreviewPanelState(viewModel, "messages");

        ExecuteRequiredCommand(viewModel, "OpenNotificationCenterCommand");
        Assert.Equal("notifications", GetRequiredProperty<string>(viewModel, "OpenPreviewPanelKey"));
        AssertPreviewPanelState(viewModel, "notifications");

        ExecuteRequiredCommand(viewModel, "OpenProfilePreviewCommand");
        Assert.Equal("profile", GetRequiredProperty<string>(viewModel, "OpenPreviewPanelKey"));
        AssertPreviewPanelState(viewModel, "profile");

        ExecuteRequiredCommand(viewModel, "ClosePreviewPanelCommand");
        Assert.False(GetRequiredProperty<bool>(viewModel, "HasOpenPreviewPanel"));
        Assert.Null(GetReferenceProperty(viewModel, "OpenPreviewPanelKey"));
        Assert.All(GetCollectionItems(viewModel, "PreviewPanels"), panel => Assert.False(GetRequiredProperty<bool>(panel, "IsOpen")));
    }

    [Fact]
    public void Feed_actions_stay_preview_only_without_mutating_reference_collections()
    {
        var viewModel = CreateViewModel();
        var feedCardIds = GetCollectionItems(viewModel, "FeedCards").Select(card => GetRequiredProperty<string>(card, "Id")).ToArray();
        var firstFeedCard = FindCollectionItemByProperty(viewModel, "FeedCards", "Id", "study-refresh");
        var initialLikeCountText = GetRequiredProperty<string>(firstFeedCard, "LikeCountText");
        var initialCommentCountText = GetRequiredProperty<string>(firstFeedCard, "CommentCountText");
        var initialLikeState = GetRequiredProperty<bool>(firstFeedCard, "IsLiked");

        ExecuteRequiredCommand(viewModel, "ToggleLikeCommand", firstFeedCard);

        Assert.Equal(!initialLikeState, GetRequiredProperty<bool>(firstFeedCard, "IsLiked"));
        Assert.Equal(initialLikeCountText, GetRequiredProperty<string>(firstFeedCard, "LikeCountText"));
        Assert.Equal(initialCommentCountText, GetRequiredProperty<string>(firstFeedCard, "CommentCountText"));
        Assert.Equal(feedCardIds, GetCollectionItems(viewModel, "FeedCards").Select(card => GetRequiredProperty<string>(card, "Id")).ToArray());
        Assert.Equal("Post saved in preview mode.", GetRequiredProperty<string>(viewModel, "StatusMessage"));

        ExecuteRequiredCommand(viewModel, "ComposeCommand");

        Assert.Equal(feedCardIds, GetCollectionItems(viewModel, "FeedCards").Select(card => GetRequiredProperty<string>(card, "Id")).ToArray());
        Assert.True(GetRequiredProperty<bool>(viewModel, "IsUsingPreviewData"));
        Assert.True(GetRequiredProperty<bool>(viewModel, "IsComposeDialogOpen"));
    }

    [Fact]
    public void Activity_preview_toggles_local_join_state_without_mutating_reference_items()
    {
        var viewModel = CreateViewModel();
        var activityIds = GetCollectionItems(viewModel, "ActivityItems").Select(item => GetRequiredProperty<string>(item, "Id")).ToArray();
        var activityItem = FindCollectionItemByProperty(viewModel, "ActivityItems", "Id", "activity-football");

        ExecuteRequiredCommand(viewModel, "OpenActivityPreviewCommand", activityItem);

        Assert.True(GetRequiredProperty<bool>(activityItem, "IsPreviewJoined"));
        Assert.Equal(activityIds, GetCollectionItems(viewModel, "ActivityItems").Select(item => GetRequiredProperty<string>(item, "Id")).ToArray());
        Assert.Equal("Đã lưu hoạt động xem trước: Giải bóng đá sinh viên nam KTX.", GetRequiredProperty<string>(viewModel, "StatusMessage"));

        ExecuteRequiredCommand(viewModel, "OpenActivityPreviewCommand", activityItem);

        Assert.False(GetRequiredProperty<bool>(activityItem, "IsPreviewJoined"));
        Assert.Equal(activityIds, GetCollectionItems(viewModel, "ActivityItems").Select(item => GetRequiredProperty<string>(item, "Id")).ToArray());
        Assert.Equal("Đã đóng hoạt động xem trước: Giải bóng đá sinh viên nam KTX.", GetRequiredProperty<string>(viewModel, "StatusMessage"));
        Assert.True(GetRequiredProperty<bool>(viewModel, "IsUsingPreviewData"));
    }

    [Fact]
    public void Activity_header_add_command_stays_preview_only()
    {
        var viewModel = new ForumHomeViewModel();
        var activityIds = viewModel.ActivityItems.Select(item => item.Id).ToArray();

        Assert.True(viewModel.OpenActivityQuickAddCommand.CanExecute(null));

        viewModel.OpenActivityQuickAddCommand.Execute(null);

        Assert.Equal(activityIds, viewModel.ActivityItems.Select(item => item.Id).ToArray());
        Assert.Equal("Opened activity popup in preview mode.", viewModel.StatusMessage);
        Assert.True(viewModel.IsUsingPreviewData);
        Assert.False(viewModel.IsComposeDialogOpen);
        Assert.True(viewModel.IsActivityDialogOpen);
    }

    [Fact]
    public void Emergency_contact_preview_opens_and_closes_local_dialog_state_only()
    {
        var viewModel = CreateViewModel();
        var contactIds = GetCollectionItems(viewModel, "EmergencyContacts").Select(item => GetRequiredProperty<string>(item, "Id")).ToArray();
        var guardContact = FindCollectionItemByProperty(viewModel, "EmergencyContacts", "Id", "contact-guard");
        var officeContact = FindCollectionItemByProperty(viewModel, "EmergencyContacts", "Id", "contact-office");

        ExecuteRequiredCommand(viewModel, "OpenEmergencyContactPreviewCommand", guardContact);

        Assert.True(GetRequiredProperty<bool>(viewModel, "HasEmergencyContactPreview"));
        var selectedContact = GetReferenceProperty(viewModel, "SelectedEmergencyContact");
        Assert.NotNull(selectedContact);
        Assert.Equal("contact-guard", GetRequiredProperty<string>(selectedContact, "Id"));
        Assert.Equal("Sẵn sàng kết nối xem trước.", GetRequiredProperty<string>(guardContact, "PreviewDialState"));
        Assert.Null(GetReferenceProperty(officeContact, "PreviewDialState"));
        Assert.Equal("Đang xem trước cuộc gọi khẩn cấp: Phòng Bảo vệ KTX.", GetRequiredProperty<string>(viewModel, "StatusMessage"));

        ExecuteRequiredCommand(viewModel, "DismissEmergencyContactPreviewCommand");

        Assert.False(GetRequiredProperty<bool>(viewModel, "HasEmergencyContactPreview"));
        Assert.Null(GetReferenceProperty(viewModel, "SelectedEmergencyContact"));
        Assert.Null(GetReferenceProperty(guardContact, "PreviewDialState"));
        Assert.Equal(contactIds, GetCollectionItems(viewModel, "EmergencyContacts").Select(item => GetRequiredProperty<string>(item, "Id")).ToArray());
        Assert.Equal("Đã đóng xem trước liên hệ khẩn cấp.", GetRequiredProperty<string>(viewModel, "StatusMessage"));
        Assert.True(GetRequiredProperty<bool>(viewModel, "IsUsingPreviewData"));
    }

    [Fact]
    public void Compose_command_stays_local_after_old_forum_cleanup()
    {
        var viewModel = new ForumHomeViewModel();

        Assert.True(viewModel.ComposeCommand.CanExecute(null));

        viewModel.ComposeCommand.Execute(null);

        Assert.True(viewModel.IsComposeDialogOpen);
        Assert.Equal("Đã mở popup tạo bài viết.", viewModel.StatusMessage);
        Assert.True(viewModel.IsUsingPreviewData);
    }

    [Fact]
    public void Compose_dialog_submit_and_close_stay_preview_only()
    {
        var viewModel = new ForumHomeViewModel();
        var feedCardIds = viewModel.FeedCards.Select(card => card.Id).ToArray();

        viewModel.ComposeCommand.Execute(null);

        Assert.True(viewModel.ShowComposeDialog);
        Assert.Equal("Đăng bài viết mới", viewModel.ComposeDialogTitle);
        Assert.Equal("Tiêu đề bài đăng *", viewModel.ComposeTitleLabel);
        Assert.Equal("Nội dung chi tiết *", viewModel.ComposeContentLabel);
        Assert.Equal("Thẻ phụ (Tags)", viewModel.ComposeCustomTagsLabel);
        Assert.True(viewModel.SubmitComposePreviewCommand.CanExecute(null));

        viewModel.ComposeTitleText = "Thông báo thử nghiệm";
        viewModel.ComposeDraftText = "Thông báo thử nghiệm trong bản xem trước";
        viewModel.ComposeCustomTagsText = "#wifi";
        viewModel.SubmitComposePreviewCommand.Execute(null);

        Assert.False(viewModel.ShowComposeDialog);
        Assert.Equal(string.Empty, viewModel.ComposeTitleText);
        Assert.Equal(string.Empty, viewModel.ComposeDraftText);
        Assert.Equal(string.Empty, viewModel.ComposeCustomTagsText);
        Assert.Equal(feedCardIds, viewModel.FeedCards.Select(card => card.Id).ToArray());
        Assert.Equal("Bài viết đã được lưu ở chế độ xem trước.", viewModel.StatusMessage);

        viewModel.ComposeCommand.Execute(null);
        Assert.True(viewModel.CloseComposeDialogCommand.CanExecute(null));

        viewModel.CloseComposeDialogCommand.Execute(null);

        Assert.False(viewModel.ShowComposeDialog);
        Assert.Equal("Đã hủy tạo bài viết.", viewModel.StatusMessage);
    }
    [Fact]
    public void Compose_dialog_contract_matches_popup_recovery_plan()
    {
        var viewModel = new ForumHomeViewModel();

        viewModel.ComposeCommand.Execute(null);

        Assert.True(viewModel.ShowComposeDialog);
        Assert.Equal("Đăng bài viết mới", viewModel.ComposeDialogTitle);
        Assert.Equal("Tiêu đề bài đăng *", viewModel.ComposeTitleLabel);
        Assert.Equal("Danh mục *", viewModel.ComposeCategoryLabel);
        Assert.Equal("Nội dung chi tiết *", viewModel.ComposeContentLabel);
        Assert.Equal("Thẻ phụ (Tags)", viewModel.ComposeCustomTagsLabel);
        Assert.Equal("Đã mở popup tạo bài viết.", viewModel.StatusMessage);
        Assert.Null(viewModel.GetType().GetProperty("ComposeDialogSubtitle", BindingFlags.Public | BindingFlags.Instance));
        Assert.Null(viewModel.GetType().GetProperty("ComposeRegionLabel", BindingFlags.Public | BindingFlags.Instance));
        Assert.Null(viewModel.GetType().GetProperty("ComposePopularTagsLabel", BindingFlags.Public | BindingFlags.Instance));

        viewModel.CloseComposeDialogCommand.Execute(null);

        Assert.Equal("Đã hủy tạo bài viết.", viewModel.StatusMessage);
    }
    [Fact]
    public void Compose_tag_chips_toggle_independently_and_allow_multiple_selection()
    {
        var viewModel = new ForumHomeViewModel();
        var announcementTag = FindCollectionItemByProperty(viewModel, "ComposeTagChips", "Key", "clubs");
        var eventTag = FindCollectionItemByProperty(viewModel, "ComposeTagChips", "Key", "quiet-study");
        var moveInTag = FindCollectionItemByProperty(viewModel, "ComposeTagChips", "Key", "move-in");

        Assert.True(GetRequiredProperty<bool>(announcementTag, "IsSelected"));
        Assert.True(GetRequiredProperty<bool>(eventTag, "IsSelected"));
        Assert.False(GetRequiredProperty<bool>(moveInTag, "IsSelected"));
        AssertNoSelectedState(viewModel, "TagChips");

        ExecuteRequiredCommand(viewModel, "ToggleComposeTagCommand", moveInTag);

        Assert.True(GetRequiredProperty<bool>(announcementTag, "IsSelected"));
        Assert.True(GetRequiredProperty<bool>(eventTag, "IsSelected"));
        Assert.True(GetRequiredProperty<bool>(moveInTag, "IsSelected"));
        AssertNoSelectedState(viewModel, "TagChips");
        Assert.Equal(string.Empty, GetRequiredProperty<string>(viewModel, "SelectedTagKey"));

        ExecuteRequiredCommand(viewModel, "ToggleComposeTagCommand", eventTag);

        Assert.True(GetRequiredProperty<bool>(announcementTag, "IsSelected"));
        Assert.False(GetRequiredProperty<bool>(eventTag, "IsSelected"));
        Assert.True(GetRequiredProperty<bool>(moveInTag, "IsSelected"));
        AssertNoSelectedState(viewModel, "TagChips");
    }

    [Fact]
    public void Home_popular_tag_chips_toggle_independently_and_allow_multiple_selection()
    {
        var viewModel = new ForumHomeViewModel();
        var quietStudyTag = FindCollectionItemByProperty(viewModel, "TagChips", "Key", "quiet-study");
        var moveInTag = FindCollectionItemByProperty(viewModel, "TagChips", "Key", "move-in");

        ExecuteRequiredCommand(viewModel, "SelectTagCommand", quietStudyTag);
        ExecuteRequiredCommand(viewModel, "SelectTagCommand", moveInTag);

        Assert.True(GetRequiredProperty<bool>(quietStudyTag, "IsSelected"));
        Assert.True(GetRequiredProperty<bool>(moveInTag, "IsSelected"));
        Assert.Equal("move-in", GetRequiredProperty<string>(viewModel, "SelectedTagKey"));
        Assert.Equal(
        [
            "study-refresh",
            "quiet-hours",
            "move-in-guide",
            "maintenance-slot"
        ],
        GetCollectionItems(viewModel, "FeedCards").Select(card => GetRequiredProperty<string>(card, "Id")).ToArray());

        ExecuteRequiredCommand(viewModel, "SelectTagCommand", quietStudyTag);

        Assert.False(GetRequiredProperty<bool>(quietStudyTag, "IsSelected"));
        Assert.True(GetRequiredProperty<bool>(moveInTag, "IsSelected"));
        Assert.Equal("move-in", GetRequiredProperty<string>(viewModel, "SelectedTagKey"));
        Assert.Equal(
        [
            "move-in-guide",
            "maintenance-slot"
        ],
        GetCollectionItems(viewModel, "FeedCards").Select(card => GetRequiredProperty<string>(card, "Id")).ToArray());
    }

    [Fact]
    public void Activity_quick_link_stays_local_after_old_forum_cleanup()
    {
        var viewModel = new ForumHomeViewModel();
        var activityItem = Assert.IsType<ForumHomeActivityItem>(viewModel.ActivityItems.First());

        Assert.True(viewModel.OpenActivityPreviewCommand.CanExecute(activityItem));

        viewModel.OpenActivityPreviewCommand.Execute(activityItem);

        Assert.True(activityItem.IsPreviewJoined);
        Assert.Equal($"Đã lưu hoạt động xem trước: {activityItem.Title}.", viewModel.StatusMessage);
        Assert.True(viewModel.IsUsingPreviewData);
    }

    [Fact]
    public void Reset_filters_command_restores_default_preview_state()
    {
        var viewModel = CreateViewModel();
        var category = FindCollectionItemByProperty(viewModel, "CategoryItems", "Key", "events");
        var area = FindCollectionItemByProperty(viewModel, "AreaShortcuts", "Key", "north-wing");

        ExecuteRequiredCommand(viewModel, "SelectCategoryCommand", category);
        ExecuteRequiredCommand(viewModel, "SelectAreaCommand", area);
        SetRequiredProperty(viewModel, "SearchText", "khong-co-ket-qua");

        ExecuteRequiredCommand(viewModel, "ResetFiltersCommand");

        Assert.Equal(string.Empty, GetRequiredProperty<string>(viewModel, "SearchText"));
        Assert.False(GetRequiredProperty<bool>(viewModel, "HasSearchInput"));
        Assert.False(GetRequiredProperty<bool>(viewModel, "HasActivePreviewFilters"));
        Assert.False(GetRequiredProperty<bool>(viewModel, "HasEmptyResults"));
        Assert.Equal("Đã xóa bộ lọc xem trước.", GetRequiredProperty<string>(viewModel, "StatusMessage"));

        AssertSelectedState(viewModel, "TopNavItems", "Key", "home");
        AssertSelectedState(viewModel, "CategoryItems", "Key", "announcements");
        AssertNoSelectedState(viewModel, "TagChips");
        AssertNoSelectedState(viewModel, "AreaShortcuts");

        Assert.Equal(
        [
            "study-refresh",
            "club-rehearsal",
            "quiet-hours",
            "move-in-guide",
            "maintenance-slot"
        ],
        GetCollectionItems(viewModel, "FeedCards").Select(card => GetRequiredProperty<string>(card, "Id")).ToArray());
    }

    [Fact]
    public void Reset_filters_command_closes_preview_surfaces_and_clears_support_preview_state()
    {
        var viewModel = CreateViewModel();
        var category = FindCollectionItemByProperty(viewModel, "CategoryItems", "Key", "events");
        var guardContact = FindCollectionItemByProperty(viewModel, "EmergencyContacts", "Id", "contact-guard");

        ExecuteRequiredCommand(viewModel, "SelectCategoryCommand", category);
        ExecuteRequiredCommand(viewModel, "OpenMessagesCommand");
        Assert.True(GetRequiredProperty<bool>(viewModel, "HasOpenPreviewPanel"));

        ExecuteRequiredCommand(viewModel, "ResetFiltersCommand");

        Assert.False(GetRequiredProperty<bool>(viewModel, "HasOpenPreviewPanel"));
        Assert.Null(GetReferenceProperty(viewModel, "OpenPreviewPanelKey"));

        ExecuteRequiredCommand(viewModel, "SelectCategoryCommand", category);
        ExecuteRequiredCommand(viewModel, "OpenEmergencyContactPreviewCommand", guardContact);
        Assert.True(GetRequiredProperty<bool>(viewModel, "HasEmergencyContactPreview"));

        ExecuteRequiredCommand(viewModel, "ResetFiltersCommand");

        Assert.False(GetRequiredProperty<bool>(viewModel, "HasEmergencyContactPreview"));
        Assert.Null(GetReferenceProperty(viewModel, "SelectedEmergencyContact"));
        Assert.Null(GetReferenceProperty(guardContact, "PreviewDialState"));
        Assert.Equal("Đã xóa bộ lọc xem trước.", GetRequiredProperty<string>(viewModel, "StatusMessage"));
    }
    [Fact]
    public void Activity_quick_add_command_opens_activity_popup_contract()
    {
        var viewModel = new ForumHomeViewModel();
        var activityIds = viewModel.ActivityItems.Select(item => item.Id).ToArray();

        Assert.True(viewModel.OpenActivityQuickAddCommand.CanExecute(null));

        viewModel.OpenActivityQuickAddCommand.Execute(null);

        Assert.Equal(activityIds, viewModel.ActivityItems.Select(item => item.Id).ToArray());
        Assert.True(GetRequiredProperty<bool>(viewModel, "IsActivityDialogOpen"));
        Assert.False(GetRequiredProperty<bool>(viewModel, "IsComposeDialogOpen"));
        Assert.Equal("Create New Activity", GetRequiredProperty<string>(viewModel, "ActivityDialogTitle"));
        Assert.Equal("Cancel", GetRequiredProperty<string>(viewModel, "ActivitySecondaryActionText"));
        Assert.Equal("Create Activity", GetRequiredProperty<string>(viewModel, "ActivityPrimaryActionText"));
        Assert.Equal("Activity Name", GetRequiredProperty<string>(viewModel, "ActivityNameLabel"));
        Assert.Equal("Category", GetRequiredProperty<string>(viewModel, "ActivityCategoryLabel"));
        Assert.Equal("Date", GetRequiredProperty<string>(viewModel, "ActivityDateLabel"));
        Assert.Equal("Time", GetRequiredProperty<string>(viewModel, "ActivityTimeLabel"));
        Assert.Equal("Location", GetRequiredProperty<string>(viewModel, "ActivityLocationLabel"));
        Assert.Equal("Description", GetRequiredProperty<string>(viewModel, "ActivityDescriptionLabel"));
        Assert.True(GetRequiredProperty<bool>(viewModel, "IsUsingPreviewData"));
    }

    [Fact]
    public void Activity_popup_submit_and_close_stay_preview_only()
    {
        var viewModel = new ForumHomeViewModel();
        var activityIds = viewModel.ActivityItems.Select(item => item.Id).ToArray();

        viewModel.OpenActivityQuickAddCommand.Execute(null);

        Assert.True(GetRequiredProperty<bool>(viewModel, "ShowActivityDialog"));
        Assert.True(GetRequiredProperty<bool>(viewModel, "CanSubmitActivityDialog"));

        SetRequiredProperty(viewModel, "ActivityNameText", "Weekend Basketball Tournament");
        SetRequiredProperty(viewModel, "ActivityLocationText", "Main Courtyard");
        SetRequiredProperty(viewModel, "ActivityDateText", "2026-06-07");
        SetRequiredProperty(viewModel, "ActivityTimeText", "18:30");
        SetRequiredProperty(viewModel, "ActivityDescriptionText", "Bring sportswear and water.");

        var category = FindCollectionItemByProperty(viewModel, "ActivityCategoryOptions", "Key", "sports");
        ExecuteRequiredCommand(viewModel, "SelectActivityCategoryCommand", category);
        ExecuteRequiredCommand(viewModel, "SubmitActivityPreviewCommand");

        Assert.False(GetRequiredProperty<bool>(viewModel, "ShowActivityDialog"));
        Assert.Equal(activityIds, viewModel.ActivityItems.Select(item => item.Id).ToArray());
        Assert.Equal(string.Empty, GetRequiredProperty<string>(viewModel, "ActivityNameText"));
        Assert.Equal(string.Empty, GetRequiredProperty<string>(viewModel, "ActivityLocationText"));
        Assert.Equal(string.Empty, GetRequiredProperty<string>(viewModel, "ActivityDateText"));
        Assert.Equal(string.Empty, GetRequiredProperty<string>(viewModel, "ActivityTimeText"));
        Assert.Equal(string.Empty, GetRequiredProperty<string>(viewModel, "ActivityDescriptionText"));
        Assert.True(GetRequiredProperty<bool>(viewModel, "IsUsingPreviewData"));

        viewModel.OpenActivityQuickAddCommand.Execute(null);
        Assert.True(GetRequiredProperty<bool>(viewModel, "CanCloseActivityDialog"));
        ExecuteRequiredCommand(viewModel, "CloseActivityDialogCommand");
        Assert.False(GetRequiredProperty<bool>(viewModel, "ShowActivityDialog"));
    }
    private static object CreateViewModel()
    {
        var viewModelType = Type.GetType(ViewModelTypeName);

        Assert.NotNull(viewModelType);

        var constructor = viewModelType.GetConstructor(Type.EmptyTypes);
        Assert.NotNull(constructor);

        return constructor.Invoke([]);
    }

    private static void AssertCollectionProperty(object instance, string propertyName, int minimumCount)
    {
        var value = GetReferenceProperty(instance, propertyName);

        Assert.NotNull(value);
        Assert.IsAssignableFrom<IEnumerable>(value);
        Assert.True(Count((IEnumerable)value) >= minimumCount, $"{propertyName} should contain at least {minimumCount} preview items.");
    }

    private static object GetFirstCollectionItem(object instance, string propertyName)
    {
        var value = GetReferenceProperty(instance, propertyName);

        Assert.NotNull(value);
        Assert.IsAssignableFrom<IEnumerable>(value);

        foreach (var item in (IEnumerable)value)
        {
            if (item is not null)
            {
                return item;
            }
        }

        throw new Xunit.Sdk.XunitException($"{propertyName} should contain at least one item.");
    }

    private static object FindCollectionItemByProperty(object instance, string propertyName, string itemPropertyName, string expectedValue)
    {
        var item = GetCollectionItems(instance, propertyName)
            .FirstOrDefault(candidate => string.Equals(GetReferenceProperty(candidate, itemPropertyName) as string, expectedValue, StringComparison.Ordinal));

        return item ?? throw new Xunit.Sdk.XunitException($"{propertyName} should contain an item where {itemPropertyName} == '{expectedValue}'.");
    }

    private static void AssertSelectedState(object instance, string propertyName, string itemPropertyName, string selectedValue)
    {
        foreach (var item in GetCollectionItems(instance, propertyName))
        {
            var isSelected = GetRequiredProperty<bool>(item, "IsSelected");
            var currentValue = GetReferenceProperty(item, itemPropertyName) as string;

            if (string.Equals(currentValue, selectedValue, StringComparison.Ordinal))
            {
                Assert.True(isSelected, $"{propertyName} item '{selectedValue}' should be selected.");
            }
            else
            {
                Assert.False(isSelected, $"{propertyName} item '{currentValue}' should not stay selected.");
            }
        }
    }

    private static void AssertNoSelectedState(object instance, string propertyName)
    {
        foreach (var item in GetCollectionItems(instance, propertyName))
        {
            Assert.False(GetRequiredProperty<bool>(item, "IsSelected"), $"{propertyName} should not retain a selected item.");
        }
    }

    private static void AssertPreviewPanelState(object instance, string openPanelKey)
    {
        foreach (var panel in GetCollectionItems(instance, "PreviewPanels"))
        {
            var key = GetRequiredProperty<string>(panel, "PanelType");
            var isOpen = GetRequiredProperty<bool>(panel, "IsOpen");

            if (string.Equals(key, openPanelKey, StringComparison.Ordinal))
            {
                Assert.True(isOpen, $"Preview panel '{openPanelKey}' should be open.");
                Assert.True(GetRequiredProperty<bool>(panel, "HasItems"));
            }
            else
            {
                Assert.False(isOpen, $"Preview panel '{key}' should be closed.");
            }
        }
    }

    private static void ExecuteRequiredCommand(object instance, string propertyName, object? parameter = null)
    {
        var command = GetRequiredProperty<ICommand>(instance, propertyName);

        Assert.True(command.CanExecute(parameter), $"{propertyName} should be executable for the review-build placeholder flow.");
        command.Execute(parameter);
    }

    private static void AssertFeedCard(
        object feedCard,
        string expectedId,
        string expectedTitle,
        string expectedExcerpt,
        string expectedAuthor,
        string expectedTime,
        string expectedViews,
        string expectedLikes,
        string expectedComments)
    {
        Assert.Equal(expectedId, GetRequiredProperty<string>(feedCard, "Id"));
        Assert.Equal(expectedTitle, GetRequiredProperty<string>(feedCard, "Title"));
        Assert.Equal(expectedExcerpt, GetRequiredProperty<string>(feedCard, "Excerpt"));
        Assert.Equal(expectedAuthor, GetRequiredProperty<string>(feedCard, "AuthorName"));
        Assert.Equal(expectedTime, GetRequiredProperty<string>(feedCard, "RelativeTimeText"));
        Assert.Equal(expectedViews, GetRequiredProperty<string>(feedCard, "ViewCountText"));
        Assert.Equal(expectedLikes, GetRequiredProperty<string>(feedCard, "LikeCountText"));
        Assert.Equal(expectedComments, GetRequiredProperty<string>(feedCard, "CommentCountText"));
    }

    private static T GetRequiredProperty<T>(object instance, string propertyName)
    {
        var property = instance.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

        Assert.NotNull(property);

        var value = property.GetValue(instance);

        Assert.NotNull(value);
        return Assert.IsAssignableFrom<T>(value);
    }

    private static object? GetReferenceProperty(object instance, string propertyName)
    {
        var property = instance.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

        Assert.NotNull(property);
        return property.GetValue(instance);
    }

    private static void SetRequiredProperty(object instance, string propertyName, object? value)
    {
        var property = instance.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

        Assert.NotNull(property);
        Assert.True(property.CanWrite, $"{propertyName} should be writable for local preview interaction.");
        property.SetValue(instance, value);
    }

    private static List<object> GetCollectionItems(object instance, string propertyName)
    {
        var value = GetReferenceProperty(instance, propertyName);

        Assert.NotNull(value);
        Assert.IsAssignableFrom<IEnumerable>(value);

        return ((IEnumerable)value)
            .Cast<object?>()
            .Where(item => item is not null)
            .Cast<object>()
            .ToList();
    }

    private static int Count(IEnumerable source)
    {
        var count = 0;
        foreach (var _ in source)
        {
            count++;
        }

        return count;
    }

    private sealed class RecordingNavigationService : INavigationService
    {
        public Type? LastViewModelType { get; private set; }

        public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase
        {
            LastViewModelType = typeof(TViewModel);
        }
    }
}






