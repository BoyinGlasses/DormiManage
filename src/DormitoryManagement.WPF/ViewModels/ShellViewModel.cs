using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using DormitoryManagement.Application.Abstractions.Auth;
using DormitoryManagement.Application.Abstractions.Services;
using DormitoryManagement.Application.DTOs.Notifications;
using DormitoryManagement.Domain.Constants;
using DormitoryManagement.WPF.Common;
using DormitoryManagement.WPF.Navigation;
using DormitoryManagement.WPF.ViewModels.Audit;
using DormitoryManagement.WPF.ViewModels.Auth;
using DormitoryManagement.WPF.ViewModels.Billing;
using DormitoryManagement.WPF.ViewModels.Dashboard;
using DormitoryManagement.WPF.ViewModels.Forum;
using DormitoryManagement.WPF.ViewModels.Profile;
using DormitoryManagement.WPF.ViewModels.Registrations;
using DormitoryManagement.WPF.ViewModels.Rooms;
using DormitoryManagement.WPF.ViewModels.Settings;
using DormitoryManagement.WPF.ViewModels.Students;
using DormitoryManagement.WPF.ViewModels.SupportTickets;
using DormitoryManagement.WPF.ViewModels.Vehicles;
using Microsoft.Extensions.DependencyInjection;

namespace DormitoryManagement.WPF.ViewModels;

public sealed class ShellViewModel : ViewModelBase
{
    private readonly NavigationStore _navigationStore;
    private readonly INavigationService _navigationService;
    private readonly ICurrentUserService _currentUser;
    private readonly ISessionService _sessionService;
    private readonly IRememberedLoginService _rememberedLoginService;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly SessionState _sessionState;
    private readonly RelayCommand _navigateCommand;
    private readonly AsyncRelayCommand _logoutCommand;
    private readonly AsyncRelayCommand _loadNotificationsCommand;
    private readonly AsyncRelayCommand _toggleNotificationsCommand;
    private readonly AsyncRelayCommand _markNotificationsReadCommand;
    private readonly RelayCommand _openProfileCommand;
    private string _currentPageTitle = "Đăng nhập";
    private string? _currentMenuKey;
    private int _unreadNotificationCount;
    private bool _isNotificationPanelOpen;

    public ShellViewModel(
        NavigationStore navigationStore,
        INavigationService navigationService,
        ICurrentUserService currentUser,
        ISessionService sessionService,
        IRememberedLoginService rememberedLoginService,
        IServiceScopeFactory scopeFactory,
        SessionState sessionState)
    {
        _navigationStore = navigationStore;
        _navigationService = navigationService;
        _currentUser = currentUser;
        _sessionService = sessionService;
        _rememberedLoginService = rememberedLoginService;
        _scopeFactory = scopeFactory;
        _sessionState = sessionState;
        _navigationStore.PropertyChanged += OnNavigationStoreChanged;
        _sessionState.Changed += OnSessionStateChanged;

        _navigateCommand = new RelayCommand(parameter => NavigateByKey(parameter?.ToString()), parameter => CanNavigate(parameter?.ToString()));
        _logoutCommand = new AsyncRelayCommand(LogoutAsync, () => IsAuthenticated);
        _loadNotificationsCommand = new AsyncRelayCommand(LoadNotificationsAsync, () => IsAuthenticated);
        _toggleNotificationsCommand = new AsyncRelayCommand(ToggleNotificationsAsync, () => IsAuthenticated);
        _markNotificationsReadCommand = new AsyncRelayCommand(MarkNotificationsReadAsync, () => IsAuthenticated && Notifications.Count > 0);
        _openProfileCommand = new RelayCommand(OpenProfile, () => IsAuthenticated);
        NavigateCommand = _navigateCommand;
        LogoutCommand = _logoutCommand;
        LoadNotificationsCommand = _loadNotificationsCommand;
        ToggleNotificationsCommand = _toggleNotificationsCommand;
        MarkNotificationsReadCommand = _markNotificationsReadCommand;
        OpenProfileCommand = _openProfileCommand;

        RebuildMenu();
        Navigate<LoginViewModel>("Đăng nhập");
    }

    public ObservableCollection<ShellMenuItem> MenuItems { get; } = new();
    public ObservableCollection<ShellNotificationItem> Notifications { get; } = new();
    public ViewModelBase? CurrentViewModel => _navigationStore.CurrentViewModel;
    public string CurrentUserName => _currentUser.FullName ?? _currentUser.UserName ?? "Guest";
    public string RoleText => _currentUser.Roles.Count == 0 ? "Not signed in" : string.Join(", ", _currentUser.Roles);
    public bool IsAuthenticated => _currentUser.IsAuthenticated;
    public string CurrentPageTitle
    {
        get => _currentPageTitle;
        private set => SetProperty(ref _currentPageTitle, value);
    }

