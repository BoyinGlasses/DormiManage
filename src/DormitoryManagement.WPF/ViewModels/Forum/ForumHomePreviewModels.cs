using DormitoryManagement.WPF.Common;

namespace DormitoryManagement.WPF.ViewModels.Forum;

public sealed class ForumHomePreviewState
{
    public ForumHomePreviewState(
        ForumHomeUserSummary headerUserSummary,
        IEnumerable<ForumHomeNavItem> topNavItems,
        string selectedTopNavKey,
        IEnumerable<ForumHomeNavItem> categoryItems,
        string selectedCategoryKey,
        IEnumerable<ForumHomeTagChip> popularTags,
        string selectedTagKey,
        IEnumerable<ForumHomeAreaShortcut> areaShortcuts,
        string selectedAreaKey,
        string searchText,
        string composePrompt,
        IEnumerable<ForumHomePostCard> postCards,
        IEnumerable<ForumHomeActivityItem> activityItems,
        IEnumerable<ForumHomeEmergencyContactItem> emergencyContacts,
        bool isUsingPreviewData,
        string? statusMessage = null,
        string brandText = "DMForum",
        string searchPlaceholder = "",
        IEnumerable<ForumHomePreviewPanel>? previewPanels = null,
        string? openPreviewPanelKey = null,
        IEnumerable<ForumHomeFidelityException>? fidelityExceptions = null)
    {
        HeaderUserSummary = headerUserSummary;
        TopNavItems = topNavItems.ToArray();
        SelectedTopNavKey = selectedTopNavKey;
        CategoryItems = categoryItems.ToArray();
        SelectedCategoryKey = selectedCategoryKey;
        PopularTags = popularTags.ToArray();
        SelectedTagKey = selectedTagKey;
        AreaShortcuts = areaShortcuts.ToArray();
        SelectedAreaKey = selectedAreaKey;
        SearchText = searchText;
        ComposePrompt = composePrompt;
        PostCards = postCards.ToArray();
        ActivityItems = activityItems.ToArray();
        EmergencyContacts = emergencyContacts.ToArray();
        IsUsingPreviewData = isUsingPreviewData;
        StatusMessage = statusMessage;
        BrandText = brandText;
        SearchPlaceholder = searchPlaceholder;
        PreviewPanels = previewPanels?.ToArray() ?? [];
        OpenPreviewPanelKey = openPreviewPanelKey;
        FidelityExceptions = fidelityExceptions?.ToArray() ?? [];
    }

    public ForumHomeUserSummary HeaderUserSummary { get; }
    public IReadOnlyList<ForumHomeNavItem> TopNavItems { get; }
    public string SelectedTopNavKey { get; }
    public IReadOnlyList<ForumHomeNavItem> CategoryItems { get; }
    public string SelectedCategoryKey { get; }
    public IReadOnlyList<ForumHomeTagChip> PopularTags { get; }
    public string SelectedTagKey { get; }
    public IReadOnlyList<ForumHomeAreaShortcut> AreaShortcuts { get; }
    public string SelectedAreaKey { get; }
    public string SearchText { get; }
    public string ComposePrompt { get; }
    public IReadOnlyList<ForumHomePostCard> PostCards { get; }
    public IReadOnlyList<ForumHomeActivityItem> ActivityItems { get; }
    public IReadOnlyList<ForumHomeEmergencyContactItem> EmergencyContacts { get; }
    public bool IsUsingPreviewData { get; }
    public bool HasSearchInput => !string.IsNullOrWhiteSpace(SearchText);
    public string? StatusMessage { get; }
    public string BrandText { get; }
    public string SearchPlaceholder { get; }
    public IReadOnlyList<ForumHomePreviewPanel> PreviewPanels { get; }
    public string? OpenPreviewPanelKey { get; }
    public bool HasOpenPreviewPanel => !string.IsNullOrWhiteSpace(OpenPreviewPanelKey);
    public bool ShowPreviewPanels => HasOpenPreviewPanel;
    public IReadOnlyList<ForumHomeFidelityException> FidelityExceptions { get; }
}

public sealed class ForumHomeUserSummary
{
    public ForumHomeUserSummary(string displayName, string subtitle, string avatarLabel, string roomLabel, bool showChevron, string? avatarAssetPath = null)
    {
        DisplayName = displayName;
        Subtitle = subtitle;
        AvatarLabel = avatarLabel;
        RoomLabel = roomLabel;
        ShowChevron = showChevron;
        AvatarAssetPath = avatarAssetPath;
    }

