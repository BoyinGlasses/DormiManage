using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using DormitoryManagement.Application.DTOs.Students;
using DormitoryManagement.Application.Services.Students;
using DormitoryManagement.WPF.Common;

namespace DormitoryManagement.WPF.ViewModels.Students;

public sealed class StudentProfileViewModel : ViewModelBase
{
    private const string DefaultAvatarAssetPath = "/DormitoryManagement.WPF;component/Assets/Images/Profile/resident-photo.png";

    private readonly IStudentService _studentService;
    private readonly AsyncRelayCommand _loadCommand;
    private readonly RelayCommand _editProfileCommand;
    private readonly RelayCommand _openSettingCommand;
    private bool _hasLoaded;
    private string _fullName = "Sinh viên";
    private string _studentCode = "Chưa cập nhật";
    private string _email = "Chưa cập nhật";
    private string _phoneNumber = "Chưa cập nhật";
    private string _dateOfBirth = "Chưa cập nhật";
    private string _gender = "Chưa cập nhật";
    private string _avatarImageSource = DefaultAvatarAssetPath;
    private string _profileStatusMessage = string.Empty;
    private string _buildingName = "Chưa cập nhật";
    private string _roomLabel = "Chưa cập nhật";
    private string _assignmentStatus = "Chưa phân phòng";
    private string _dormitorySupportMessage = "Bạn chưa có phòng nội trú đang hoạt động.";
    private string? _actionMessage;
    private bool _hasActiveAssignment;

    public StudentProfileViewModel(IStudentService studentService)
    {
        _studentService = studentService;
        _loadCommand = new AsyncRelayCommand(LoadAsync, () => !IsBusy);
        _editProfileCommand = new RelayCommand(() => ActionMessage = "Chỉnh sửa hồ sơ sẽ được mở trong phạm vi tính năng tiếp theo.");
        _openSettingCommand = new RelayCommand(OpenSetting);
        LoadCommand = _loadCommand;
        EditProfileCommand = _editProfileCommand;
        OpenSettingCommand = _openSettingCommand;
        AccountSettings = new ObservableCollection<StudentProfileSettingItemViewModel>
        {
            new("lock", "Đổi mật khẩu", "Cập nhật mật khẩu để bảo vệ tài khoản", "safe"),
            new("language", "Ngôn ngữ", "Chọn ngôn ngữ hiển thị", "placeholder", "Tiếng Việt")
        };
    }

    public ICommand LoadCommand { get; }
    public ICommand RefreshCommand => LoadCommand;
    public ICommand EditProfileCommand { get; }
    public ICommand OpenSettingCommand { get; }

    public ObservableCollection<StudentProfileSettingItemViewModel> AccountSettings { get; }

    public bool HasLoaded => _hasLoaded;
    public bool HasActionMessage => !string.IsNullOrWhiteSpace(ActionMessage);
    public bool HasActiveAssignment
    {
        get => _hasActiveAssignment;
        private set
        {
            if (SetProperty(ref _hasActiveAssignment, value))
            {
                OnPropertyChanged(nameof(DormitoryStatusTone));
            }
        }
    }

    public string PageTitle => "Hồ sơ cá nhân";
    public string PageSubtitle => "Quản lý thông tin cá nhân và tùy chọn tài khoản của bạn.";
    public string PersonalInformationTitle => "Thông tin cá nhân";
    public string EditProfileLabel => "Chỉnh sửa";
    public string DormitorySectionTitle => "Thông tin phòng ở";
    public string AccountSettingsTitle => "Cài đặt tài khoản";
    public string AccountSettingsSubtitle => "Tùy chỉnh trải nghiệm và bảo mật của bạn.";
    public string BuildingLabel => "Tòa nhà";
    public string RoomLabelTitle => "Phòng";
    public string StudentCodeBadgeText => $"MSSV: {StudentCode}";
    public string DormitoryStatusTone => HasActiveAssignment ? "Assigned" : "Empty";

    public string FullName
    {
        get => _fullName;
        private set => SetProperty(ref _fullName, value);
    }

    public string StudentCode
    {
        get => _studentCode;
        private set
        {
            if (SetProperty(ref _studentCode, value))
            {
                OnPropertyChanged(nameof(StudentCodeBadgeText));
            }
        }
    }

    public string Email
    {
        get => _email;
        private set => SetProperty(ref _email, value);
    }