    public ICommand NavigateCommand { get; }
    public ICommand LogoutCommand { get; }
    public ICommand LoadNotificationsCommand { get; }
    public ICommand ToggleNotificationsCommand { get; }
    public ICommand MarkNotificationsReadCommand { get; }
    public ICommand OpenProfileCommand { get; }
    public int UnreadNotificationCount
    {
        get => _unreadNotificationCount;
        private set
        {
            if (SetProperty(ref _unreadNotificationCount, value))
            {
                OnPropertyChanged(nameof(HasUnreadNotifications));
                OnPropertyChanged(nameof(UnreadNotificationText));
            }
        }
    }

    public bool HasUnreadNotifications => UnreadNotificationCount > 0;
    public string UnreadNotificationText => UnreadNotificationCount > 99 ? "99+" : UnreadNotificationCount.ToString();
    public bool HasNotifications => Notifications.Count > 0;
    public bool IsForumHomeChrome => CurrentViewModel is ForumHomeViewModel or ForumPostDetailViewModel;
    public bool IsStudentDashboardChrome => CurrentViewModel is StudentDashboardViewModel;
    public bool IsVehicleRegistrationChrome => false;
    public bool IsDefaultTopBarChrome => false;
    public bool IsSharedTopBarChrome => IsTopBarVisible;
    public bool IsTopBarVisible => !IsForumHomeChrome;
    public bool IsNotificationPanelOpen
    {
        get => _isNotificationPanelOpen;
        set => SetProperty(ref _isNotificationPanelOpen, value);
    }

    private void Navigate<TViewModel>(string title) where TViewModel : ViewModelBase
    {
        CurrentPageTitle = title;
        _navigationService.NavigateTo<TViewModel>();
    }

    private bool CanNavigate(string? key) =>
        key == "Login"
        || key == "Payments" && IsAuthenticated
        || (key is not null && MenuItems.Any(item => item.Key == key));

    private void NavigateByKey(string? key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        if (IsCurrentMenuTarget(key))
        {
            ActivateMenu(key);
            return;
        }

        ActivateMenu(key);
        switch (key)
        {
            case "Login":
                Navigate<LoginViewModel>("Đăng nhập");
                break;
            case "AdminDashboard":
                Navigate<AdminDashboardViewModel>("Dashboard");
                break;
            case "StudentDashboard":
                Navigate<StudentDashboardViewModel>("Dashboard");
                break;
            case "Students":
                Navigate<StudentListViewModel>("Students");
                break;
            case "Rooms":
                Navigate<RoomListViewModel>("Rooms");
                break;
            case "RoomRegistration":
                Navigate<RoomRegistrationViewModel>("Room registration");
                break;
            case "RegistrationApproval":
                Navigate<RegistrationApprovalViewModel>("Registration approvals");
                break;
            case "Invoices":
                Navigate<InvoiceListViewModel>("Billing");
                break;
            case "InvoiceDetail":
                Navigate<InvoiceDetailViewModel>("Invoice detail");
                break;
            case "Payments":
                Navigate<PaymentViewModel>("Payments");
                break;
            case "AuditLogs":
                Navigate<AuditLogListViewModel>("Audit logs");
                break;
            case "Vehicles":
                Navigate<VehicleRegistrationViewModel>("Đăng ký gửi xe");
                break;
            case "Tickets":
                Navigate<SupportTicketListViewModel>("Support tickets");
                break;
            case "TicketDetail":
                Navigate<SupportTicketDetailViewModel>("Ticket detail");
                break;
            case "Topics":
                Navigate<ForumHomeViewModel>("Forum");
                break;
            case "Users":
                Navigate<UserManagementViewModel>("Settings");
                break;
            case "FeeTypes":
                Navigate<FeeTypeViewModel>("Fee types");
                break;
        }
    }

    private bool IsCurrentMenuTarget(string key) =>
        key switch
        {
            "Login" => CurrentViewModel is LoginViewModel,
            "AdminDashboard" => CurrentViewModel is AdminDashboardViewModel,
            "StudentDashboard" => CurrentViewModel is StudentDashboardViewModel,
            "Students" => CurrentViewModel is StudentListViewModel,
            "Rooms" => CurrentViewModel is RoomListViewModel,
            "RoomRegistration" => CurrentViewModel is RoomRegistrationViewModel,
            "RegistrationApproval" => CurrentViewModel is RegistrationApprovalViewModel,
            "Invoices" => CurrentViewModel is InvoiceListViewModel,
            "InvoiceDetail" => CurrentViewModel is InvoiceDetailViewModel,
            "Payments" => CurrentViewModel is PaymentViewModel,
            "AuditLogs" => CurrentViewModel is AuditLogListViewModel,
            "Vehicles" => CurrentViewModel is VehicleRegistrationViewModel,
            "Tickets" => CurrentViewModel is SupportTicketListViewModel,
            "TicketDetail" => CurrentViewModel is SupportTicketDetailViewModel,
            "Topics" => CurrentViewModel is ForumHomeViewModel or ForumPostDetailViewModel,
            "Users" => CurrentViewModel is UserManagementViewModel,
            "FeeTypes" => CurrentViewModel is FeeTypeViewModel,
            _ => false
        };

