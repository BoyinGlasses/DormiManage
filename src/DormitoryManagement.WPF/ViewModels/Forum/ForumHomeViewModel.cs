using System.Collections.ObjectModel;
using System.Windows.Input;
using DormitoryManagement.WPF.Common;
using DormitoryManagement.WPF.Navigation;

namespace DormitoryManagement.WPF.ViewModels.Forum;

public sealed partial class ForumHomeViewModel : ViewModelBase
{
    private readonly IReadOnlyList<ForumHomePostCard> _allFeedCards;
    private readonly IReadOnlyList<ForumHomeActivityItem> _allActivityItems;
    private readonly IReadOnlyList<ForumHomeEmergencyContactItem> _allEmergencyContacts;
    private readonly INavigationService? _navigationService;
    private readonly RelayCommand _resetFiltersCommand;
    private readonly RelayCommand _closePreviewPanelCommand;
    private readonly RelayCommand _dismissEmergencyContactPreviewCommand;
    private readonly RelayCommand _closeComposeDialogCommand;
    private readonly RelayCommand _submitComposePreviewCommand;
    private readonly RelayCommand _openActivityQuickAddCommand;
    private readonly string _brandText;
    private readonly string _searchPlaceholder;
    private string _selectedTopNavKey = string.Empty;
    private string _selectedCategoryKey = string.Empty;
    private string _selectedComposeCategoryKey = string.Empty;
    private string _selectedTagKey = string.Empty;
    private string _selectedAreaKey = string.Empty;
    private string _searchText = string.Empty;
    private string? _openPreviewPanelKey;
    private string? _statusMessage;
    private ForumHomeEmergencyContactItem? _selectedEmergencyContact;
    private bool _isComposeDialogOpen;
    private string _composeDraftText = string.Empty;
    private string _composeTitleText = string.Empty;
    private string _composeCustomTagsText = string.Empty;
    private bool _hasEmptyResults;
    private bool _hasAppliedTopNavFilter;
    private bool _hasAppliedCategoryFilter;
    private bool _hasAppliedTagFilter;
    private bool _hasAppliedAreaFilter;

    public ForumHomeViewModel()
        : this(null)
    {
    }

