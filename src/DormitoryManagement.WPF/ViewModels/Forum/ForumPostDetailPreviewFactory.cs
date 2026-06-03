namespace DormitoryManagement.WPF.ViewModels.Forum;

public static class ForumPostDetailPreviewFactory
{
    private const string ForumPostDetailAssetBaseUri = "pack://application:,,,/DormitoryManagement.WPF;component/Assets/Images/ForumPostDetail/";
    private const string ForumHomeAssetBaseUri = "pack://application:,,,/DormitoryManagement.WPF;component/Assets/Images/ForumHome/";
    private const string SafetyImagePath = ForumPostDetailAssetBaseUri + "forum-post-detail-hero.png";
    private const string StudyImagePath = ForumHomeAssetBaseUri + "forum-home-cover-study.png";
    private const string ClubImagePath = ForumHomeAssetBaseUri + "forum-home-cover-club.png";

    public static ForumPostDetailPreviewState CreateDefault() => CreateForPostId(null);

    public static ForumPostDetailPreviewState CreateForPostId(string? postId)
    {
        var resolvedPostId = string.IsNullOrWhiteSpace(postId) ? "maintenance-detail" : postId;
        var article = CreateArticle(resolvedPostId);

        return new ForumPostDetailPreviewState(
            brandText: "DMForum",
            searchPlaceholder: "Search forum...",
            breadcrumbItems:
            [
                new ForumPostDetailBreadcrumbItem("Home", "Home"),
                new ForumPostDetailBreadcrumbItem("General News", "ChevronRight"),
                new ForumPostDetailBreadcrumbItem("Thông báo lịch bảo trì...", "ChevronRight", isCurrent: true)
            ],
            headerUserSummary: new ForumPostDetailHeaderUserSummary("U"),
            article: article,
            composer: new ForumPostDetailComposer(
                avatarLabel: "U",
                placeholder: "Add to the discussion...",
                submitLabel: "Post"),
            comments:
            [
                new ForumPostDetailCommentItem(
                    id: "comment-nguyen-van-a",
                    author: "Nguyen Van A",
                    avatarLabel: "N",
                    relativeTimeText: "1 hour ago",
                    message: "Cho em hỏi là wifi có bị ngắt theo điện luôn không ạ? Sáng T7 em có ca thi online.",
                    likeCountText: "12",
                    replyLabel: "Reply",
                    reportLabel: "Report"),
                new ForumPostDetailCommentItem(
                    id: "reply-ban-quan-ly",
                    author: "Ban Quản Lý",
                    avatarLabel: "BQ",
                    relativeTimeText: "45 mins ago",
                    message: "Chào em, hệ thống mạng sử dụng nguồn dự phòng nên wifi vẫn hoạt động bình thường nhé. Tuy nhiên khuyên em nên sạc đầy laptop đề phòng.",
                    likeCountText: "8",
                    replyLabel: "Reply",
                    reportLabel: string.Empty,
                    isVerified: true,
                    isReply: true)
            ],
            categories:
            [
                new ForumPostDetailCategoryItem("Tin tức chung", "Newspaper", isSelected: true),
                new ForumPostDetailCategoryItem("CLB & Sự kiện", "Groups"),
                new ForumPostDetailCategoryItem("Mua bán & trao đổi", "SwapHorizontalCircle")
            ],
            relatedPosts:
            [
                new ForumPostDetailRelatedPostItem("wifi-guide", "Hướng dẫn cài đặt Wifi mới cho khu A và C", "Ban Quản Lý • 2 days ago"),
                new ForumPostDetailRelatedPostItem("shared-room-rules", "Quy định mới về việc sử dụng phòng sinh hoạt chung", "Ban Quản Lý • 1 tuần trước")
            ],
            trendingTags:
            [
                new ForumPostDetailTrendingTagItem("#thongbao (142)"),
                new ForumPostDetailTrendingTagItem("#timdo (89)"),
                new ForumPostDetailTrendingTagItem("#baotri (56)"),
                new ForumPostDetailTrendingTagItem("#giaitri (45)"),
                new ForumPostDetailTrendingTagItem("#hoctap (30)")
            ],
            isUsingPreviewData: true);
    }

