using System.Collections.ObjectModel;
using System.Windows.Input;
using DormitoryManagement.Application.DTOs.Forum;
using DormitoryManagement.Application.Services.Forum;
using DormitoryManagement.WPF.Common;
using DormitoryManagement.WPF.Navigation;

namespace DormitoryManagement.WPF.ViewModels.Forum;

public sealed class ForumPostDetailViewModel : ViewModelBase
{
    private readonly INavigationService? _navigationService;
    private readonly IForumPostService? _forumPostService;
    private readonly ForumNavigationState? _forumNavigationState;
    private ForumPostDetailArticle _article;
    private ForumPostDetailComposer _composer;
    private string? _statusMessage;
    private bool _isUsingPreviewData;

    public ForumPostDetailViewModel()
        : this(null, null, null, null)
    {
    }

    public ForumPostDetailViewModel(INavigationService navigationService)
        : this(navigationService, null)
    {
    }

    public ForumPostDetailViewModel(INavigationService? navigationService, string? selectedPostId)
        : this(navigationService, null, null, selectedPostId)
    {
    }

    public ForumPostDetailViewModel(IForumPostService forumPostService, ForumNavigationState forumNavigationState)
        : this(null, forumPostService, forumNavigationState, null)
    {
    }

    public ForumPostDetailViewModel(
        INavigationService? navigationService,
        IForumPostService? forumPostService,
        ForumNavigationState? forumNavigationState)
        : this(navigationService, forumPostService, forumNavigationState, null)
    {
    }

    private ForumPostDetailViewModel(
        INavigationService? navigationService,
        IForumPostService? forumPostService,
        ForumNavigationState? forumNavigationState,
        string? selectedPostId)
    {
        _navigationService = navigationService;
        _forumPostService = forumPostService;
        _forumNavigationState = forumNavigationState;
        var state = ForumPostDetailPreviewFactory.CreateForPostId(selectedPostId);
        BrandText = state.BrandText;
        SearchPlaceholder = state.SearchPlaceholder;
        HeaderUserSummary = state.HeaderUserSummary;
        BreadcrumbItems = new ObservableCollection<ForumPostDetailBreadcrumbItem>(state.BreadcrumbItems);
        _article = state.Article;
        _composer = state.Composer;
        Comments = new ObservableCollection<ForumPostDetailCommentItem>(state.Comments);
        Categories = new ObservableCollection<ForumPostDetailCategoryItem>(state.Categories);
        RelatedPosts = new ObservableCollection<ForumPostDetailRelatedPostItem>(state.RelatedPosts);
        TrendingTags = new ObservableCollection<ForumPostDetailTrendingTagItem>(state.TrendingTags);
        _isUsingPreviewData = state.IsUsingPreviewData;
        _statusMessage = state.StatusMessage;

        BackToForumCommand = new RelayCommand(BackToForum);
        OpenRelatedPostCommand = new RelayCommand(OpenRelatedPost, parameter => parameter is ForumPostDetailRelatedPostItem);
        OpenCategoryCommand = new RelayCommand(OpenCategory, parameter => parameter is ForumPostDetailCategoryItem);
        OpenTrendingTagCommand = new RelayCommand(OpenTrendingTag, parameter => parameter is ForumPostDetailTrendingTagItem);
        ToggleLikeCommand = new RelayCommand(ToggleLike, parameter => parameter is ForumPostDetailArticle);
        ReplyCommand = new RelayCommand(Reply, parameter => parameter is ForumPostDetailCommentItem);
        ReportCommand = new RelayCommand(Report, parameter => parameter is ForumPostDetailCommentItem);
        SubmitCommentDraftCommand = new RelayCommand(SubmitCommentDraft);
        OpenCreatePostCommand = new RelayCommand(OpenCreatePost);

        _ = LoadSelectedPostAsync();
    }

