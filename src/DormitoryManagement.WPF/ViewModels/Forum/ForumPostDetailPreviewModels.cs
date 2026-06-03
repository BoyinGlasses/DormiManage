using DormitoryManagement.WPF.Common;

namespace DormitoryManagement.WPF.ViewModels.Forum;

public sealed class ForumPostDetailPreviewState
{
    public ForumPostDetailPreviewState(
        string brandText,
        string searchPlaceholder,
        IReadOnlyList<ForumPostDetailBreadcrumbItem> breadcrumbItems,
        ForumPostDetailHeaderUserSummary headerUserSummary,
        ForumPostDetailArticle article,
        ForumPostDetailComposer composer,
        IReadOnlyList<ForumPostDetailCommentItem> comments,
        IReadOnlyList<ForumPostDetailCategoryItem> categories,
        IReadOnlyList<ForumPostDetailRelatedPostItem> relatedPosts,
        IReadOnlyList<ForumPostDetailTrendingTagItem> trendingTags,
        bool isUsingPreviewData,
        string? statusMessage = null)
    {
        BrandText = brandText;
        SearchPlaceholder = searchPlaceholder;
        BreadcrumbItems = breadcrumbItems;
        HeaderUserSummary = headerUserSummary;
        Article = article;
        Composer = composer;
        Comments = comments;
        Categories = categories;
        RelatedPosts = relatedPosts;
        TrendingTags = trendingTags;
        IsUsingPreviewData = isUsingPreviewData;
        StatusMessage = statusMessage;
    }

    public string BrandText { get; }
    public string SearchPlaceholder { get; }
    public IReadOnlyList<ForumPostDetailBreadcrumbItem> BreadcrumbItems { get; }
    public ForumPostDetailHeaderUserSummary HeaderUserSummary { get; }
    public ForumPostDetailArticle Article { get; }
    public ForumPostDetailComposer Composer { get; }
    public IReadOnlyList<ForumPostDetailCommentItem> Comments { get; }
    public IReadOnlyList<ForumPostDetailCategoryItem> Categories { get; }
    public IReadOnlyList<ForumPostDetailRelatedPostItem> RelatedPosts { get; }
    public IReadOnlyList<ForumPostDetailTrendingTagItem> TrendingTags { get; }
    public bool IsUsingPreviewData { get; }
    public string? StatusMessage { get; }
}

public sealed class ForumPostDetailBreadcrumbItem
{
    public ForumPostDetailBreadcrumbItem(string text, string iconKind, bool isCurrent = false)
    {
        Text = text;
        IconKind = iconKind;
        IsCurrent = isCurrent;
    }

    public string Text { get; }
    public string IconKind { get; }
    public bool IsCurrent { get; }
}

public sealed class ForumPostDetailHeaderUserSummary
{
    public ForumPostDetailHeaderUserSummary(string avatarLabel)
    {
        AvatarLabel = avatarLabel;
    }

    public string AvatarLabel { get; }
}

public sealed class ForumPostDetailArticle : ObservableObject
{
    private bool _isLiked;

    public ForumPostDetailArticle(
        string title,
        string author,
        string authorBadgeText,
        string relativeTimeText,
        string likeCountText,
        string commentCountText,
        string shareLabel,
        string imagePath,
        IReadOnlyList<ForumPostDetailTagChip> tags,
        IReadOnlyList<string> bodyParagraphs,
        IReadOnlyList<ForumPostDetailInfoRow> infoRows,
        string warningText,
        string signatureText,
        bool isLiked = false)
    {
        Title = title;
        Author = author;
        AuthorBadgeText = authorBadgeText;
        RelativeTimeText = relativeTimeText;
        LikeCountText = likeCountText;
        CommentCountText = commentCountText;
        ShareLabel = shareLabel;
        ImagePath = imagePath;
        Tags = tags;
        BodyParagraphs = bodyParagraphs;
        InfoRows = infoRows;
        WarningText = warningText;
        SignatureText = signatureText;
        _isLiked = isLiked;
    }

    public string Title { get; }
    public string Author { get; }
    public string AuthorBadgeText { get; }
    public string RelativeTimeText { get; }
    public string LikeCountText { get; }
    public string CommentCountText { get; }
    public string ShareLabel { get; }
    public string ImagePath { get; }
    public IReadOnlyList<ForumPostDetailTagChip> Tags { get; }
    public IReadOnlyList<string> BodyParagraphs { get; }
    public IReadOnlyList<ForumPostDetailInfoRow> InfoRows { get; }
    public string WarningText { get; }
    public string SignatureText { get; }

    public bool IsLiked
    {
        get => _isLiked;
        set => SetProperty(ref _isLiked, value);
    }
}

public sealed class ForumPostDetailTagChip
{
    public ForumPostDetailTagChip(string text, string tone)
    {
        Text = text;
        Tone = tone;
    }

    public string Text { get; }
    public string Tone { get; }
}

public sealed class ForumPostDetailInfoRow
{
    public ForumPostDetailInfoRow(string iconKind, string label, string value)
    {
        IconKind = iconKind;
        Label = label;
        Value = value;
    }

    public string IconKind { get; }
    public string Label { get; }
    public string Value { get; }
}

public sealed class ForumPostDetailComposer : ObservableObject
{
    private string _draftText;

    public ForumPostDetailComposer(string avatarLabel, string placeholder, string submitLabel, string draftText = "")
    {
        AvatarLabel = avatarLabel;
        Placeholder = placeholder;
        SubmitLabel = submitLabel;
        _draftText = draftText;
    }

    public string AvatarLabel { get; }
    public string Placeholder { get; }
    public string SubmitLabel { get; }

    public string DraftText
    {
        get => _draftText;
        set => SetProperty(ref _draftText, value);
    }
}

public sealed class ForumPostDetailCommentItem
{
    public ForumPostDetailCommentItem(
        string id,
        string author,
        string avatarLabel,
        string relativeTimeText,
        string message,
        string likeCountText,
        string replyLabel,
        string reportLabel,
        bool isVerified = false,
        bool isReply = false)
    {
        Id = id;
        Author = author;
        AvatarLabel = avatarLabel;
        RelativeTimeText = relativeTimeText;
        Message = message;
        LikeCountText = likeCountText;
        ReplyLabel = replyLabel;
        ReportLabel = reportLabel;
        IsVerified = isVerified;
        IsReply = isReply;
    }

    public string Id { get; }
    public string Author { get; }
    public string AvatarLabel { get; }
    public string RelativeTimeText { get; }
    public string Message { get; }
    public string LikeCountText { get; }
    public string ReplyLabel { get; }
    public string ReportLabel { get; }
    public bool IsVerified { get; }
    public bool IsReply { get; }
}

public sealed class ForumPostDetailCategoryItem
{
    public ForumPostDetailCategoryItem(string text, string iconKind, bool isSelected = false)
    {
        Text = text;
        IconKind = iconKind;
        IsSelected = isSelected;
    }

    public string Text { get; }
    public string IconKind { get; }
    public bool IsSelected { get; }
}

public sealed class ForumPostDetailRelatedPostItem
{
    public ForumPostDetailRelatedPostItem(string id, string title, string metaText)
    {
        Id = id;
        Title = title;
        MetaText = metaText;
    }

    public string Id { get; }
    public string Title { get; }
    public string MetaText { get; }
}

public sealed class ForumPostDetailTrendingTagItem
{
    public ForumPostDetailTrendingTagItem(string text)
    {
        Text = text;
    }

    public string Text { get; }
}