    public ForumHomeViewModel(INavigationService? navigationService)
    {
        _navigationService = navigationService;
        var state = ForumHomePreviewFactory.CreateDefault();
        HeaderUserSummary = state.HeaderUserSummary;
        TopNavItems = new ObservableCollection<ForumHomeNavItem>(state.TopNavItems);
        CategoryItems = new ObservableCollection<ForumHomeNavItem>(state.CategoryItems);
        ComposeCategoryItems = new ObservableCollection<ForumHomeNavItem>(CreateComposeCategoryItems(state.CategoryItems));
        TagChips = new ObservableCollection<ForumHomeTagChip>(state.PopularTags);
        ComposeTagChips = new ObservableCollection<ForumHomeTagChip>(CreateComposeTagChips(state.PopularTags));
        AreaShortcuts = new ObservableCollection<ForumHomeAreaShortcut>(state.AreaShortcuts);
        _allFeedCards = state.PostCards.ToArray();
        _allActivityItems = state.ActivityItems.ToArray();
        _allEmergencyContacts = state.EmergencyContacts.ToArray();
        FeedCards = new ObservableCollection<ForumHomePostCard>(_allFeedCards);
        ActivityItems = new ObservableCollection<ForumHomeActivityItem>(_allActivityItems);
        EmergencyContacts = new ObservableCollection<ForumHomeEmergencyContactItem>(_allEmergencyContacts);
        PreviewPanels = new ObservableCollection<ForumHomePreviewPanel>(
            state.PreviewPanels.Count > 0
                ? state.PreviewPanels
                : CreateDefaultPreviewPanels(HeaderUserSummary));

        _selectedTopNavKey = state.SelectedTopNavKey;
        _selectedCategoryKey = state.SelectedCategoryKey;
        _selectedComposeCategoryKey = state.SelectedCategoryKey;
        _selectedTagKey = state.SelectedTagKey;
        _selectedAreaKey = state.SelectedAreaKey;
        _searchText = state.SearchText;
        _openPreviewPanelKey = state.OpenPreviewPanelKey;
        _statusMessage = state.StatusMessage;
        _brandText = state.BrandText;
        _searchPlaceholder = state.SearchPlaceholder;

        ComposePrompt = state.ComposePrompt;
        IsUsingPreviewData = state.IsUsingPreviewData;
        InitializeActivityDialogState();

        SelectTopNavCommand = new RelayCommand(SelectTopNav, parameter => parameter is ForumHomeNavItem);
        SelectCategoryCommand = new RelayCommand(SelectCategory, parameter => parameter is ForumHomeNavItem);
        SelectComposeCategoryCommand = new RelayCommand(SelectComposeCategory, parameter => parameter is ForumHomeNavItem);
        SelectTagCommand = new RelayCommand(SelectTag, parameter => parameter is ForumHomeTagChip);
        ToggleComposeTagCommand = new RelayCommand(ToggleComposeTag, parameter => parameter is ForumHomeTagChip);
        SelectAreaCommand = new RelayCommand(SelectArea, parameter => parameter is ForumHomeAreaShortcut);
        ComposeCommand = new RelayCommand(ComposePlaceholder);
        _closeComposeDialogCommand = new RelayCommand(CloseComposeDialog, () => IsComposeDialogOpen);
        CloseComposeDialogCommand = _closeComposeDialogCommand;
        _submitComposePreviewCommand = new RelayCommand(SubmitComposePreview, () => IsComposeDialogOpen);
        SubmitComposePreviewCommand = _submitComposePreviewCommand;
        OpenNotificationCenterCommand = new RelayCommand(OpenNotificationPlaceholder);
        OpenMessagesCommand = new RelayCommand(OpenMessagesPlaceholder);
        OpenProfilePreviewCommand = new RelayCommand(OpenProfilePreview);
        _openActivityQuickAddCommand = new RelayCommand(OpenActivityQuickAdd);
        OpenActivityQuickAddCommand = _openActivityQuickAddCommand;
        OpenActivityPreviewCommand = new RelayCommand(OpenActivityPreview, parameter => parameter is ForumHomeActivityItem);
        OpenEmergencyContactPreviewCommand = new RelayCommand(OpenEmergencyContactPreview, parameter => parameter is ForumHomeEmergencyContactItem);
        ToggleLikeCommand = new RelayCommand(ToggleLike, parameter => parameter is ForumHomePostCard);
        OpenPostDetailCommand = new RelayCommand(OpenPostDetail, parameter => parameter is ForumHomePostCard);
        _resetFiltersCommand = new RelayCommand(ResetFilters, () => HasActivePreviewFilters || HasEmptyResults);
        ResetFiltersCommand = _resetFiltersCommand;
        _closePreviewPanelCommand = new RelayCommand(ClosePreviewPanel, () => HasOpenPreviewPanel);
        ClosePreviewPanelCommand = _closePreviewPanelCommand;
        _dismissEmergencyContactPreviewCommand = new RelayCommand(DismissEmergencyContactPreview, () => HasEmergencyContactPreview);
        DismissEmergencyContactPreviewCommand = _dismissEmergencyContactPreviewCommand;

        if (ForumHomeComposeRequest.PendingOpen)
        {
            ForumHomeComposeRequest.PendingOpen = false;
            ComposePlaceholder();
        }
    }

