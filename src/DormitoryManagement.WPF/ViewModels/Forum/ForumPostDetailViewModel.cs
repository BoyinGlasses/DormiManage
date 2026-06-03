using System.Collections.ObjectModel;
using System.Windows.Input;
using DormitoryManagement.WPF.Common;
using DormitoryManagement.WPF.Navigation;

namespace DormitoryManagement.WPF.ViewModels.Forum;

public sealed class ForumPostDetailViewModel : ViewModelBase
{
    private readonly INavigationService? _navigationService;
    private ForumPostDetailArticle _article;
    private ForumPostDetailComposer _composer;
    private string? _statusMessage;

    public ForumPostDetailViewModel()
        : this(null, null)
    {
    }

    public ForumPostDetailViewModel(INavigationService navigationService)
        : this(navigationService, null)
    {
    }

    public ForumPostDetailViewModel(INavigationService? navigationService, string? selectedPostId)
    {
        _navigationService = navigationService;
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
        IsUsingPreviewData = state.IsUsingPreviewData;
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
    }

    public string BrandText { get; }
    public string SearchPlaceholder { get; }
    public ForumPostDetailHeaderUserSummary HeaderUserSummary { get; }
    public ObservableCollection<ForumPostDetailBreadcrumbItem> BreadcrumbItems { get; }
    public ObservableCollection<ForumPostDetailCommentItem> Comments { get; }
    public ObservableCollection<ForumPostDetailCategoryItem> Categories { get; }
    public ObservableCollection<ForumPostDetailRelatedPostItem> RelatedPosts { get; }
    public ObservableCollection<ForumPostDetailTrendingTagItem> TrendingTags { get; }
    public bool IsUsingPreviewData { get; }
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

