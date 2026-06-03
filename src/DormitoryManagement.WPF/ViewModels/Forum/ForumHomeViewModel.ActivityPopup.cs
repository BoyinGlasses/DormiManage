using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using DormitoryManagement.WPF.Common;

namespace DormitoryManagement.WPF.ViewModels.Forum;

public sealed partial class ForumHomeViewModel
{
    private RelayCommand _closeActivityDialogCommand = null!;
    private RelayCommand _submitActivityPreviewCommand = null!;
    private string _selectedActivityCategoryKey = string.Empty;
    private string _activityNameText = string.Empty;
    private string _activityDateText = string.Empty;
    private string _activityTimeText = string.Empty;
    private string _activityLocationText = string.Empty;
    private string _activityDescriptionText = string.Empty;
    private string? _activityValidationSummary;
    private bool _isActivityDialogOpen;

    public ObservableCollection<ForumHomeActivityCategoryOption> ActivityCategoryOptions { get; private set; } = [];
    public ObservableCollection<ForumHomeActivityValidationMessage> ActivityValidationMessages { get; private set; } = [];
    public ICommand SelectActivityCategoryCommand { get; private set; } = null!;
    public ICommand CloseActivityDialogCommand { get; private set; } = null!;
    public ICommand SubmitActivityPreviewCommand { get; private set; } = null!;

    public string ActivityDialogTitle => "Create New Activity";
    public string ActivitySecondaryActionText => "Cancel";
    public string ActivityPrimaryActionText => "Create Activity";
    public string ActivityNameLabel => "Activity Name";
    public string ActivityNamePlaceholder => "e.g., Weekend Basketball Tournament";
    public string ActivityCategoryLabel => "Category";
    public string ActivityDateLabel => "Date";
    public string ActivityTimeLabel => "Time";
    public string ActivityLocationLabel => "Location";
    public string ActivityLocationPlaceholder => "e.g., Main Courtyard or Room 402";
    public string ActivityDescriptionLabel => "Description";
    public string ActivityDescriptionOptionalText => "Optional";
    public string ActivityDescriptionPlaceholder => "Provide details about the activity, what to bring, etc.";
    public bool CanSubmitActivityDialog => IsActivityDialogOpen;
    public bool CanCloseActivityDialog => IsActivityDialogOpen;