    private void OpenProfile()
    {
        if (!IsAuthenticated)
        {
            return;
        }

        CurrentPageTitle = "Profile";
        _navigationService.NavigateTo<ProfileViewModel>();
    }

    private async Task LogoutAsync()
    {
        if (!string.IsNullOrWhiteSpace(_currentUser.Email))
        {
            _rememberedLoginService.DowngradeToEmailOnly(_currentUser.Email);
        }

        _sessionService.Clear();
        Notifications.Clear();
        UnreadNotificationCount = 0;
        IsNotificationPanelOpen = false;
        OnPropertyChanged(nameof(HasNotifications));
        _sessionState.NotifyChanged();
        Navigate<LoginViewModel>("Đăng nhập");
        await Task.CompletedTask;
    }

    private void RebuildMenu()
    {
        MenuItems.Clear();

        if (!IsAuthenticated)
        {
            MenuItems.Add(new ShellMenuItem("Đăng nhập", "Login", "IN"));
            ActivateMenu(_currentMenuKey ?? "Login");
            return;
        }

        if (HasAnyRole(RoleNames.Admin))
        {
            AddManagementMenu(includeSettings: true);
            return;
        }

        if (HasAnyRole(RoleNames.Manager))
        {
            AddManagementMenu(includeSettings: false);
            return;
        }

        MenuItems.Add(new ShellMenuItem("Dashboard", "StudentDashboard", "\uE9D2"));
        MenuItems.Add(new ShellMenuItem("Registrations", "RoomRegistration", "\uE9F5"));
        MenuItems.Add(new ShellMenuItem("Billing", "Invoices", "\uE8A5"));
        MenuItems.Add(new ShellMenuItem("Vehicles", "Vehicles", "\uE804"));
        MenuItems.Add(new ShellMenuItem("Support Tickets", "Tickets", "\uE90F"));
        MenuItems.Add(new ShellMenuItem("Forum", "Topics", "\uE8F2"));
        ActivateMenu(_currentMenuKey ?? "StudentDashboard");
    }

    private void AddManagementMenu(bool includeSettings)
    {
        MenuItems.Add(new ShellMenuItem("Dashboard", "AdminDashboard", "\uE9D2"));
        MenuItems.Add(new ShellMenuItem("Students", "Students", "\uE716"));
        MenuItems.Add(new ShellMenuItem("Rooms", "Rooms", "\uE80F"));
        MenuItems.Add(new ShellMenuItem("Registrations", "RegistrationApproval", "\uE9F5"));
        MenuItems.Add(new ShellMenuItem("Billing", "Invoices", "\uE8A5"));
        MenuItems.Add(new ShellMenuItem("Payments", "Payments", "\uE8C7"));
        MenuItems.Add(new ShellMenuItem("Support Tickets", "Tickets", "\uE90F"));
        MenuItems.Add(new ShellMenuItem("Audit Logs", "AuditLogs", "\uE8A5"));
        MenuItems.Add(new ShellMenuItem("Forum", "Topics", "\uE8F2"));
        if (includeSettings)
        {
            MenuItems.Add(new ShellMenuItem("Settings", "Users", "\uE713"));
        }

        ActivateMenu(_currentMenuKey ?? "AdminDashboard");
    }

    private void ActivateMenu(string? key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        _currentMenuKey = key;
        foreach (var item in MenuItems)
        {
            item.IsActive = item.Key == key;
        }
    }

    private bool HasAnyRole(params string[] roleNames) => roleNames.Any(_currentUser.IsInRole);

