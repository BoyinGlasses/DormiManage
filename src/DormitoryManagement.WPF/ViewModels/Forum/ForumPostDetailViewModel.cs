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
    private readonly IForumCommentService? _forumCommentService;
    private readonly IForumReactionService? _forumReactionService;
    private readonly ForumNavigationState? _forumNavigationState;
    private ForumPostDetailArticle _article;
    private ForumPostDetailComposer _composer;
    private string? _statusMessage;
    private bool _isUsingPreviewData;
    private Guid? _replyParentCommentId;

    public ForumPostDetailViewModel()
        : this(null, null, null, null, null, null)
    {
    }

    public ForumPostDetailViewModel(INavigationService navigationService)
        : this(navigationService, null)
    {
    }

    public ForumPostDetailViewModel(INavigationService? navigationService, string? selectedPostId)
        : this(navigationService, null, null, null, null, selectedPostId)
    {
    }

    public ForumPostDetailViewModel(IForumPostService forumPostService, ForumNavigationState forumNavigationState)
        : this(null, forumPostService, null, null, forumNavigationState, null)
    {
    }

    public ForumPostDetailViewModel(
        IForumPostService forumPostService,
        IForumCommentService forumCommentService,
        IForumReactionService forumReactionService,
        ForumNavigationState forumNavigationState)
        : this(null, forumPostService, forumCommentService, forumReactionService, forumNavigationState, null)
    {
    }

    public ForumPostDetailViewModel(
        INavigationService navigationService,
        IForumPostService forumPostService,
        IForumCommentService forumCommentService,
        IForumReactionService forumReactionService,
        ForumNavigationState forumNavigationState)
        : this(navigationService, forumPostService, forumCommentService, forumReactionService, forumNavigationState, null)
    {
    }

    public ForumPostDetailViewModel(
        INavigationService? navigationService,
        IForumPostService? forumPostService,
        ForumNavigationState? forumNavigationState)
        : this(navigationService, forumPostService, null, null, forumNavigationState, null)
    {
    }

    private ForumPostDetailViewModel(
        INavigationService? navigationService,
        IForumPostService? forumPostService,
        IForumCommentService? forumCommentService,
        IForumReactionService? forumReactionService,
        ForumNavigationState? forumNavigationState,
        string? selectedPostId)
    {
        _navigationService = navigationService;
        _forumPostService = forumPostService;
        _forumCommentService = forumCommentService;
        _forumReactionService = forumReactionService;
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
        ToggleLikeCommand = new RelayCommand(ToggleLike, parameter => parameter is ForumPostDetailArticle or ForumPostDetailCommentItem);
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
        _navigationService?.NavigateTo<ForumHomeViewModel>();
        StatusMessage = "Returned to forum.";
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
        StatusMessage = $"Viewing related post: {relatedPost.Title}.";
    }

    private void OpenCategory(object? parameter)
    {
        if (parameter is ForumPostDetailCategoryItem category)
        {
            StatusMessage = $"Preview category: {category.Text}.";
        }
    }

    private void OpenTrendingTag(object? parameter)
    {
        if (parameter is ForumPostDetailTrendingTagItem tag)
        {
            StatusMessage = $"Preview tag: {tag.Text}.";
        }
    }

    private void ToggleLike(object? parameter)
    {
        if (parameter is ForumPostDetailArticle article)
        {
            if (_forumReactionService is not null && _forumNavigationState?.SelectedPostId is { } postId)
            {
                _ = TogglePostLikeAsync(postId);
                return;
            }

            article.IsLiked = !article.IsLiked;
            StatusMessage = article.IsLiked ? "Preview like saved." : "Preview like removed.";
            return;
        }

        if (parameter is ForumPostDetailCommentItem comment && _forumReactionService is not null && Guid.TryParse(comment.Id, out var commentId))
        {
            _ = ToggleCommentLikeAsync(commentId);
        }
    }

    private void Reply(object? parameter)
    {
        if (parameter is not ForumPostDetailCommentItem comment)
        {
            return;
        }

        _replyParentCommentId = Guid.TryParse(comment.Id, out var commentId) ? commentId : null;
        if (string.IsNullOrWhiteSpace(Composer.DraftText))
        {
            Composer.DraftText = $"@{comment.Author} ";
        }

        StatusMessage = $"Replying to {comment.Author}.";
    }

    private void Report(object? parameter)
    {
        if (parameter is ForumPostDetailCommentItem comment)
        {
            StatusMessage = $"Preview report recorded for {comment.Author}.";
        }
    }

    private void OpenCreatePost()
    {
        if (_navigationService is not null)
        {
            ForumHomeComposeRequest.PendingOpen = true;
            _navigationService.NavigateTo<ForumHomeViewModel>();
        }

        StatusMessage = "Opening create post flow.";
    }

    private void SubmitCommentDraft()
    {
        if (_forumCommentService is not null && _forumNavigationState?.SelectedPostId is { } postId)
        {
            _ = SubmitCommentDraftAsync(postId);
            return;
        }

        Composer.DraftText = string.Empty;
        StatusMessage = "Preview comment saved.";
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
            await LoadCommentsAsync(postId);
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
        _composer = new ForumPostDetailComposer(HeaderUserSummary.AvatarLabel, "Add to the discussion...", "Post");
        OnPropertyChanged(nameof(Article));
        OnPropertyChanged(nameof(Composer));
    }

    private async Task LoadCommentsAsync(Guid postId)
    {
        if (_forumCommentService is null)
        {
            return;
        }

        var comments = await _forumCommentService.GetPostCommentsAsync(postId);
        ReplaceCollection(Comments, FlattenComments(comments));
    }

    private async Task SubmitCommentDraftAsync(Guid postId)
    {
        if (_forumCommentService is null)
        {
            return;
        }

        var result = await _forumCommentService.CreateAsync(new CreateForumCommentRequest
        {
            ForumPostId = postId,
            ParentCommentId = _replyParentCommentId,
            Content = Composer.DraftText
        });

        if (!result.Succeeded)
        {
            StatusMessage = result.Error;
            return;
        }

        Composer.DraftText = string.Empty;
        _replyParentCommentId = null;
        await LoadSelectedPostAsync();
        StatusMessage = "Comment saved.";
    }

    private async Task TogglePostLikeAsync(Guid postId)
    {
        if (_forumReactionService is null)
        {
            return;
        }

        var result = await _forumReactionService.TogglePostLikeAsync(postId);
        if (!result.Succeeded)
        {
            StatusMessage = result.Error;
            return;
        }

        await LoadSelectedPostAsync();
        StatusMessage = "Post like updated.";
    }

    private async Task ToggleCommentLikeAsync(Guid commentId)
    {
        if (_forumReactionService is null || _forumNavigationState?.SelectedPostId is not { } postId)
        {
            return;
        }

        var result = await _forumReactionService.ToggleCommentLikeAsync(commentId);
        if (!result.Succeeded)
        {
            StatusMessage = result.Error;
            return;
        }

        await LoadCommentsAsync(postId);
        StatusMessage = "Comment like updated.";
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

    private static IReadOnlyList<ForumPostDetailCommentItem> FlattenComments(IReadOnlyList<ForumCommentDto> comments)
    {
        var items = new List<ForumPostDetailCommentItem>();
        foreach (var comment in comments)
        {
            AddComment(items, comment, isReply: false);
        }

        return items;
    }

    private static void AddComment(ICollection<ForumPostDetailCommentItem> target, ForumCommentDto comment, bool isReply)
    {
        target.Add(new ForumPostDetailCommentItem(
            id: comment.Id.ToString(),
            author: comment.AuthorName,
            avatarLabel: CreateAvatarLabel(comment.AuthorName),
            relativeTimeText: FormatRelativeTime(comment.CreatedAt),
            message: comment.Content,
            likeCountText: comment.LikeCount.ToString(),
            replyLabel: "Reply",
            reportLabel: "Report",
            isVerified: string.Equals(comment.AuthorRole, "Admin", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(comment.AuthorRole, "Manager", StringComparison.OrdinalIgnoreCase),
            isReply: isReply));

        foreach (var reply in comment.Replies)
        {
            AddComment(target, reply, isReply: true);
        }
    }

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

    private static string CreateAvatarLabel(string name)
    {
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            return "U";
        }

        return string.Concat(parts.Take(2).Select(part => char.ToUpperInvariant(part[0])));
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
