using System.Globalization;
using System.Windows.Input;
using DormitoryManagement.Application.Abstractions.Auth;
using DormitoryManagement.Application.DTOs.Dashboard;
using DormitoryManagement.Application.Services.Dashboard;
using DormitoryManagement.WPF.Common;
using DormitoryManagement.WPF.Navigation;
using DormitoryManagement.WPF.ViewModels.Billing;
using DormitoryManagement.WPF.ViewModels.Forum;
using DormitoryManagement.WPF.ViewModels.Profile;
using DormitoryManagement.WPF.ViewModels.Registrations;
using DormitoryManagement.WPF.ViewModels.SupportTickets;
using Microsoft.Extensions.DependencyInjection;

namespace DormitoryManagement.WPF.ViewModels.Dashboard;

public sealed class StudentDashboardViewModel : ViewModelBase
{
    private static readonly CultureInfo VietnameseCulture = new("vi-VN");
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ICurrentUserService _currentUser;
    private readonly INavigationService _navigationService;
    private readonly AsyncRelayCommand _openRoomRegistrationPopupCommand;
    private readonly RelayCommand _closeRoomRegistrationPopupCommand;
    private string? _currentRoom;
    private string? _requestedRoom;
    private string _roomCardDisplayMode = "Empty";
    private string _roomCardStatusText = "Chưa phân phòng";
    private bool _canOpenRoomRegistrationPopup = true;
    private string? _roomCardLockReason;
    private decimal _outstandingDebt;
    private int _openTickets;
    private int _unreadNotifications;
    private bool _isRoomRegistrationPopupOpen;
    private IServiceScope? _roomRegistrationScope;
    private RoomRegistrationViewModel? _roomRegistrationModal;

    public StudentDashboardViewModel(IServiceScopeFactory scopeFactory, ICurrentUserService currentUser, INavigationService navigationService)
    {
        _scopeFactory = scopeFactory;
        _currentUser = currentUser;
        _navigationService = navigationService;
        RefreshCommand = new AsyncRelayCommand(LoadAsync);
        _openRoomRegistrationPopupCommand = new AsyncRelayCommand(OpenRoomRegistrationPopupAsync, () => CanOpenRoomRegistrationPopup && !IsRoomRegistrationPopupOpen);
        OpenRoomRegistrationPopupCommand = _openRoomRegistrationPopupCommand;
        _closeRoomRegistrationPopupCommand = new RelayCommand(CloseRoomRegistrationPopup, () => IsRoomRegistrationPopupOpen);
        CloseRoomRegistrationPopupCommand = _closeRoomRegistrationPopupCommand;
        RegisterRoomCommand = new RelayCommand(() => _navigationService.NavigateTo<RoomRegistrationViewModel>());
        PayInvoiceCommand = new RelayCommand(() => _navigationService.NavigateTo<PaymentViewModel>());
        CreateTicketCommand = new RelayCommand(() => _navigationService.NavigateTo<SupportTicketListViewModel>());
        OpenForumCommand = new RelayCommand(() => _navigationService.NavigateTo<ForumHomeViewModel>());
        OpenProfileCommand = new RelayCommand(() => _navigationService.NavigateTo<ProfileViewModel>());
    }

    public ICommand RefreshCommand { get; }
    public ICommand OpenRoomRegistrationPopupCommand { get; }
    public ICommand CloseRoomRegistrationPopupCommand { get; }
    public ICommand RegisterRoomCommand { get; }
    public ICommand PayInvoiceCommand { get; }
    public ICommand CreateTicketCommand { get; }
    public ICommand OpenForumCommand { get; }
    public ICommand OpenProfileCommand { get; }

    public string? CurrentRoom
    {
        get => _currentRoom;
        private set
        {
            if (SetProperty(ref _currentRoom, value))
            {
                NotifyDashboardPresentationChanged();
            }
        }
    }

    public string? RequestedRoom
    {
        get => _requestedRoom;
        private set
        {
            if (SetProperty(ref _requestedRoom, value))
            {
                NotifyDashboardPresentationChanged();
            }
        }
    }

    public string RoomCardDisplayMode
    {
        get => _roomCardDisplayMode;
        private set
        {
            if (SetProperty(ref _roomCardDisplayMode, value))
            {
                NotifyDashboardPresentationChanged();
            }
        }
    }

    public string RoomCardStatusText
    {
        get => _roomCardStatusText;
        private set
        {
            if (SetProperty(ref _roomCardStatusText, value))
            {
                NotifyDashboardPresentationChanged();
            }
        }
    }