    private void OnNavigationStoreChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(NavigationStore.CurrentViewModel))
        {
            OnPropertyChanged(nameof(CurrentViewModel));
            OnPropertyChanged(nameof(IsForumHomeChrome));
            OnPropertyChanged(nameof(IsStudentDashboardChrome));
            OnPropertyChanged(nameof(IsVehicleRegistrationChrome));
            OnPropertyChanged(nameof(IsDefaultTopBarChrome));
            OnPropertyChanged(nameof(IsSharedTopBarChrome));
            OnPropertyChanged(nameof(IsTopBarVisible));
            CurrentPageTitle = CurrentViewModel switch
            {
                LoginViewModel => "Đăng nhập",
                StudentDashboardViewModel => "Dashboard",
                AdminDashboardViewModel => "Dashboard",
                StudentListViewModel => "Students",
                RoomListViewModel => "Rooms",
                RoomRegistrationViewModel => "Room registration",
                RegistrationApprovalViewModel => "Registration approvals",
                InvoiceListViewModel => "Billing",
                InvoiceDetailViewModel => "Invoice detail",
                PaymentViewModel => MenuItems.Any(item => item.Key == "Payments") ? "Payments" : "Invoices",
                AuditLogListViewModel => "Audit logs",
                SupportTicketListViewModel => "Support tickets",
                SupportTicketDetailViewModel => "Ticket detail",
                ForumHomeViewModel => "Forum",
                ForumPostDetailViewModel => "Forum",
                ProfileViewModel => "Profile",
                UserManagementViewModel => "Settings",
                FeeTypeViewModel => "Fee types",
                VehicleRegistrationViewModel => "Đăng ký gửi xe",
                _ => CurrentPageTitle
            };
            ActivateMenu(CurrentViewModel switch
            {
                LoginViewModel => "Login",
                StudentDashboardViewModel => "StudentDashboard",
                AdminDashboardViewModel => "AdminDashboard",
                StudentListViewModel => "Students",
                RoomListViewModel => "Rooms",
                RoomRegistrationViewModel => "RoomRegistration",
                RegistrationApprovalViewModel => "RegistrationApproval",
                InvoiceListViewModel => "Invoices",
                InvoiceDetailViewModel => "Invoices",
                PaymentViewModel => "Payments",
                AuditLogListViewModel => "AuditLogs",
                SupportTicketListViewModel => "Tickets",
                SupportTicketDetailViewModel => "Tickets",
                ForumHomeViewModel => "Topics",
                ForumPostDetailViewModel => "Topics",
                ProfileViewModel => _currentMenuKey,
                UserManagementViewModel => "Users",
                FeeTypeViewModel => "FeeTypes",
                VehicleRegistrationViewModel => "Vehicles",
                _ => _currentMenuKey
            });
            if (IsAuthenticated)
            {
                _ = LoadNotificationsAsync();
            }
        }
    }

    private void OnSessionStateChanged(object? sender, EventArgs e)
    {
        RebuildMenu();
        OnPropertyChanged(nameof(CurrentUserName));
        OnPropertyChanged(nameof(RoleText));
        OnPropertyChanged(nameof(IsAuthenticated));
        _navigateCommand.RaiseCanExecuteChanged();
        _logoutCommand.RaiseCanExecuteChanged();
        _loadNotificationsCommand.RaiseCanExecuteChanged();
        _toggleNotificationsCommand.RaiseCanExecuteChanged();
        _markNotificationsReadCommand.RaiseCanExecuteChanged();
        _openProfileCommand.RaiseCanExecuteChanged();
        if (IsAuthenticated)
        {
            _ = LoadNotificationsAsync();
        }
    }

    private async Task LoadNotificationsAsync()
    {
        if (!IsAuthenticated)
        {
            return;
        }

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
            var notifications = await notificationService.GetCurrentUserNotificationsAsync(20);
            Notifications.Clear();
            foreach (var notification in notifications)
            {
                Notifications.Add(new ShellNotificationItem(notification));
            }

            UnreadNotificationCount = await notificationService.GetCurrentUserUnreadCountAsync();
        }
        catch
        {
            Notifications.Clear();
            UnreadNotificationCount = 0;
        }

        OnPropertyChanged(nameof(HasNotifications));
        _markNotificationsReadCommand.RaiseCanExecuteChanged();
        _openProfileCommand.RaiseCanExecuteChanged();
    }

    private async Task ToggleNotificationsAsync()
    {
        if (!IsAuthenticated)
        {
            return;
        }

        IsNotificationPanelOpen = !IsNotificationPanelOpen;
        if (IsNotificationPanelOpen)
        {
            await LoadNotificationsAsync();
        }
    }

    private async Task MarkNotificationsReadAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        await notificationService.MarkAllCurrentUserNotificationsAsReadAsync();
        await LoadNotificationsAsync();
    }
}

public sealed class ShellNotificationItem
{
    public ShellNotificationItem(NotificationDto notification)
    {
        Id = notification.Id;
        UserNotificationId = notification.UserNotificationId;
        Title = notification.Title;
        Message = notification.Message;
        IsRead = notification.IsRead;
        CreatedAt = notification.CreatedAt;
    }

    public Guid Id { get; }
    public Guid UserNotificationId { get; }
    public string Title { get; }
    public string Message { get; }
    public bool IsRead { get; }
    public DateTime CreatedAt { get; }
}