    public string PhoneNumber
    {
        get => _phoneNumber;
        private set => SetProperty(ref _phoneNumber, value);
    }

    public string DateOfBirth
    {
        get => _dateOfBirth;
        private set => SetProperty(ref _dateOfBirth, value);
    }

    public string Gender
    {
        get => _gender;
        private set => SetProperty(ref _gender, value);
    }

    public string AvatarImageSource
    {
        get => _avatarImageSource;
        private set => SetProperty(ref _avatarImageSource, value);
    }

    public string ProfileStatusMessage
    {
        get => _profileStatusMessage;
        private set => SetProperty(ref _profileStatusMessage, value);
    }

    public string BuildingName
    {
        get => _buildingName;
        private set => SetProperty(ref _buildingName, value);
    }

    public string RoomLabel
    {
        get => _roomLabel;
        private set => SetProperty(ref _roomLabel, value);
    }

    public string AssignmentStatus
    {
        get => _assignmentStatus;
        private set => SetProperty(ref _assignmentStatus, value);
    }

    public string DormitorySupportMessage
    {
        get => _dormitorySupportMessage;
        private set => SetProperty(ref _dormitorySupportMessage, value);
    }

    public string? ActionMessage
    {
        get => _actionMessage;
        private set
        {
            if (SetProperty(ref _actionMessage, value))
            {
                OnPropertyChanged(nameof(HasActionMessage));
            }
        }
    }

    private async Task LoadAsync()
    {
        ClearError();
        ActionMessage = null;
        IsBusy = true;
        try
        {
            var profile = await _studentService.GetCurrentStudentProfileAsync();
            ApplyProfile(profile);
            _hasLoaded = true;
            OnPropertyChanged(nameof(HasLoaded));
        }
        catch (Exception ex)
        {
            SetError(ex.Message);
        }
        finally
        {
            IsBusy = false;
            _loadCommand.RaiseCanExecuteChanged();
        }
    }

    private void ApplyProfile(StudentProfileDto profile)
    {
        FullName = string.IsNullOrWhiteSpace(profile.FullName) ? "Sinh viên" : profile.FullName;
        StudentCode = GetDisplayValue(profile.StudentCode);
        Email = GetDisplayValue(profile.Email);
        PhoneNumber = GetDisplayValue(profile.PhoneNumber);
        DateOfBirth = profile.DateOfBirth.HasValue
            ? profile.DateOfBirth.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)
            : "Chưa cập nhật";
        Gender = GetDisplayValue(profile.Gender);
        AvatarImageSource = string.IsNullOrWhiteSpace(profile.AvatarAssetPath) ? DefaultAvatarAssetPath : profile.AvatarAssetPath;
        ProfileStatusMessage = GetDisplayValue(profile.ProfileStatusMessage);
        BuildingName = GetDisplayValue(profile.BuildingName);
        RoomLabel = GetDisplayValue(profile.RoomLabel);
        AssignmentStatus = GetDisplayValue(profile.AssignmentStatus);
        DormitorySupportMessage = GetDisplayValue(profile.DormitorySupportMessage);
        HasActiveAssignment = profile.HasActiveAssignment;
    }

    private void OpenSetting(object? parameter)
    {
        if (parameter is StudentProfileSettingItemViewModel item)
        {
            ActionMessage = item.InteractionMode == "safe"
                ? $"{item.Label} sẽ mở trong luồng tài khoản hiện có."
                : $"{item.Label} hiện đang ở chế độ xem trước an toàn.";
            return;
        }

        ActionMessage = "Tùy chọn này hiện đang ở chế độ xem trước an toàn.";
    }

    private static string GetDisplayValue(string? value) => string.IsNullOrWhiteSpace(value) ? "Chưa cập nhật" : value;
}

public sealed class StudentProfileSettingItemViewModel
{
    public StudentProfileSettingItemViewModel(string iconKey, string label, string supportingText, string interactionMode, string trailingValue = "")
    {
        IconKey = iconKey;
        Label = label;
        SupportingText = supportingText;
        InteractionMode = interactionMode;
        TrailingValue = trailingValue;
    }

    public string IconKey { get; }
    public string Label { get; }
    public string SupportingText { get; }
    public string InteractionMode { get; }
    public string TrailingValue { get; }
    public bool HasTrailingValue => !string.IsNullOrWhiteSpace(TrailingValue);
}
