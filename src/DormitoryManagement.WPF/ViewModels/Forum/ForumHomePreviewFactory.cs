namespace DormitoryManagement.WPF.ViewModels.Forum;

public static class ForumHomePreviewFactory
{
    private const string ForumHomeAssetBaseUri = "pack://application:,,,/DormitoryManagement.WPF;component/Assets/Images/ForumHome/";

    public static IReadOnlyList<ForumHomeActivityCategoryOption> CreateActivityCategoryOptions() =>
    [
        new ForumHomeActivityCategoryOption("sports", "Sports", "Basketball", "primary", isSelected: true),
        new ForumHomeActivityCategoryOption("music", "Music", "MusicNote", "secondary"),
        new ForumHomeActivityCategoryOption("study", "Study Group", "BookOpenVariant", "tertiary"),
        new ForumHomeActivityCategoryOption("volunteer", "Volunteer", "HandHeart", "primary")
    ];

    public static ForumHomePreviewState CreateDefault() =>        new(
            new ForumHomeUserSummary(
                displayName: "Nguyễn Văn A",
                subtitle: string.Empty,
                avatarLabel: "NVA",
                roomLabel: "Phòng 402 - Khu B",
                showChevron: true,
                avatarAssetPath: ForumHomeAssetBaseUri + "forum-home-avatar-student.png"),
            CreateTopNavItems(),
            selectedTopNavKey: "home",
            CreateCategoryItems(),
            selectedCategoryKey: "announcements",
            CreatePopularTags(),
            selectedTagKey: string.Empty,
            CreateAreaShortcuts(),
            selectedAreaKey: string.Empty,
            searchText: string.Empty,
            composePrompt: "Bạn có thông tin gì muốn chia sẻ với mọi người?",
            CreatePostCards(),
            CreateActivityItems(),
            CreateEmergencyContacts(),
            isUsingPreviewData: true,
            statusMessage: null,
            brandText: "DMForum",
            searchPlaceholder: "Tìm kiếm tin tức, thông báo...");

    private static IReadOnlyList<ForumHomeNavItem> CreateTopNavItems() =>
    [
        new ForumHomeNavItem("home", "Trang chủ", "Home", isSelected: true),
        new ForumHomeNavItem("calendar", "Lịch", "CalendarToday"),
        new ForumHomeNavItem("groups", "Cộng đồng", "AccountGroupOutline"),
        new ForumHomeNavItem("campaign", "Thông báo", "BullhornOutline"),
        new ForumHomeNavItem("support", "Hỗ trợ", "Headset")
    ];

    private static IReadOnlyList<ForumHomeNavItem> CreateCategoryItems() =>
    [
        new ForumHomeNavItem("announcements", "Tin tức chung", "NewspaperVariantOutline", isSelected: true),
        new ForumHomeNavItem("events", "CLB & Sự kiện", "AccountGroupOutline"),
        new ForumHomeNavItem("housing", "Mua bán & Trao đổi", "StorefrontOutline"),
        new ForumHomeNavItem("market", "Góp ý & Khiếu nại", "MessageAlertOutline")
    ];

    private static IReadOnlyList<ForumHomeTagChip> CreatePopularTags() =>
    [
        new ForumHomeTagChip("clubs", "#thongbao", "secondary"),
        new ForumHomeTagChip("quiet-study", "#clbsukien", "tertiary"),
        new ForumHomeTagChip("move-in", "#passdo", "primary"),
        new ForumHomeTagChip("maintenance", "#cangtin", "neutral"),
        new ForumHomeTagChip("sports", "#thethao", "secondary"),
        new ForumHomeTagChip("power", "#matdien", "primary")
    ];

    private static IReadOnlyList<ForumHomeAreaShortcut> CreateAreaShortcuts() =>
    [
        new ForumHomeAreaShortcut("study-hall", "A", "Khu A", "DeskLamp"),
        new ForumHomeAreaShortcut("north-wing", "B", "Khu B", "Domain"),
        new ForumHomeAreaShortcut("front-desk", "C", "Nhà ăn & Dịch vụ", "Desk")
    ];