    public string DisplayName { get; }
    public string Subtitle { get; }
    public string AvatarLabel { get; }
    public string RoomLabel { get; }
    public bool ShowChevron { get; }
    public string? AvatarAssetPath { get; }
}

public sealed class ForumHomeNavItem : ObservableObject
{
    private bool _isSelected;

    public ForumHomeNavItem(string key, string label, string iconKind, bool isSelected = false, string? countText = null)
    {
        Key = key;
        Label = label;
        IconKind = iconKind;
        _isSelected = isSelected;
        CountText = countText;
    }

    public string Key { get; }
    public string Label { get; }
    public string IconKind { get; }
    public string? CountText { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}

public sealed class ForumHomeTagChip : ObservableObject
{
    private bool _isSelected;

    public ForumHomeTagChip(string key, string text, string accentKind, bool isSelected = false, string? countText = null)
    {
        Key = key;
        Text = text;
        AccentKind = accentKind;
        _isSelected = isSelected;
        CountText = countText;
    }

    public string Key { get; }
    public string Text { get; }
    public string AccentKind { get; }
    public string? CountText { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}

public sealed class ForumHomeAreaShortcut : ObservableObject
{
    private bool _isSelected;

    public ForumHomeAreaShortcut(
        string key,
        string badgeText,
        string title,
        string iconKind,
        string? subtitle = null,
        bool isSelected = false,
        string? countText = null)
    {
        Key = key;
        BadgeText = badgeText;
        Title = title;
        IconKind = iconKind;
        Subtitle = subtitle;
        _isSelected = isSelected;
        CountText = countText;
    }

    public string Key { get; }
    public string BadgeText { get; }
    public string Title { get; }
    public string IconKind { get; }
    public string? Subtitle { get; }
    public string? CountText { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}

public sealed class ForumHomePostCard : ObservableObject
{
    private bool _isLiked;
    private bool _isPreviewExpanded;

    public ForumHomePostCard(
        string id,
        string? coverAssetPath,
        bool useFallbackVisual,
        string? overlayTitle,
        string? overlaySubtitle,
        string? priorityLabel,
        IEnumerable<ForumHomeBadge> badges,
        string title,
        string excerpt,
        string authorName,
        string authorAvatarLabel,
        string authorSubtitle,
        string relativeTimeText,
        string viewCountText,
        string likeCountText,
        string commentCountText,
        string? fallbackBadgeText = null,
        string? fallbackAccentTone = null,
        bool isLiked = false,
        string? areaLabelText = null,
        string? areaOverlayEmphasis = null,
        string? authorRoleText = null,
        bool isImportant = false,
        bool isPreviewExpanded = false,
        string? fallbackVisualKey = null)
        : this(
            id,
            coverAssetPath,
            useFallbackVisual,
            overlayTitle,
            overlaySubtitle,
            priorityLabel,
            badges,
            title,
            excerpt,
            authorName,
            authorAvatarAssetPath: null,
            authorAvatarLabel,
            authorSubtitle,
            relativeTimeText,
            viewCountText,
            likeCountText,
            commentCountText,
            fallbackBadgeText,
            fallbackAccentTone,
            isLiked,
            areaLabelText,
            areaOverlayEmphasis,
            authorRoleText,
            isImportant,
            isPreviewExpanded,
            fallbackVisualKey)
    {
    }