    public bool IsActivityDialogOpen
    {
        get => _isActivityDialogOpen;
        private set
        {
            if (SetProperty(ref _isActivityDialogOpen, value))
            {
                OnPropertyChanged(nameof(ShowActivityDialog));
                OnPropertyChanged(nameof(CanSubmitActivityDialog));
                OnPropertyChanged(nameof(CanCloseActivityDialog));
                _closeActivityDialogCommand.RaiseCanExecuteChanged();
                _submitActivityPreviewCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool ShowActivityDialog => IsActivityDialogOpen;

    public string ActivityNameText
    {
        get => _activityNameText;
        set
        {
            if (SetProperty(ref _activityNameText, value))
            {
                ClearActivityValidationMessage("name", nameof(ActivityNameError));
            }
        }
    }

    public string ActivityDateText
    {
        get => _activityDateText;
        set
        {
            if (SetProperty(ref _activityDateText, value))
            {
                ClearActivityValidationMessage("date", nameof(ActivityDateError));
            }
        }
    }

    public string ActivityTimeText
    {
        get => _activityTimeText;
        set
        {
            if (SetProperty(ref _activityTimeText, value))
            {
                ClearActivityValidationMessage("time", nameof(ActivityTimeError));
            }
        }
    }

    public string ActivityLocationText
    {
        get => _activityLocationText;
        set
        {
            if (SetProperty(ref _activityLocationText, value))
            {
                ClearActivityValidationMessage("location", nameof(ActivityLocationError));
            }
        }
    }

    public string ActivityDescriptionText
    {
        get => _activityDescriptionText;
        set => SetProperty(ref _activityDescriptionText, value);
    }

    public string? ActivityValidationSummary
    {
        get => _activityValidationSummary;
        private set => SetProperty(ref _activityValidationSummary, value);
    }

    public string? ActivityNameError => GetActivityValidationMessage("name");
    public string? ActivityCategoryError => GetActivityValidationMessage("category");
    public string? ActivityDateError => GetActivityValidationMessage("date");
    public string? ActivityTimeError => GetActivityValidationMessage("time");
    public string? ActivityLocationError => GetActivityValidationMessage("location");

    private void InitializeActivityDialogState()
    {
        ActivityCategoryOptions = new ObservableCollection<ForumHomeActivityCategoryOption>(ForumHomePreviewFactory.CreateActivityCategoryOptions());
        ActivityValidationMessages = new ObservableCollection<ForumHomeActivityValidationMessage>(
        [
            new ForumHomeActivityValidationMessage("name"),
            new ForumHomeActivityValidationMessage("category"),
            new ForumHomeActivityValidationMessage("date"),
            new ForumHomeActivityValidationMessage("time"),
            new ForumHomeActivityValidationMessage("location")
        ]);

        SelectActivityCategoryCommand = new RelayCommand(SelectActivityCategory, parameter => parameter is ForumHomeActivityCategoryOption);
        CloseActivityDialogCommand = _closeActivityDialogCommand = new RelayCommand(CloseActivityDialog, () => IsActivityDialogOpen);
        SubmitActivityPreviewCommand = _submitActivityPreviewCommand = new RelayCommand(SubmitActivityPreview, () => IsActivityDialogOpen);
        ResetActivityDialogState();
    }

    private void OpenActivityPopup()
    {
        if (HasOpenPreviewPanel)
        {
            ClosePreviewPanel();
        }

        if (HasEmergencyContactPreview)
        {
            DismissEmergencyContactPreview(updateStatusMessage: false);
        }

        if (IsComposeDialogOpen)
        {
            IsComposeDialogOpen = false;
            ResetComposeDialogState();
        }

        ResetActivityDialogState();
        IsActivityDialogOpen = true;
        StatusMessage = "Opened activity popup in preview mode.";
    }

    private void CloseActivityDialog()
    {
        DismissActivityPopup("Activity creation cancelled.");
    }

    private void CloseActivityDialogSilently()
    {
        DismissActivityPopup(statusMessage: null);
    }

    private void SubmitActivityPreview()
    {
        ClearActivityValidation();

        var hasErrors = false;

        if (string.IsNullOrWhiteSpace(ActivityNameText))
        {
            SetActivityValidationMessage("name", "Activity name is required.", nameof(ActivityNameError));
            hasErrors = true;
        }

        if (string.IsNullOrWhiteSpace(_selectedActivityCategoryKey))
        {
            SetActivityValidationMessage("category", "Please choose one category.", nameof(ActivityCategoryError));
            hasErrors = true;
        }

        if (string.IsNullOrWhiteSpace(ActivityDateText))
        {
            SetActivityValidationMessage("date", "Date is required.", nameof(ActivityDateError));
            hasErrors = true;
        }
        else if (!DateOnly.TryParseExact(ActivityDateText, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
        {
            SetActivityValidationMessage("date", "Use YYYY-MM-DD for the date.", nameof(ActivityDateError));
            hasErrors = true;
        }

        if (string.IsNullOrWhiteSpace(ActivityTimeText))
        {
            SetActivityValidationMessage("time", "Time is required.", nameof(ActivityTimeError));
            hasErrors = true;
        }
        else if (!TimeOnly.TryParseExact(ActivityTimeText, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
        {
            SetActivityValidationMessage("time", "Use HH:MM for the time.", nameof(ActivityTimeError));
            hasErrors = true;
        }

        if (string.IsNullOrWhiteSpace(ActivityLocationText))
        {
            SetActivityValidationMessage("location", "Location is required.", nameof(ActivityLocationError));
            hasErrors = true;
        }

        if (hasErrors)
        {
            ActivityValidationSummary = "Please complete the required activity details.";
            StatusMessage = ActivityValidationSummary;
            return;
        }

        DismissActivityPopup("Activity saved in preview mode.");
    }

    private void SelectActivityCategory(object? parameter)
    {
        if (parameter is not ForumHomeActivityCategoryOption selectedItem)
        {
            return;
        }

        foreach (var item in ActivityCategoryOptions)
        {
            item.IsSelected = item == selectedItem;
        }

        _selectedActivityCategoryKey = selectedItem.Key;
        ClearActivityValidationMessage("category", nameof(ActivityCategoryError));
    }

    private void ResetActivityDialogState()
    {
        _selectedActivityCategoryKey = ActivityCategoryOptions.FirstOrDefault(item => item.IsSelected)?.Key ?? string.Empty;
        ActivityNameText = string.Empty;
        ActivityDateText = string.Empty;
        ActivityTimeText = string.Empty;
        ActivityLocationText = string.Empty;
        ActivityDescriptionText = string.Empty;
        foreach (var option in ActivityCategoryOptions)
        {
            option.IsSelected = false;
        }

        ClearActivityValidation();
    }

    private void DismissActivityPopup(string? statusMessage)
    {
        IsActivityDialogOpen = false;
        ResetActivityDialogState();
        if (statusMessage is not null)
        {
            StatusMessage = statusMessage;
        }
    }

    private void ClearActivityValidation()
    {
        ActivityValidationSummary = null;
        foreach (var message in ActivityValidationMessages)
        {
            message.Message = null;
        }

        OnPropertyChanged(nameof(ActivityNameError));
        OnPropertyChanged(nameof(ActivityCategoryError));
        OnPropertyChanged(nameof(ActivityDateError));
        OnPropertyChanged(nameof(ActivityTimeError));
        OnPropertyChanged(nameof(ActivityLocationError));
    }

    private void ClearActivityValidationMessage(string key, string propertyName)
    {
        var message = ActivityValidationMessages.FirstOrDefault(item => string.Equals(item.Key, key, StringComparison.Ordinal));
        if (message is null || string.IsNullOrWhiteSpace(message.Message))
        {
            return;
        }

        message.Message = null;
        OnPropertyChanged(propertyName);
        if (ActivityValidationMessages.All(item => string.IsNullOrWhiteSpace(item.Message)))
        {
            ActivityValidationSummary = null;
        }
    }

    private void SetActivityValidationMessage(string key, string message, string propertyName)
    {
        var validation = ActivityValidationMessages.First(item => string.Equals(item.Key, key, StringComparison.Ordinal));
        validation.Message = message;
        OnPropertyChanged(propertyName);
    }

    private string? GetActivityValidationMessage(string key) =>
        ActivityValidationMessages.FirstOrDefault(item => string.Equals(item.Key, key, StringComparison.Ordinal))?.Message;
}