    private static IReadOnlyList<ForumHomePostCard> CreatePostCards() =>
    [
        new ForumHomePostCard(
            id: "study-refresh",
            coverAssetPath: null,
            useFallbackVisual: false,
            overlayTitle: null,
            overlaySubtitle: null,
            priorityLabel: null,
            badges:
            [
                new ForumHomeBadge("Thông báo", "secondary"),
                new ForumHomeBadge("Bảo trì", "primary")
            ],
            title: "Thông báo lịch bảo trì điện nước khu B",
            excerpt: "Ban quản lý KTX xin thông báo: Từ 8h00 đến 11h30 sáng thứ 7 tuần này, khu B sẽ tạm ngưng cấp điện, nước để tiến hành bảo trì hệ thống định kỳ. Mong các bạn sinh viên chủ động sắp xếp...",
            authorName: "Ban Quản Lý",
            authorAvatarAssetPath: ForumHomeAssetBaseUri + "forum-home-author-management.png",
            authorAvatarLabel: "BQL",
            authorSubtitle: string.Empty,
            relativeTimeText: "2 giờ trước",
            viewCountText: "1.2k",
            likeCountText: "45",
            commentCountText: "12",
            areaLabelText: null,
            areaOverlayEmphasis: null,
            authorRoleText: "Văn phòng KTX",
            isImportant: true),
        new ForumHomePostCard(
            id: "club-rehearsal",
            coverAssetPath: null,
            useFallbackVisual: false,
            overlayTitle: null,
            overlaySubtitle: null,
            priorityLabel: null,
            badges:
            [
                new ForumHomeBadge("Tuyển thành viên", "tertiary"),
                new ForumHomeBadge("Tình nguyện", "secondary")
            ],
            title: "Tuyển thành viên CLB Tình nguyện KTX",
            excerpt: "CLB Tình nguyện đang tìm kiếm các bạn sinh viên nhiệt huyết tham gia chiến dịch 'Mùa hè xanh' tại khuôn viên kí túc xá. Nếu bạn yêu thích các hoạt động cộng đồng và muốn đóng góp sức trẻ, hãy tham gia cùng chúng mình.",
            authorName: "Đội Tình Nguyện",
            authorAvatarAssetPath: ForumHomeAssetBaseUri + "forum-home-author-volunteer.png",
            authorAvatarLabel: "ĐTN",
            authorSubtitle: string.Empty,
            relativeTimeText: "Hôm qua",
            viewCountText: "120",
            likeCountText: "24",
            commentCountText: "5",
            authorRoleText: "CLB Sinh Viên KTX",
            isLiked: true),
        new ForumHomePostCard(
            id: "quiet-hours",
            coverAssetPath: null,
            useFallbackVisual: false,
            overlayTitle: null,
            overlaySubtitle: null,
            priorityLabel: null,
            badges:
            [
                new ForumHomeBadge("Review", "primary"),
                new ForumHomeBadge("Đời sống", "neutral")
            ],
            title: "Review quán cơm mới mở cạnh căng tin khu C",
            excerpt: "Hôm nay mình mới thử quán cơm bình dân mới khai trương ở căng tin khu C. Suất 30k mà được khá nhiều thịt, có cả canh chua và trà đá miễn phí. Cô chủ quán rất nhiệt tình...",
            authorName: "Lan Anh",
            authorAvatarAssetPath: ForumHomeAssetBaseUri + "forum-home-author-lan-anh.png",
            authorAvatarLabel: "LA",
            authorSubtitle: string.Empty,
            relativeTimeText: "3 ngày trước",
            viewCountText: "850",
            likeCountText: "120",
            commentCountText: "34",
            authorRoleText: "Phòng 201 - Căng tin C",
            isLiked: true),
        new ForumHomePostCard(
            id: "move-in-guide",
            coverAssetPath: null,
            useFallbackVisual: true,
            overlayTitle: "Hỗ trợ",
            overlaySubtitle: "Văn phòng Ban Quản lý",
            priorityLabel: "Preview",
            badges:
            [
                new ForumHomeBadge("Liên hệ", "primary"),
                new ForumHomeBadge("Khẩn cấp", "neutral")
            ],
            title: "Hướng dẫn liên hệ nhanh khi cần hỗ trợ ngoài giờ tại ký túc xá",
            excerpt: "Danh bạ bảo vệ, trạm y tế, và văn phòng ban quản lý được ghim để sinh viên mở nhanh trong bản xem trước.",
            authorName: "Ban Quản Lý",
            authorAvatarAssetPath: ForumHomeAssetBaseUri + "forum-home-author-management.png",
            authorAvatarLabel: "BQL",
            authorSubtitle: string.Empty,
            relativeTimeText: "2 giờ trước",
            viewCountText: "320",
            likeCountText: "18",
            commentCountText: "2",
            fallbackBadgeText: "Preview",
            fallbackAccentTone: "secondary",
            authorRoleText: "Văn phòng KTX"),
        new ForumHomePostCard(
            id: "maintenance-slot",
            coverAssetPath: null,
            useFallbackVisual: true,
            overlayTitle: "Khu vực",
            overlaySubtitle: "Tòa B",
            priorityLabel: "Preview",
            badges:
            [
                new ForumHomeBadge("Thông báo", "primary"),
                new ForumHomeBadge("Bảo trì", "neutral")
            ],
            title: "Nhắc lại khung giờ bảo trì điện nước để sinh viên chủ động sắp xếp sinh hoạt",
            excerpt: "Bản ghi preview dùng cho lane hỗ trợ, không thay đổi dữ liệu diễn đàn thật.",
            authorName: "Ban Quản Lý",
            authorAvatarAssetPath: ForumHomeAssetBaseUri + "forum-home-author-management.png",
            authorAvatarLabel: "BQL",
            authorSubtitle: string.Empty,
            relativeTimeText: "2 giờ trước",
            viewCountText: "180",
            likeCountText: "9",
            commentCountText: "1",
            fallbackBadgeText: "Preview",
            fallbackAccentTone: "primary",
            areaLabelText: "Khu vực",
            areaOverlayEmphasis: "Preview",
            authorRoleText: "Văn phòng KTX")
    ];