    public ForumHomePostCard(
        string id,
        string? coverAssetPath,
        bool useFallbackVisual,
        string? overlayTitle,
        string? overlaySubtitle,
        string? priorityLabel,
        IEnumerable<ForumHomeBadge> badges,
        string title,
        string excerpt,
        string authorName,
        string? authorAvatarAssetPath,
        string authorAvatarLabel,
        string authorSubtitle,
        string relativeTimeText,
        string viewCountText,
        string likeCountText,
        string commentCountText,
        string? fallbackBadgeText = null,
        string? fallbackAccentTone = null,
        bool isLiked = false,
        string? areaLabelText = null,
        string? areaOverlayEmphasis = null,
        string? authorRoleText = null,
        bool isImportant = false,
        bool isPreviewExpanded = false,
        string? fallbackVisualKey = null)
    {
        Id = id;
        CoverAssetPath = coverAssetPath;
        UseFallbackVisual = useFallbackVisual;
        OverlayTitle = overlayTitle;
        OverlaySubtitle = overlaySubtitle;
        PriorityLabel = priorityLabel;
        Badges = badges.ToArray();
        Title = title;
        Excerpt = excerpt;
        AuthorName = authorName;
        AuthorAvatarAssetPath = authorAvatarAssetPath;
        AuthorAvatarLabel = authorAvatarLabel;
        AuthorSubtitle = authorSubtitle;
        RelativeTimeText = relativeTimeText;
        ViewCountText = viewCountText;
        LikeCountText = likeCountText;
        CommentCountText = commentCountText;
        FallbackBadgeText = fallbackBadgeText;
        FallbackAccentTone = fallbackAccentTone;
        _isLiked = isLiked;
        AreaLabelText = areaLabelText;
        AreaOverlayEmphasis = areaOverlayEmphasis;
        AuthorRoleText = authorRoleText ?? authorSubtitle;
        IsImportant = isImportant;
        _isPreviewExpanded = isPreviewExpanded;
        FallbackVisualKey = fallbackVisualKey;
    }

    public string Id { get; }
    public string? CoverAssetPath { get; }
    public string? ImageAssetPath => CoverAssetPath;
    public bool UseFallbackVisual { get; }
    public string? OverlayTitle { get; }
    public string? OverlaySubtitle { get; }
    public string? PriorityLabel { get; }
    public IReadOnlyList<ForumHomeBadge> Badges { get; }
    public string Title { get; }
    public string Excerpt { get; }
    public string AuthorName { get; }
    public string? AuthorAvatarAssetPath { get; }
    public string AuthorAvatarLabel { get; }
    public string AuthorSubtitle { get; }
    public string AuthorRoleText { get; }
    public string RelativeTimeText { get; }
    public string ViewCountText { get; }
    public string LikeCountText { get; }
    public string CommentCountText { get; }
    public string? FallbackBadgeText { get; }
    public string? FallbackAccentTone { get; }
    public string? FallbackVisualKey { get; }
    public string? AreaLabelText { get; }
    public string? AreaOverlayEmphasis { get; }
    public bool IsImportant { get; }
    public bool ShowAreaOverlay => !string.IsNullOrWhiteSpace(OverlayTitle) || !string.IsNullOrWhiteSpace(OverlaySubtitle);

    public bool IsLiked
    {
        get => _isLiked;
        set => SetProperty(ref _isLiked, value);
    }

    public bool IsPreviewExpanded
    {
        get => _isPreviewExpanded;
        set => SetProperty(ref _isPreviewExpanded, value);
    }
}

public sealed class ForumHomeBadge
{
    public ForumHomeBadge(string text, string tone)
    {
        Text = text;
        Tone = tone;
    }

    public string Text { get; }
    public string Tone { get; }
}

public sealed class ForumHomeActivityCategoryOption : ObservableObject
{
    private bool _isSelected;

    public ForumHomeActivityCategoryOption(string key, string label, string iconKind, string accentTone, bool isSelected = false)
    {
        Key = key;
        Label = label;
        IconKind = iconKind;
        AccentTone = accentTone;
        _isSelected = isSelected;
    }

    public string Key { get; }
    public string Label { get; }
    public string IconKind { get; }
    public string AccentTone { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}

public sealed class ForumHomeActivityValidationMessage : ObservableObject
{
    private string? _message;

    public ForumHomeActivityValidationMessage(string key, string? message = null)
    {
        Key = key;
        _message = message;
    }

    public string Key { get; }

    public string? Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    public bool HasError => !string.IsNullOrWhiteSpace(Message);
}
public sealed class ForumHomeActivityItem : ObservableObject
{
    private bool _isPreviewJoined;

    public ForumHomeActivityItem(
        string id,
        string monthText,
        string dayText,
        string title,
        string locationText,
        string accentTone,
        string? participantText = null,
        bool isPreviewJoined = false,
        string? actionLabel = null,
        string? badgeText = null)
    {
        Id = id;
        MonthText = monthText;
        DayText = dayText;
        Title = title;
        LocationText = locationText;
        AccentTone = accentTone;
        ParticipantText = participantText;
        _isPreviewJoined = isPreviewJoined;
        ActionLabel = actionLabel;
        BadgeText = badgeText;
    }