    public string BrandText { get; }
    public string SearchPlaceholder { get; }
    public ForumPostDetailHeaderUserSummary HeaderUserSummary { get; }
    public ObservableCollection<ForumPostDetailBreadcrumbItem> BreadcrumbItems { get; }
    public ObservableCollection<ForumPostDetailCommentItem> Comments { get; }
    public ObservableCollection<ForumPostDetailCategoryItem> Categories { get; }
    public ObservableCollection<ForumPostDetailRelatedPostItem> RelatedPosts { get; }
    public ObservableCollection<ForumPostDetailTrendingTagItem> TrendingTags { get; }
    public bool IsUsingPreviewData
    {
        get => _isUsingPreviewData;
        private set => SetProperty(ref _isUsingPreviewData, value);
    }
    public ICommand BackToForumCommand { get; }
    public ICommand OpenRelatedPostCommand { get; }
    public ICommand OpenCategoryCommand { get; }
    public ICommand OpenTrendingTagCommand { get; }
    public ICommand ToggleLikeCommand { get; }
    public ICommand ReplyCommand { get; }
    public ICommand ReportCommand { get; }
    public ICommand SubmitCommentDraftCommand { get; }
    public ICommand OpenCreatePostCommand { get; }

    public ForumPostDetailArticle Article => _article;
    public ForumPostDetailComposer Composer => _composer;

    public string? StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    private void BackToForum()
    {
        if (_navigationService is not null)
        {
            _navigationService.NavigateTo<ForumHomeViewModel>();
        }

        StatusMessage = "Đã quay lại luồng diễn đàn xem trước.";
    }

    private void OpenRelatedPost(object? parameter)
    {
        if (parameter is not ForumPostDetailRelatedPostItem relatedPost)
        {
            return;
        }

        if (_forumNavigationState is not null && Guid.TryParse(relatedPost.Id, out var postId))
        {
            _forumNavigationState.SelectedPostId = postId;
            _ = LoadSelectedPostAsync();
            return;
        }

        ApplyState(ForumPostDetailPreviewFactory.CreateForPostId(relatedPost.Id));
        StatusMessage = $"Đang xem bài liên quan: {relatedPost.Title}.";
    }

    private void OpenCategory(object? parameter)
    {
        if (parameter is not ForumPostDetailCategoryItem category)
        {
            return;
        }

        StatusMessage = $"Điều hướng xem trước tới danh mục: {category.Text}.";
    }

    private void OpenTrendingTag(object? parameter)
    {
        if (parameter is not ForumPostDetailTrendingTagItem tag)
        {
            return;
        }

        StatusMessage = $"Điều hướng xem trước tới tag: {tag.Text}.";
    }

    private void ToggleLike(object? parameter)
    {
        if (parameter is not ForumPostDetailArticle article)
        {
            return;
        }

        article.IsLiked = !article.IsLiked;
        StatusMessage = article.IsLiked ? "Đã lưu lượt thích trong bản xem trước." : "Đã bỏ lượt thích trong bản xem trước.";
    }

    private void Reply(object? parameter)
    {
        if (parameter is not ForumPostDetailCommentItem comment)
        {
            return;
        }

        StatusMessage = $"Đang mở trả lời cục bộ cho: {comment.Author}.";
    }

    private void Report(object? parameter)
    {
        if (parameter is not ForumPostDetailCommentItem comment)
        {
            return;
        }

        StatusMessage = $"Đã ghi nhận báo cáo xem trước cho bình luận của {comment.Author}.";
    }

    private void OpenCreatePost()
    {
        if (_navigationService is not null)
        {
            ForumHomeComposeRequest.PendingOpen = true;
            _navigationService.NavigateTo<ForumHomeViewModel>();
        }

        StatusMessage = "Đang chuyển sang luồng tạo bài viết xem trước.";
    }

    private void SubmitCommentDraft()
    {
        Composer.DraftText = string.Empty;
        StatusMessage = "Bình luận đã được lưu ở chế độ xem trước.";
    }

    private async Task LoadSelectedPostAsync()
    {
        if (_forumPostService is null || _forumNavigationState?.SelectedPostId is not { } postId)
        {
            return;
        }

        try
        {
            var result = await _forumPostService.GetByIdAsync(postId);
            if (!result.Succeeded || result.Value is null)
            {
                IsUsingPreviewData = true;
                return;
            }

            ApplyPost(result.Value);
            IsUsingPreviewData = false;
            StatusMessage = null;
        }
        catch (Exception)
        {
            IsUsingPreviewData = true;
        }
    }