    private static IReadOnlyList<ForumHomeActivityItem> CreateActivityItems() =>
    [
        new ForumHomeActivityItem(
            id: "activity-football",
            monthText: "Th5",
            dayText: "12",
            title: "Giải bóng đá sinh viên nam KTX",
            locationText: "Sân cỏ nhân tạo A",
            accentTone: "secondary"),
        new ForumHomeActivityItem(
            id: "activity-acoustic",
            monthText: "Th5",
            dayText: "15",
            title: "Đêm nhạc Acoustic gây quỹ",
            locationText: "Sân sinh hoạt chung",
            accentTone: "primary")
    ];

    private static IReadOnlyList<ForumHomeEmergencyContactItem> CreateEmergencyContacts() =>
    [
        new ForumHomeEmergencyContactItem(
            id: "contact-guard",
            name: "Phòng Bảo vệ KTX",
            description: string.Empty,
            phoneText: "024 38xxx 111",
            accentTone: "primary",
            iconKind: "PhoneInTalk"),
        new ForumHomeEmergencyContactItem(
            id: "contact-medical",
            name: "Trạm y tế Tòa nhà C",
            description: string.Empty,
            phoneText: "024 38xxx 115",
            accentTone: "tertiary",
            iconKind: "MedicalBag"),
        new ForumHomeEmergencyContactItem(
            id: "contact-office",
            name: "Văn phòng Ban Quản lý",
            description: string.Empty,
            phoneText: "024 38xxx 222",
            accentTone: "secondary",
            iconKind: "ShieldAccount")
    ];
}