    public bool CanOpenRoomRegistrationPopup
    {
        get => _canOpenRoomRegistrationPopup;
        private set
        {
            if (SetProperty(ref _canOpenRoomRegistrationPopup, value))
            {
                NotifyDashboardPresentationChanged();
                _openRoomRegistrationPopupCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string? RoomCardLockReason
    {
        get => _roomCardLockReason;
        private set
        {
            if (SetProperty(ref _roomCardLockReason, value))
            {
                NotifyDashboardPresentationChanged();
            }
        }
    }

    public decimal OutstandingDebt
    {
        get => _outstandingDebt;
        private set
        {
            if (SetProperty(ref _outstandingDebt, value))
            {
                NotifyDashboardPresentationChanged();
            }
        }
    }

    public int OpenTickets
    {
        get => _openTickets;
        private set
        {
            if (SetProperty(ref _openTickets, value))
            {
                NotifyDashboardPresentationChanged();
            }
        }
    }

    public int UnreadNotifications
    {
        get => _unreadNotifications;
        private set
        {
            if (SetProperty(ref _unreadNotifications, value))
            {
                NotifyDashboardPresentationChanged();
            }
        }
    }

    public bool IsRoomRegistrationPopupOpen
    {
        get => _isRoomRegistrationPopupOpen;
        private set
        {
            if (SetProperty(ref _isRoomRegistrationPopupOpen, value))
            {
                NotifyDashboardPresentationChanged();
                _openRoomRegistrationPopupCommand.RaiseCanExecuteChanged();
                _closeRoomRegistrationPopupCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public RoomRegistrationViewModel? RoomRegistrationModal
    {
        get => _roomRegistrationModal;
        private set
        {
            if (SetProperty(ref _roomRegistrationModal, value))
            {
                NotifyDashboardPresentationChanged();
            }
        }
    }

    public string StudentDisplayName => string.IsNullOrWhiteSpace(_currentUser.FullName)
        ? _currentUser.UserName ?? "Sinh viên"
        : _currentUser.FullName!;

    public string GreetingHeadline => $"Chào buổi sáng, {StudentDisplayName}";
    public string GreetingSupportingText => "Tổng quan hoạt động và thông tin phòng của bạn.";
    public string RefreshButtonText => "Làm mới";

    public string RoomCardTitle => "Phòng của tôi";
    public string RoomDisplayText => RoomCardDisplayMode == "Pending"
        ? RequestedRoom ?? "Chưa có phòng"
        : string.IsNullOrWhiteSpace(CurrentRoom)
            ? "Chưa có phòng"
            : CurrentRoom!;
    public string RoomFooterLabel => "Trạng thái";
    public string RoomFooterText => RoomCardStatusText;
    public bool IsRoomCardInteractive => CanOpenRoomRegistrationPopup;
    public bool ShowRoomCardHint => CanOpenRoomRegistrationPopup;
    public string RoomCardHintText => CanOpenRoomRegistrationPopup ? "Nhấn để đăng ký phòng" : (RoomCardLockReason ?? string.Empty);
    public bool HasPendingRoomRequest => RoomCardDisplayMode == "Pending";
    public bool HasAssignedRoom => RoomCardDisplayMode == "Assigned";

    public string InvoiceCardTitle => "Hóa đơn hiện tại";
    public string InvoiceStatusLabel => "Trạng thái:";
    public string CurrentInvoiceAmountText => OutstandingDebt.ToString("N0", VietnameseCulture);
    public string CurrentInvoiceUnitText => "VND";
    public string CurrentInvoiceStatusText => OutstandingDebt > 0 ? "Chưa thanh toán" : "Đã thanh toán";

    public string TicketCardTitle => "Yêu cầu hỗ trợ";
    public string SupportTicketCountText => OpenTickets.ToString("N0", VietnameseCulture);
    public string SupportTicketStateText => OpenTickets > 0 ? "đang mở" : "không có yêu cầu";
    public string SupportTicketActionText => "Xem chi tiết";

    public string VehicleCardTitle => "Đăng ký xe";
    public string VehicleStatusLabel => "Trạng thái:";
    public string VehicleDisplayName => "Chưa có xe";
    public string VehiclePlateText => "Đăng ký để dùng bãi giữ xe";
    public string VehicleStatusText => "Chưa đăng ký";

    public string QuickActionsSectionTitle => "Tiện ích nhanh";
    public string RoomRegistrationPopupTitle => "Đăng ký phòng";
    public string RoomRegistrationPopupSubtitle => "Popup Đăng ký phòng - KTX Lumina";

    public IReadOnlyList<string> SummaryCardTitles =>
    [
        RoomCardTitle,
        InvoiceCardTitle,
        TicketCardTitle,
        VehicleCardTitle
    ];

    public IReadOnlyList<string> QuickActionLabels =>
    [
        "Thanh toán",
        "Báo hỏng",
        "Forum",
        "Lịch"
    ];

    private async Task LoadAsync()
    {
        IsBusy = true;
        ClearError();
        try
        {
            var studentId = _currentUser.CurrentUser?.StudentId ?? Guid.Empty;
            using var scope = _scopeFactory.CreateScope();
            var dashboardService = scope.ServiceProvider.GetRequiredService<IDashboardService>();
            var dashboard = await dashboardService.GetStudentDashboardAsync(studentId);
            ApplyDashboard(dashboard);
        }
        catch (Exception ex)
        {
            SetError(ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task OpenRoomRegistrationPopupAsync()
    {
        if (!CanOpenRoomRegistrationPopup)
        {
            return;
        }

        CloseRoomRegistrationPopup();
        _roomRegistrationScope = _scopeFactory.CreateScope();
        var roomRegistrationViewModel = _roomRegistrationScope.ServiceProvider.GetRequiredService<RoomRegistrationViewModel>();
        roomRegistrationViewModel.SetExternalLockMessage(null);
        roomRegistrationViewModel.RegistrationSubmitted += OnRoomRegistrationSubmitted;
        RoomRegistrationModal = roomRegistrationViewModel;
        IsRoomRegistrationPopupOpen = true;

        if (roomRegistrationViewModel.LoadCommand.CanExecute(null))
        {
            roomRegistrationViewModel.LoadCommand.Execute(null);
        }

        await Task.CompletedTask;
    }

    private async void OnRoomRegistrationSubmitted(object? sender, EventArgs e)
    {
        try
        {
            await LoadAsync();
            CloseRoomRegistrationPopup();
        }
        catch (Exception ex)
        {
            SetError(ex.Message);
        }
    }

    private void CloseRoomRegistrationPopup()
    {
        if (RoomRegistrationModal is not null)
        {
            RoomRegistrationModal.RegistrationSubmitted -= OnRoomRegistrationSubmitted;
        }

        RoomRegistrationModal = null;
        _roomRegistrationScope?.Dispose();
        _roomRegistrationScope = null;
        IsRoomRegistrationPopupOpen = false;
    }

    private void ApplyDashboard(StudentDashboardDto dashboard)
    {
        CurrentRoom = dashboard.CurrentRoom;
        RequestedRoom = dashboard.RequestedRoom;
        RoomCardDisplayMode = dashboard.RoomCardDisplayMode;
        RoomCardStatusText = dashboard.RoomCardStatusText;
        CanOpenRoomRegistrationPopup = dashboard.CanOpenRoomRegistrationPopup;
        RoomCardLockReason = dashboard.RoomCardLockReason;
        OutstandingDebt = dashboard.OutstandingDebt;
        OpenTickets = dashboard.OpenTickets;
        UnreadNotifications = dashboard.UnreadNotifications;

        if (IsRoomRegistrationPopupOpen && RoomRegistrationModal is not null && !CanOpenRoomRegistrationPopup)
        {
            RoomRegistrationModal.SetExternalLockMessage(RoomCardLockReason ?? "Phiên đăng ký này không còn khả dụng.");
        }
    }

    private void NotifyDashboardPresentationChanged()
    {
        OnPropertyChanged(nameof(StudentDisplayName));
        OnPropertyChanged(nameof(GreetingHeadline));
        OnPropertyChanged(nameof(GreetingSupportingText));
        OnPropertyChanged(nameof(RefreshButtonText));
        OnPropertyChanged(nameof(RoomCardTitle));
        OnPropertyChanged(nameof(RoomDisplayText));
        OnPropertyChanged(nameof(RoomFooterLabel));
        OnPropertyChanged(nameof(RoomFooterText));
        OnPropertyChanged(nameof(IsRoomCardInteractive));
        OnPropertyChanged(nameof(ShowRoomCardHint));
        OnPropertyChanged(nameof(RoomCardHintText));
        OnPropertyChanged(nameof(HasPendingRoomRequest));
        OnPropertyChanged(nameof(HasAssignedRoom));
        OnPropertyChanged(nameof(InvoiceCardTitle));
        OnPropertyChanged(nameof(InvoiceStatusLabel));
        OnPropertyChanged(nameof(CurrentInvoiceAmountText));
        OnPropertyChanged(nameof(CurrentInvoiceUnitText));
        OnPropertyChanged(nameof(CurrentInvoiceStatusText));
        OnPropertyChanged(nameof(TicketCardTitle));
        OnPropertyChanged(nameof(SupportTicketCountText));
        OnPropertyChanged(nameof(SupportTicketStateText));
        OnPropertyChanged(nameof(SupportTicketActionText));
        OnPropertyChanged(nameof(VehicleCardTitle));
        OnPropertyChanged(nameof(VehicleStatusLabel));
        OnPropertyChanged(nameof(VehicleDisplayName));
        OnPropertyChanged(nameof(VehiclePlateText));
        OnPropertyChanged(nameof(VehicleStatusText));
        OnPropertyChanged(nameof(QuickActionsSectionTitle));
        OnPropertyChanged(nameof(SummaryCardTitles));
        OnPropertyChanged(nameof(QuickActionLabels));
        OnPropertyChanged(nameof(RoomRegistrationPopupTitle));
        OnPropertyChanged(nameof(RoomRegistrationPopupSubtitle));
    }
}