    private static ForumPostDetailArticle CreateArticle(string postId) =>
        postId switch
        {
            "wifi-guide" => new ForumPostDetailArticle(
                title: "Hướng dẫn cài đặt Wifi mới cho khu A và C",
                author: "Ban Quản Lý",
                authorBadgeText: "check_circle",
                relativeTimeText: "2 days ago",
                likeCountText: "118",
                commentCountText: "14",
                shareLabel: "Share",
                imagePath: StudyImagePath,
                tags:
                [
                    new ForumPostDetailTagChip("#thongbao", "primary"),
                    new ForumPostDetailTagChip("#wifi", "neutral")
                ],
                bodyParagraphs:
                [
                    "Ban Quản Lý vừa hoàn tất cấu hình mạng mới cho khu A và C.",
                    "Sinh viên vui lòng quên mạng cũ, sau đó đăng nhập lại bằng mã sinh viên để bảo đảm tốc độ và ổn định kết nối."
                ],
                infoRows:
                [
                    new ForumPostDetailInfoRow("calendar_clock", "Thời gian:", "Có hiệu lực ngay hôm nay."),
                    new ForumPostDetailInfoRow("location_on", "Phạm vi:", "Khu A và Khu C."),
                    new ForumPostDetailInfoRow("build", "Nội dung:", "Thay đổi SSID, làm mới chứng thực, tối ưu tải thiết bị.")
                ],
                warningText: "Nếu thiết bị vẫn không kết nối được sau khi làm theo hướng dẫn, vui lòng báo về phòng trực mạng nội bộ.",
                signatureText: "Trân trọng,\nBan Quản Lý"),
            "shared-room-rules" => new ForumPostDetailArticle(
                title: "Quy định mới về việc sử dụng phòng sinh hoạt chung",
                author: "Ban Quản Lý",
                authorBadgeText: "check_circle",
                relativeTimeText: "1 tuần trước",
                likeCountText: "92",
                commentCountText: "11",
                shareLabel: "Share",
                imagePath: ClubImagePath,
                tags:
                [
                    new ForumPostDetailTagChip("#thongbao", "primary"),
                    new ForumPostDetailTagChip("#sinhhoatchung", "neutral")
                ],
                bodyParagraphs:
                [
                    "Phòng sinh hoạt chung sẽ áp dụng lịch đăng ký theo ca để bảo đảm công bằng cho các nhóm sử dụng.",
                    "Mỗi nhóm cần giữ vệ sinh, trả lại hiện trạng ban đầu, và tuân thủ giờ yên tĩnh sau 22h00."
                ],
                infoRows:
                [
                    new ForumPostDetailInfoRow("calendar_clock", "Thời gian:", "Áp dụng từ đầu tuần tới."),
                    new ForumPostDetailInfoRow("location_on", "Phạm vi:", "Toàn bộ phòng sinh hoạt chung."),
                    new ForumPostDetailInfoRow("build", "Nội dung:", "Đăng ký trước, giới hạn thời lượng, tăng trách nhiệm vệ sinh.")
                ],
                warningText: "Nhóm vi phạm nhiều lần có thể bị tạm dừng quyền đặt lịch trong kỳ hiện tại.",
                signatureText: "Trân trọng,\nBan Quản Lý"),
            _ => new ForumPostDetailArticle(
                title: "Thông báo lịch bảo trì điện nước khu B",
                author: "Ban Quản Lý",
                authorBadgeText: "check_circle",
                relativeTimeText: "2 hours ago",
                likeCountText: "245",
                commentCountText: "32",
                shareLabel: "Share",
                imagePath: SafetyImagePath,
                tags:
                [
                    new ForumPostDetailTagChip("#thongbao", "primary"),
                    new ForumPostDetailTagChip("#baotri", "neutral")
                ],
                bodyParagraphs:
                [
                    "Kính gửi toàn thể sinh viên khu B,",
                    "Nhằm đảm bảo hệ thống điện nước hoạt động ổn định trong học kỳ tới, Ban Quản Lý Ký túc xá xin thông báo lịch bảo trì định kỳ cụ thể như sau:",
                    "Trong thời gian trên, nguồn điện và nước có thể bị gián đoạn cục bộ. Kính mong các bạn sinh viên chủ động sắp xếp lịch sinh hoạt cá nhân, sạc đầy các thiết bị cần thiết và dự trữ nước sinh hoạt cơ bản."
                ],
                infoRows:
                [
                    new ForumPostDetailInfoRow("calendar_clock", "Thời gian:", "8:00 AM - 11:30 AM, Thứ Bảy tuần này."),
                    new ForumPostDetailInfoRow("location_on", "Phạm vi:", "Toàn bộ các tòa nhà thuộc Khu B."),
                    new ForumPostDetailInfoRow("build", "Nội dung:", "Kiểm tra trạm biến áp cục bộ và súc rửa đường ống nước sinh hoạt.")
                ],
                warningText: "Mọi thắc mắc khẩn cấp trong thời gian bảo trì, vui lòng liên hệ hotline trực ban: 0123.456.789.",
                signatureText: "Trân trọng,\nBan Quản Lý")
        };
}