    private void ApplyPost(ForumPostDto post)
    {
        ReplaceCollection(BreadcrumbItems, new[]
        {
            new ForumPostDetailBreadcrumbItem("Home", "Home"),
            new ForumPostDetailBreadcrumbItem(post.Category, "ChevronRight"),
            new ForumPostDetailBreadcrumbItem(post.Title, "ChevronRight", isCurrent: true)
        });
        ReplaceCollection(Comments, Array.Empty<ForumPostDetailCommentItem>());
        ReplaceCollection(Categories, CreateCategoryItems(post.Category));
        ReplaceCollection(RelatedPosts, Array.Empty<ForumPostDetailRelatedPostItem>());
        ReplaceCollection(TrendingTags, post.Tags.Select(tag => new ForumPostDetailTrendingTagItem("#" + tag)).ToArray());
        _article = MapArticle(post);
        _composer = new ForumPostDetailComposer(
            HeaderUserSummary.AvatarLabel,
            "Add to the discussion...",
            "Post");
        OnPropertyChanged(nameof(Article));
        OnPropertyChanged(nameof(Composer));
    }

    private static IReadOnlyList<ForumPostDetailCategoryItem> CreateCategoryItems(string selectedCategory)
    {
        var categories = new[] { "Announcements", "Events", "Guides", "General", "Support" };
        return categories
            .Select(category => new ForumPostDetailCategoryItem(category, "Newspaper", string.Equals(category, selectedCategory, StringComparison.OrdinalIgnoreCase)))
            .ToArray();
    }

    private static ForumPostDetailArticle MapArticle(ForumPostDto post) =>
        new(
            title: post.Title,
            author: post.AuthorName,
            authorBadgeText: string.IsNullOrWhiteSpace(post.AuthorRole) ? string.Empty : "check_circle",
            relativeTimeText: FormatRelativeTime(post.CreatedAt),
            likeCountText: post.LikeCount.ToString(),
            commentCountText: post.CommentCount.ToString(),
            shareLabel: "Share",
            imagePath: string.Empty,
            tags: post.Tags.Select(tag => new ForumPostDetailTagChip("#" + tag, "neutral")).ToArray(),
            bodyParagraphs: SplitParagraphs(post.Content),
            infoRows:
            [
                new ForumPostDetailInfoRow("category", "Category:", post.Category),
                new ForumPostDetailInfoRow("location_on", "Area:", post.Area ?? "Dormitory"),
                new ForumPostDetailInfoRow("visibility", "Views:", post.ViewCount.ToString())
            ],
            warningText: post.IsImportant ? "Important forum notice." : string.Empty,
            signatureText: string.IsNullOrWhiteSpace(post.AuthorName) ? string.Empty : post.AuthorName,
            isLiked: post.IsLikedByCurrentUser);

    private static IReadOnlyList<string> SplitParagraphs(string content) =>
        content
            .Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries)
            .Select(paragraph => paragraph.Trim())
            .Where(paragraph => paragraph.Length > 0)
            .DefaultIfEmpty(content)
            .ToArray();

    private static string FormatRelativeTime(DateTime createdAt)
    {
        var elapsed = DateTime.UtcNow - createdAt;
        if (elapsed.TotalMinutes < 60)
        {
            return Math.Max(1, (int)elapsed.TotalMinutes) + " minutes ago";
        }

        if (elapsed.TotalHours < 24)
        {
            return (int)elapsed.TotalHours + " hours ago";
        }

        return (int)elapsed.TotalDays + " days ago";
    }

    private void ApplyState(ForumPostDetailPreviewState state)
    {
        ReplaceCollection(BreadcrumbItems, state.BreadcrumbItems);
        ReplaceCollection(Comments, state.Comments);
        ReplaceCollection(Categories, state.Categories);
        ReplaceCollection(RelatedPosts, state.RelatedPosts);
        ReplaceCollection(TrendingTags, state.TrendingTags);
        _article = state.Article;
        _composer = state.Composer;
        OnPropertyChanged(nameof(Article));
        OnPropertyChanged(nameof(Composer));
    }

    private static void ReplaceCollection<T>(ObservableCollection<T> target, IReadOnlyList<T> items)
    {
        target.Clear();
        foreach (var item in items)
        {
            target.Add(item);
        }
    }
}