    public ForumHomeUserSummary HeaderUserSummary { get; }
    public ObservableCollection<ForumHomeNavItem> TopNavItems { get; }
    public ObservableCollection<ForumHomeNavItem> CategoryItems { get; }
    public ObservableCollection<ForumHomeNavItem> ComposeCategoryItems { get; }
    public ObservableCollection<ForumHomeTagChip> TagChips { get; }
    public ObservableCollection<ForumHomeTagChip> ComposeTagChips { get; }
    public ObservableCollection<ForumHomeAreaShortcut> AreaShortcuts { get; }
    public ObservableCollection<ForumHomePostCard> FeedCards { get; }
    public ObservableCollection<ForumHomeActivityItem> ActivityItems { get; }
    public ObservableCollection<ForumHomeEmergencyContactItem> EmergencyContacts { get; }
    public ObservableCollection<ForumHomePreviewPanel> PreviewPanels { get; }
    public ICommand SelectTopNavCommand { get; }
    public ICommand SelectCategoryCommand { get; }
    public ICommand SelectComposeCategoryCommand { get; }
    public ICommand SelectTagCommand { get; }
    public ICommand ToggleComposeTagCommand { get; }
    public ICommand SelectAreaCommand { get; }
    public ICommand ComposeCommand { get; }
    public ICommand CloseComposeDialogCommand { get; }
    public ICommand SubmitComposePreviewCommand { get; }
    public ICommand OpenNotificationCenterCommand { get; }
    public ICommand OpenMessagesCommand { get; }
    public ICommand OpenProfilePreviewCommand { get; }
    public ICommand OpenActivityQuickAddCommand { get; }
    public ICommand OpenActivityPreviewCommand { get; }
    public ICommand OpenEmergencyContactPreviewCommand { get; }
    public ICommand ToggleLikeCommand { get; }
    public ICommand OpenPostDetailCommand { get; }
    public ICommand ResetFiltersCommand { get; }
    public ICommand ClosePreviewPanelCommand { get; }
    public ICommand DismissEmergencyContactPreviewCommand { get; }
    public string ComposePrompt { get; }
    public bool IsUsingPreviewData { get; }
    public string BrandText => _brandText;
    public string SearchPlaceholder => _searchPlaceholder;
    public bool HasActivePreviewFilters => HasSearchInput || _hasAppliedTopNavFilter || _hasAppliedCategoryFilter || _hasAppliedTagFilter || _hasAppliedAreaFilter;
    public bool ShowFilterSummary => HasActivePreviewFilters;
    public bool HasOpenPreviewPanel => !string.IsNullOrWhiteSpace(OpenPreviewPanelKey);
    public bool ShowPreviewPanels => HasOpenPreviewPanel;
    public bool HasEmergencyContactPreview => SelectedEmergencyContact is not null;
    public bool ShowEmergencyContactPreview => HasEmergencyContactPreview;
    public string ComposeDialogTitle => "Đăng bài viết mới";
        public string ComposeTitleLabel => "Tiêu đề bài đăng *";
    public string ComposeTitlePlaceholder => "Nhập tiêu đề ngắn gọn súc tích...";
    public string ComposeCategoryLabel => "Danh mục *";
    public string ComposeContentLabel => "Nội dung chi tiết *";
    public string ComposeContentPlaceholder => "Bạn muốn chia sẻ điều gì với các bạn sinh viên? Hãy ghi chi tiết để mọi người dễ tương tác...";
        public string ComposeCustomTagsLabel => "Thẻ phụ (Tags)";
    public string ComposeCustomTagsPlaceholder => "ví dụ: #wifi, #passdo, #gopy (cách nhau...)";
    