    public ForumHomeActivityItem(string monthText, string dayText, string title, string subtitle, string accentTone, string? badgeText = null)
        : this(
            id: title,
            monthText: monthText,
            dayText: dayText,
            title: title,
            locationText: subtitle,
            accentTone: accentTone,
            participantText: badgeText,
            isPreviewJoined: false,
            actionLabel: null,
            badgeText: badgeText)
    {
    }

    public string Id { get; }
    public string MonthText { get; }
    public string DayText { get; }
    public string Title { get; }
    public string LocationText { get; }
    public string Subtitle => LocationText;
    public string AccentTone { get; }
    public string? ParticipantText { get; }
    public string? ActionLabel { get; }
    public string? BadgeText { get; }

    public bool IsPreviewJoined
    {
        get => _isPreviewJoined;
        set => SetProperty(ref _isPreviewJoined, value);
    }
}

public sealed class ForumHomeEmergencyContactItem : ObservableObject
{
    private string? _previewDialState;

    public ForumHomeEmergencyContactItem(
        string id,
        string name,
        string description,
        string phoneText,
        string accentTone,
        string iconKind,
        string? availabilityText = null,
        string? previewDialState = null,
        string? actionLabel = null)
    {
        Id = id;
        Name = name;
        Description = description;
        PhoneText = phoneText;
        AccentTone = accentTone;
        IconKind = iconKind;
        AvailabilityText = availabilityText;
        _previewDialState = previewDialState;
        ActionLabel = actionLabel;
    }

    public ForumHomeEmergencyContactItem(string title, string description, string contactText, string accentTone, string iconKind, string? availabilityText = null)
        : this(
            id: title,
            name: title,
            description: description,
            phoneText: contactText,
            accentTone: accentTone,
            iconKind: iconKind,
            availabilityText: availabilityText)
    {
    }

    public string Id { get; }
    public string Name { get; }
    public string Title => Name;
    public string Description { get; }
    public string PhoneText { get; }
    public string ContactText => PhoneText;
    public string AccentTone { get; }
    public string IconKind { get; }
    public string? AvailabilityText { get; }
    public string? ActionLabel { get; }

    public string? PreviewDialState
    {
        get => _previewDialState;
        set => SetProperty(ref _previewDialState, value);
    }
}

public sealed class ForumHomePreviewPanel : ObservableObject
{
    private bool _isOpen;

    public ForumHomePreviewPanel(
        string panelType,
        string title,
        IEnumerable<ForumHomePreviewPanelItem> items,
        bool isOpen = false,
        string? emptyStateText = null)
    {
        PanelType = panelType;
        Title = title;
        Items = items.ToArray();
        _isOpen = isOpen;
        EmptyStateText = emptyStateText;
    }

    public string PanelType { get; }
    public string Key => PanelType;
    public string Title { get; }
    public IReadOnlyList<ForumHomePreviewPanelItem> Items { get; }
    public string? EmptyStateText { get; }
    public bool HasItems => Items.Count > 0;

    public bool IsOpen
    {
        get => _isOpen;
        set => SetProperty(ref _isOpen, value);
    }
}

public sealed class ForumHomePreviewPanelItem
{
    public ForumHomePreviewPanelItem(
        string key,
        string title,
        string description,
        string? metaText = null,
        string? accentTone = null,
        bool isUnread = false)
    {
        Key = key;
        Title = title;
        Description = description;
        MetaText = metaText;
        AccentTone = accentTone;
        IsUnread = isUnread;
    }

    public string Key { get; }
    public string Title { get; }
    public string Description { get; }
    public string? MetaText { get; }
    public string? AccentTone { get; }
    public bool IsUnread { get; }
}

public sealed class ForumHomeFidelityException
{
    public ForumHomeFidelityException(
        string area,
        string referenceBehavior,
        string deliveredBehavior,
        string rationale,
        string reviewerStatus)
    {
        Area = area;
        ReferenceBehavior = referenceBehavior;
        DeliveredBehavior = deliveredBehavior;
        Rationale = rationale;
        ReviewerStatus = reviewerStatus;
    }

    public string Area { get; }
    public string ReferenceBehavior { get; }
    public string DeliveredBehavior { get; }
    public string Rationale { get; }
    public string ReviewerStatus { get; }
}