    public bool IsComposeDialogOpen
    {
        get => _isComposeDialogOpen;
        private set
        {
            if (SetProperty(ref _isComposeDialogOpen, value))
            {
                OnPropertyChanged(nameof(ShowComposeDialog));
                _closeComposeDialogCommand.RaiseCanExecuteChanged();
                _submitComposePreviewCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool ShowComposeDialog => IsComposeDialogOpen;

    public string ComposeDraftText
    {
        get => _composeDraftText;
        set => SetProperty(ref _composeDraftText, value);
    }

    public string ComposeTitleText
    {
        get => _composeTitleText;
        set => SetProperty(ref _composeTitleText, value);
    }

    public string ComposeCustomTagsText
    {
        get => _composeCustomTagsText;
        set => SetProperty(ref _composeCustomTagsText, value);
    }

    public string SelectedTopNavKey
    {
        get => _selectedTopNavKey;
        private set => SetProperty(ref _selectedTopNavKey, value);
    }

    public string SelectedCategoryKey
    {
        get => _selectedCategoryKey;
        private set => SetProperty(ref _selectedCategoryKey, value);
    }

    public string SelectedTagKey
    {
        get => _selectedTagKey;
        private set => SetProperty(ref _selectedTagKey, value);
    }

    public string SelectedAreaKey
    {
        get => _selectedAreaKey;
        private set => SetProperty(ref _selectedAreaKey, value);
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                OnPropertyChanged(nameof(HasSearchInput));
                RefreshPreviewContent(
                    HasSearchInput
                        ? $"Tìm kiếm: {SearchText.Trim()}."
                        : "Tìm kiếm đã xóa.");
            }
        }
    }

    public bool HasSearchInput => !string.IsNullOrWhiteSpace(SearchText);

    public string? StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public ForumHomeEmergencyContactItem? SelectedEmergencyContact
    {
        get => _selectedEmergencyContact;
        private set
        {
            if (SetProperty(ref _selectedEmergencyContact, value))
            {
                OnPropertyChanged(nameof(HasEmergencyContactPreview));
                OnPropertyChanged(nameof(ShowEmergencyContactPreview));
                _dismissEmergencyContactPreviewCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string? OpenPreviewPanelKey
    {
        get => _openPreviewPanelKey;
        private set
        {
            if (SetProperty(ref _openPreviewPanelKey, value))
            {
                OnPropertyChanged(nameof(HasOpenPreviewPanel));
                OnPropertyChanged(nameof(ShowPreviewPanels));
                _closePreviewPanelCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool HasEmptyResults
    {
        get => _hasEmptyResults;
        private set
        {
            if (SetProperty(ref _hasEmptyResults, value))
            {
                OnPropertyChanged(nameof(ShowEmptyState));
            }
        }
    }

    public bool ShowEmptyState => HasEmptyResults;

    private void SelectTopNav(object? parameter)
    {
        if (parameter is not ForumHomeNavItem selectedItem)
        {
            return;
        }

        foreach (var item in TopNavItems)
        {
            item.IsSelected = item == selectedItem;
        }

        SelectedTopNavKey = selectedItem.Key;
        _hasAppliedTopNavFilter = true;
        RefreshPreviewContent($"Preview lane: {selectedItem.Label}.");
    }

    private void SelectCategory(object? parameter)
    {
        if (parameter is not ForumHomeNavItem selectedItem)
        {
            return;
        }

        foreach (var item in CategoryItems)
        {
            item.IsSelected = item == selectedItem;
        }

        SelectedCategoryKey = selectedItem.Key;
        _hasAppliedCategoryFilter = true;
        RefreshPreviewContent($"Category: {selectedItem.Label}.");
    }

    private void SelectTag(object? parameter)
    {
        if (parameter is not ForumHomeTagChip selectedItem)
        {
            return;
        }

        selectedItem.IsSelected = !selectedItem.IsSelected;

        _hasAppliedTagFilter = TagChips.Any(item => item.IsSelected);
        SelectedTagKey = GetSelectedTagKeys().LastOrDefault() ?? string.Empty;
        RefreshPreviewContent(selectedItem.IsSelected
            ? $"Tag: {selectedItem.Text}."
            : $"Đã bỏ chọn tag: {selectedItem.Text}.");
    }

    private static IEnumerable<ForumHomeNavItem> CreateComposeCategoryItems(IEnumerable<ForumHomeNavItem> sourceCategories)
    {
        return sourceCategories.Select(category => new ForumHomeNavItem(category.Key, category.Label, category.IconKind, category.Key == "announcements", category.CountText));
    }

    private static IEnumerable<ForumHomeTagChip> CreateComposeTagChips(IEnumerable<ForumHomeTagChip> sourceTags)
    {
        return sourceTags.Select(tag => new ForumHomeTagChip(
            tag.Key,
            tag.Text,
            tag.AccentKind,
            tag.Key is "clubs" or "quiet-study",
            tag.CountText));
    }

    private void SelectComposeCategory(object? parameter)
    {
        if (parameter is not ForumHomeNavItem selectedItem)
        {
            return;
        }

        foreach (var item in ComposeCategoryItems)
        {
            item.IsSelected = item == selectedItem;
        }

        _selectedComposeCategoryKey = selectedItem.Key;
    }

    private void ToggleComposeTag(object? parameter)
    {
        if (parameter is not ForumHomeTagChip selectedItem)
        {
            return;
        }

        selectedItem.IsSelected = !selectedItem.IsSelected;
    }

    private void SelectArea(object? parameter)
    {
        if (parameter is not ForumHomeAreaShortcut selectedItem)
        {
            return;
        }

        foreach (var item in AreaShortcuts)
        {
            item.IsSelected = item == selectedItem;
        }

        SelectedAreaKey = selectedItem.Key;
        _hasAppliedAreaFilter = true;
        RefreshPreviewContent($"Area: {selectedItem.Title}.");
    }

    private void ResetComposeDialogState()
    {
        ComposeTitleText = string.Empty;
        ComposeDraftText = string.Empty;
        ComposeCustomTagsText = string.Empty;
        _selectedComposeCategoryKey = "announcements";

        foreach (var item in ComposeCategoryItems)
        {
            item.IsSelected = string.Equals(item.Key, _selectedComposeCategoryKey, StringComparison.Ordinal);
        }

        foreach (var item in ComposeTagChips)
        {
            item.IsSelected = item.Key is "clubs" or "quiet-study";
        }
    }

    private void ComposePlaceholder()
    {
        if (IsActivityDialogOpen)
        {
            CloseActivityDialogSilently();
        }

        if (HasOpenPreviewPanel)
        {
            ClosePreviewPanel();
        }

        if (HasEmergencyContactPreview)
        {
            DismissEmergencyContactPreview(updateStatusMessage: false);
        }

        ResetComposeDialogState();
        IsComposeDialogOpen = true;
        StatusMessage = "Đã mở popup tạo bài viết.";
    }

    private void CloseComposeDialog()
    {
        IsComposeDialogOpen = false;
        ResetComposeDialogState();
        StatusMessage = "Đã hủy tạo bài viết.";
    }

    private void SubmitComposePreview()
    {
        IsComposeDialogOpen = false;
        ResetComposeDialogState();
        StatusMessage = "Bài viết đã được lưu ở chế độ xem trước.";
    }

    private void OpenNotificationPlaceholder() => OpenPreviewPanel("notifications", "Đang xem trước thông báo.");

    private void OpenMessagesPlaceholder() => OpenPreviewPanel("messages", "Đang xem trước tin nhắn.");

    private void OpenProfilePreview() => OpenPreviewPanel("profile", "Đang xem trước hồ sơ.");

    private void OpenActivityQuickAdd()
    {
        OpenActivityPopup();
    }

    private void OpenActivityPreview(object? parameter)
    {
        if (parameter is not ForumHomeActivityItem activityItem)
        {
            return;
        }

        activityItem.IsPreviewJoined = !activityItem.IsPreviewJoined;
        StatusMessage = activityItem.IsPreviewJoined
            ? $"Đã lưu hoạt động xem trước: {activityItem.Title}."
            : $"Đã đóng hoạt động xem trước: {activityItem.Title}.";
    }

    private void OpenEmergencyContactPreview(object? parameter)
    {
        if (parameter is not ForumHomeEmergencyContactItem contactItem)
        {
            return;
        }

        if (HasOpenPreviewPanel)
        {
            ClosePreviewPanel();
        }

        ClearEmergencyContactPreviewState();
        contactItem.PreviewDialState = "Sẵn sàng kết nối xem trước.";
        SelectedEmergencyContact = contactItem;
        StatusMessage = $"Đang xem trước cuộc gọi khẩn cấp: {contactItem.Title}.";
    }

    private void ToggleLike(object? parameter)
    {
        if (parameter is not ForumHomePostCard postCard)
        {
            return;
        }

        postCard.IsLiked = !postCard.IsLiked;
        StatusMessage = postCard.IsLiked ? "Post saved in preview mode." : "Preview like removed.";
    }

    private void OpenPostDetail(object? parameter)
    {
        if (parameter is not ForumHomePostCard postCard)
        {
            return;
        }

        StatusMessage = $"Đang mở chi tiết bài viết: {postCard.Title}.";
        _navigationService?.NavigateTo<ForumPostDetailViewModel>();
    }

    private void RefreshPreviewContent(string statusMessage)
    {
        var filteredCards = FilterFeedCards();
        ReplaceCollection(FeedCards, filteredCards);
        ReplaceCollection(ActivityItems, FilterActivityItems());
        ReplaceCollection(EmergencyContacts, FilterEmergencyContacts());
        HasEmptyResults = filteredCards.Count == 0;
        StatusMessage = HasEmptyResults ? "Không tìm thấy bài viết phù hợp." : statusMessage;
        OnPropertyChanged(nameof(HasActivePreviewFilters));
        OnPropertyChanged(nameof(ShowFilterSummary));
        _resetFiltersCommand.RaiseCanExecuteChanged();
    }

    private void OpenPreviewPanel(string panelKey, string statusMessage)
    {
        if (HasEmergencyContactPreview)
        {
            DismissEmergencyContactPreview(updateStatusMessage: false);
        }

        var panel = PreviewPanels.FirstOrDefault(candidate => string.Equals(candidate.PanelType, panelKey, StringComparison.Ordinal));
        if (panel is null)
        {
            return;
        }

        foreach (var candidate in PreviewPanels)
        {
            candidate.IsOpen = candidate == panel;
        }

        OpenPreviewPanelKey = panel.PanelType;
        StatusMessage = statusMessage;
    }

    private void ClosePreviewPanel()
    {
        foreach (var panel in PreviewPanels)
        {
            panel.IsOpen = false;
        }

        OpenPreviewPanelKey = null;
        StatusMessage = "Đã đóng bảng xem trước.";
    }

    private void DismissEmergencyContactPreview()
    {
        DismissEmergencyContactPreview(updateStatusMessage: true);
    }

    private void DismissEmergencyContactPreview(bool updateStatusMessage)
    {
        ClearEmergencyContactPreviewState();
        if (updateStatusMessage)
        {
            StatusMessage = "Đã đóng xem trước liên hệ khẩn cấp.";
        }
    }

    private IReadOnlyList<ForumHomePostCard> FilterFeedCards()
    {
        var cards = _allFeedCards
            .Where(MatchesTopNav)
            .Where(MatchesCategory)
            .Where(MatchesTag)
            .Where(MatchesArea)
            .Where(MatchesSearch)
            .ToList();

        return cards;
    }

    private void ResetFilters()
    {
        _hasAppliedTopNavFilter = false;
        _hasAppliedCategoryFilter = false;
        _hasAppliedTagFilter = false;
        _hasAppliedAreaFilter = false;

        ResetSelection(TopNavItems, item => item.Key == "home", (item, isSelected) => item.IsSelected = isSelected);
        ResetSelection(CategoryItems, item => item.Key == "announcements", (item, isSelected) => item.IsSelected = isSelected);
        ResetSelection(TagChips, _ => false, (item, isSelected) => item.IsSelected = isSelected);
        ResetSelection(AreaShortcuts, _ => false, (item, isSelected) => item.IsSelected = isSelected);

        SelectedTopNavKey = "home";
        SelectedCategoryKey = "announcements";
        SelectedTagKey = string.Empty;
        SelectedAreaKey = string.Empty;

        if (!string.IsNullOrEmpty(_searchText))
        {
            _searchText = string.Empty;
            OnPropertyChanged(nameof(SearchText));
            OnPropertyChanged(nameof(HasSearchInput));
        }

        if (HasOpenPreviewPanel)
        {
            ClosePreviewPanel();
        }

        if (HasEmergencyContactPreview)
        {
            DismissEmergencyContactPreview(updateStatusMessage: false);
        }

        RefreshPreviewContent("Đã xóa bộ lọc xem trước.");
    }

    private IReadOnlyList<ForumHomeActivityItem> FilterActivityItems()
    {
        if (SelectedTopNavKey == "support")
        {
            return _allActivityItems.Take(2).ToArray();
        }

        if (SelectedCategoryKey == "events" || SelectedTopNavKey == "clubs")
        {
            return _allActivityItems.Where(item => item.BadgeText is not null).Take(4).ToArray();
        }

        return _allActivityItems;
    }

    private IReadOnlyList<ForumHomeEmergencyContactItem> FilterEmergencyContacts()
    {
        if (SelectedTopNavKey == "support" || GetSelectedTagKeys().Contains("maintenance") || SelectedAreaKey == "front-desk")
        {
            return _allEmergencyContacts;
        }

        return _allEmergencyContacts.Take(3).ToArray();
    }

    private bool MatchesTopNav(ForumHomePostCard card) =>
        !_hasAppliedTopNavFilter
            || SelectedTopNavKey switch
        {
            "calendar" => card.Id is "club-rehearsal" or "move-in-guide",
            "groups" => card.Id is "club-rehearsal" or "quiet-hours",
            "campaign" => card.Id is "study-refresh" or "maintenance-slot",
            "support" => card.Id is "move-in-guide" or "maintenance-slot",
            _ => true
        };

    private bool MatchesCategory(ForumHomePostCard card) =>
        !_hasAppliedCategoryFilter
            || SelectedCategoryKey switch
        {
            "events" => card.Id is "club-rehearsal",
            "housing" => card.Id is "study-refresh" or "move-in-guide" or "maintenance-slot",
            "market" => card.Id is "club-rehearsal" or "quiet-hours",
            _ => card.Id is "study-refresh" or "quiet-hours" or "move-in-guide" or "maintenance-slot"
        };

    private bool MatchesTag(ForumHomePostCard card)
    {
        var selectedTagKeys = GetSelectedTagKeys();

        return !_hasAppliedTagFilter
            || selectedTagKeys.Count == 0
            || selectedTagKeys.Any(tagKey => MatchesTagKey(card, tagKey));
    }

    private IReadOnlyList<string> GetSelectedTagKeys() =>
        TagChips
            .Where(item => item.IsSelected)
            .Select(item => item.Key)
            .ToArray();

    private static bool MatchesTagKey(ForumHomePostCard card, string tagKey) =>
        tagKey switch
        {
            "quiet-study" => card.Id is "study-refresh" or "quiet-hours",
            "move-in" => card.Id is "move-in-guide" or "maintenance-slot",
            "maintenance" => card.Id is "maintenance-slot",
            "clubs" => card.Id is "club-rehearsal" or "quiet-hours",
            _ => true
        };

    private bool MatchesArea(ForumHomePostCard card) =>
        !_hasAppliedAreaFilter
            || SelectedAreaKey switch
        {
            "north-wing" => card.Id is "study-refresh" or "maintenance-slot",
            "front-desk" => card.Id is "quiet-hours" or "move-in-guide",
            "study-hall" => card.Id is "club-rehearsal",
            _ => true
        };

    private bool MatchesSearch(ForumHomePostCard card)
    {
        if (!HasSearchInput)
        {
            return true;
        }

        var query = SearchText.Trim();

        return ContainsQuery(card.Title, query)
               || ContainsQuery(card.Excerpt, query)
               || ContainsQuery(card.AuthorName, query)
               || card.Badges.Any(badge => ContainsQuery(badge.Text, query))
               || ContainsQuery(card.OverlayTitle, query)
               || ContainsQuery(card.OverlaySubtitle, query);
    }

    private static bool ContainsQuery(string? source, string query) =>
        !string.IsNullOrWhiteSpace(source)
        && source.Contains(query, StringComparison.CurrentCultureIgnoreCase);

    private static IReadOnlyList<ForumHomePreviewPanel> CreateDefaultPreviewPanels(ForumHomeUserSummary userSummary) =>
    [
        new ForumHomePreviewPanel(
            panelType: "messages",
            title: "Tin nhắn gần đây",
            items:
            [
                new ForumHomePreviewPanelItem(
                    key: "msg-guard",
                    title: "Nhóm trực KTX",
                    description: "Nhớ kiểm tra lại thông báo bảo trì khu B trước 22h tối nay.",
                    metaText: "5 phút trước",
                    accentTone: "primary",
                    isUnread: true),
                new ForumHomePreviewPanelItem(
                    key: "msg-club",
                    title: "CLB Tình nguyện",
                    description: "Danh sách sinh viên quan tâm đã được ghim trong bài tuyển thành viên.",
                    metaText: "30 phút trước",
                    accentTone: "tertiary")
            ],
            emptyStateText: "Không có tin nhắn xem trước."),
        new ForumHomePreviewPanel(
            panelType: "notifications",
            title: "Thông báo xem trước",
            items:
            [
                new ForumHomePreviewPanelItem(
                    key: "notif-maintenance",
                    title: "Bảo trì điện nước khu B",
                    description: "Thông báo quan trọng đã được đẩy lên đầu nguồn cấp tin của bạn.",
                    metaText: "2 giờ trước",
                    accentTone: "primary",
                    isUnread: true),
                new ForumHomePreviewPanelItem(
                    key: "notif-event",
                    title: "Đêm nhạc Acoustic gây quỹ",
                    description: "Sự kiện sinh viên mới được thêm vào danh sách hoạt động xem trước.",
                    metaText: "Hôm qua",
                    accentTone: "secondary")
            ],
            emptyStateText: "Không có thông báo xem trước."),
        new ForumHomePreviewPanel(
            panelType: "profile",
            title: "Tóm tắt hồ sơ",
            items:
            [
                new ForumHomePreviewPanelItem(
                    key: "profile-name",
                    title: userSummary.DisplayName,
                    description: userSummary.RoomLabel,
                    metaText: "Sinh viên nội trú",
                    accentTone: "primary"),
                new ForumHomePreviewPanelItem(
                    key: "profile-scope",
                    title: "Phạm vi xem trước",
                    description: "Các tương tác tại trang chủ diễn đàn hiện chỉ lưu trong phiên xem trước.",
                    metaText: "Không thay đổi dữ liệu thật",
                    accentTone: "secondary")
            ],
            emptyStateText: "Không có dữ liệu hồ sơ xem trước.")
    ];

    private static void ReplaceCollection<T>(ObservableCollection<T> target, IReadOnlyList<T> items)
    {
        target.Clear();
        foreach (var item in items)
        {
            target.Add(item);
        }
    }

    private void ClearEmergencyContactPreviewState()
    {
        foreach (var contact in EmergencyContacts)
        {
            contact.PreviewDialState = null;
        }

        SelectedEmergencyContact = null;
    }

    private static void ResetSelection<T>(IEnumerable<T> items, Func<T, bool> predicate, Action<T, bool> setSelected)
    {
        foreach (var item in items)
        {
            setSelected(item, predicate(item));
        }
    }
}









