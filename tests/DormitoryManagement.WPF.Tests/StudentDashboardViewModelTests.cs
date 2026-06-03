using DormitoryManagement.Application.Abstractions.Auth;
using DormitoryManagement.Application.Common;
using DormitoryManagement.Application.DTOs.Auth;
using DormitoryManagement.Application.DTOs.Dashboard;
using DormitoryManagement.Application.DTOs.Registrations;
using DormitoryManagement.Application.DTOs.Rooms;
using DormitoryManagement.Application.Services.Dashboard;
using DormitoryManagement.Application.Services.Registrations;
using DormitoryManagement.Application.Services.Rooms;
using DormitoryManagement.Domain.Constants;
using DormitoryManagement.Domain.Enums;
using DormitoryManagement.WPF.Common;
using DormitoryManagement.WPF.Navigation;
using DormitoryManagement.WPF.ViewModels;
using DormitoryManagement.WPF.ViewModels.Billing;
using DormitoryManagement.WPF.ViewModels.Dashboard;
using DormitoryManagement.WPF.ViewModels.Forum;
using DormitoryManagement.WPF.ViewModels.Registrations;
using DormitoryManagement.WPF.ViewModels.SupportTickets;
using Microsoft.Extensions.DependencyInjection;

namespace DormitoryManagement.WPF.Tests;

public sealed class StudentDashboardViewModelTests
{
    [Fact]
    public void Greeting_and_dashboard_copy_match_reference_text()
    {
        var viewModel = CreateViewModel();

        Assert.Equal("Chào buổi sáng, Nguyễn Văn A", viewModel.GreetingHeadline);
        Assert.Equal("Tổng quan hoạt động và thông tin phòng của bạn.", viewModel.GreetingSupportingText);
        Assert.Equal("Làm mới", viewModel.RefreshButtonText);
        Assert.Equal("Tiện ích nhanh", viewModel.QuickActionsSectionTitle);
        Assert.Equal("Trạng thái:", viewModel.InvoiceStatusLabel);
        Assert.Equal("Trạng thái:", viewModel.VehicleStatusLabel);
        Assert.Equal(["Phòng của tôi", "Hóa đơn hiện tại", "Yêu cầu hỗ trợ", "Đăng ký xe"], viewModel.SummaryCardTitles);
        Assert.Equal(["Thanh toán", "Báo hỏng", "Forum", "Lịch"], viewModel.QuickActionLabels);
    }

    [Fact]
    public async Task Empty_state_allows_room_card_click_and_opens_modal_registration_session()
    {
        var viewModel = CreateViewModel(
            dashboard: new StudentDashboardDto
            {
                RoomCardDisplayMode = "Empty",
                RoomCardStatusText = "Chưa phân phòng",
                CanOpenRoomRegistrationPopup = true
            },
            availableRooms: [new RoomDto { Id = Guid.NewGuid(), BuildingName = "Khu A", RoomNumber = "101", AvailableSlots = 2, MonthlyPrice = 750000m, GenderType = RoomGenderType.Male }]);

        viewModel.RefreshCommand.Execute(null);
        Assert.True(await WaitUntilAsync(() => viewModel.CanOpenRoomRegistrationPopup));

        viewModel.OpenRoomRegistrationPopupCommand.Execute(null);

        Assert.True(await WaitUntilAsync(() => viewModel.IsRoomRegistrationPopupOpen && viewModel.RoomRegistrationModal is not null));
        Assert.True(viewModel.IsRoomCardInteractive);
        Assert.NotNull(viewModel.RoomRegistrationModal);
    }

    [Fact]
    public async Task Pending_state_locks_card_and_projects_pending_label_text()
    {
        var viewModel = CreateViewModel(dashboard: new StudentDashboardDto
        {
            RequestedRoom = "A-101",
            RoomCardDisplayMode = "Pending",
            RoomCardStatusText = "(đang chờ xử lí)",
            CanOpenRoomRegistrationPopup = false,
            RoomCardLockReason = "Yêu cầu đăng ký đang chờ xử lí."
        });

        viewModel.RefreshCommand.Execute(null);

        Assert.True(await WaitUntilAsync(() => viewModel.RoomCardDisplayMode == "Pending"));
        Assert.Equal("A-101", viewModel.RoomDisplayText);
        Assert.Equal("(đang chờ xử lí)", viewModel.RoomFooterText);
        Assert.False(viewModel.CanOpenRoomRegistrationPopup);
        Assert.False(viewModel.IsRoomCardInteractive);
    }

    [Fact]
    public async Task Payment_pending_state_keeps_card_locked_and_surfaces_lock_reason()
    {
        var viewModel = CreateViewModel(dashboard: new StudentDashboardDto
        {
            RequestedRoom = "A-101",
            RoomCardDisplayMode = "Pending",
            RoomCardStatusText = "(đang chờ xử lí)",
            CanOpenRoomRegistrationPopup = false,
            RoomCardLockReason = "Yêu cầu đăng ký đang chờ thanh toán hợp đồng."
        });

        viewModel.RefreshCommand.Execute(null);

        Assert.True(await WaitUntilAsync(() => viewModel.RoomCardDisplayMode == "Pending"));
        Assert.Equal("Yêu cầu đăng ký đang chờ thanh toán hợp đồng.", viewModel.RoomCardHintText);
        Assert.False(viewModel.CanOpenRoomRegistrationPopup);
    }

    [Fact]
    public async Task Modal_shows_no_eligible_room_status_and_keeps_submit_disabled_when_room_list_is_empty()
    {
        var viewModel = CreateViewModel(
            dashboard: new StudentDashboardDto
            {
                RoomCardDisplayMode = "Empty",
                RoomCardStatusText = "Chưa phân phòng",
                CanOpenRoomRegistrationPopup = true
            },
            availableRooms: Array.Empty<RoomDto>());

        viewModel.RefreshCommand.Execute(null);
        Assert.True(await WaitUntilAsync(() => viewModel.CanOpenRoomRegistrationPopup));

        viewModel.OpenRoomRegistrationPopupCommand.Execute(null);

        Assert.True(await WaitUntilAsync(() => viewModel.RoomRegistrationModal is not null));
        Assert.NotNull(viewModel.RoomRegistrationModal);
        Assert.True(await WaitUntilAsync(() => viewModel.RoomRegistrationModal!.IsAvailableRoomsEmpty));
        Assert.False(viewModel.RoomRegistrationModal!.CanSubmitRegistration);
        Assert.Equal("Hiện chưa có phòng trống phù hợp để đăng ký.", viewModel.RoomRegistrationModal.AvailabilityStatusMessage);
    }

    [Fact]
    public async Task Popup_dependent_selectors_narrow_room_numbers_by_building_floor_and_room_type()
    {
        var khuA101 = new RoomDto { Id = Guid.NewGuid(), BuildingName = "Khu A", FloorNumber = 1, RoomNumber = "101", Capacity = 4, AvailableSlots = 2, MonthlyPrice = 750000m, GenderType = RoomGenderType.Male };
        var khuA202 = new RoomDto { Id = Guid.NewGuid(), BuildingName = "Khu A", FloorNumber = 2, RoomNumber = "202", Capacity = 8, AvailableSlots = 5, MonthlyPrice = 650000m, GenderType = RoomGenderType.Male };
        var khuB201 = new RoomDto { Id = Guid.NewGuid(), BuildingName = "Khu B", FloorNumber = 2, RoomNumber = "201", Capacity = 8, AvailableSlots = 4, MonthlyPrice = 650000m, GenderType = RoomGenderType.Female };
        var viewModel = CreateViewModel(
            dashboard: new StudentDashboardDto { RoomCardDisplayMode = "Empty", RoomCardStatusText = "Chưa phân phòng", CanOpenRoomRegistrationPopup = true },
            availableRooms: [khuA101, khuA202, khuB201]);

        viewModel.RefreshCommand.Execute(null);
        Assert.True(await WaitUntilAsync(() => viewModel.CanOpenRoomRegistrationPopup));
        viewModel.OpenRoomRegistrationPopupCommand.Execute(null);
        Assert.True(await WaitUntilAsync(() => viewModel.RoomRegistrationModal?.HasAvailableRooms == true));

        var modal = viewModel.RoomRegistrationModal!;
        modal.SelectedBuilding = "Khu A";
        modal.SelectedFloor = "Tầng 2";
        modal.SelectedRoomType = "Phòng 8 người";

        var room = Assert.Single(modal.AvailableRooms);
        Assert.Equal(khuA202.Id, room.Id);
        Assert.Equal(khuA202.Id, modal.SelectedRoom?.Id);
    }

    [Fact]
    public async Task Popup_submit_requires_terms_acceptance_before_service_call()
    {
        var room = new RoomDto { Id = Guid.NewGuid(), BuildingName = "Khu A", FloorNumber = 1, RoomNumber = "101", Capacity = 4, AvailableSlots = 2, MonthlyPrice = 750000m, GenderType = RoomGenderType.Male };
        var registrationService = new StubRegistrationService(Array.Empty<RoomRegistrationDto>());
        var viewModel = CreateViewModel(
            dashboard: new StudentDashboardDto { RoomCardDisplayMode = "Empty", RoomCardStatusText = "Chưa phân phòng", CanOpenRoomRegistrationPopup = true },
            availableRooms: [room],
            registrationService: registrationService);

        viewModel.RefreshCommand.Execute(null);
        Assert.True(await WaitUntilAsync(() => viewModel.CanOpenRoomRegistrationPopup));
        viewModel.OpenRoomRegistrationPopupCommand.Execute(null);
        Assert.True(await WaitUntilAsync(() => viewModel.RoomRegistrationModal?.SelectedRoom is not null));

        var modal = viewModel.RoomRegistrationModal!;
        Assert.False(modal.AcceptsDormRules);
        Assert.False(modal.CanSubmitRegistration);

        modal.AcceptsDormRules = true;

        Assert.True(modal.CanSubmitRegistration);
        modal.SubmitCommand.Execute(null);

        Assert.True(await WaitUntilAsync(() => registrationService.LastRequest is not null));
        Assert.Equal(room.Id, registrationService.LastRequest!.RoomId);
        Assert.False(registrationService.LastRequest.IncludesInternet);
    }

    [Fact]
    public async Task Popup_external_lock_keeps_modal_open_and_disables_submit_with_status_message()
    {
        var room = new RoomDto { Id = Guid.NewGuid(), BuildingName = "Khu A", FloorNumber = 1, RoomNumber = "101", Capacity = 4, AvailableSlots = 2, MonthlyPrice = 750000m, GenderType = RoomGenderType.Male };
        var viewModel = CreateViewModel(
            dashboard: new StudentDashboardDto { RoomCardDisplayMode = "Empty", RoomCardStatusText = "Chưa phân phòng", CanOpenRoomRegistrationPopup = true },
            availableRooms: [room]);

        viewModel.RefreshCommand.Execute(null);
        Assert.True(await WaitUntilAsync(() => viewModel.CanOpenRoomRegistrationPopup));
        viewModel.OpenRoomRegistrationPopupCommand.Execute(null);
        Assert.True(await WaitUntilAsync(() => viewModel.RoomRegistrationModal?.SelectedRoom is not null));

        var modal = viewModel.RoomRegistrationModal!;
        modal.AcceptsDormRules = true;
        Assert.True(modal.CanSubmitRegistration);

        modal.SetExternalLockMessage("Yêu cầu đăng ký đang chờ xử lí.");

        Assert.True(viewModel.IsRoomRegistrationPopupOpen);
        Assert.False(modal.CanSubmitRegistration);
        Assert.Equal("Yêu cầu đăng ký đang chờ xử lí.", modal.AvailabilityStatusMessage);
    }

    [Fact]
    public void Shortcut_commands_preserve_navigation_outcomes()
    {
        var navigation = new RecordingNavigationService();
        var viewModel = CreateViewModel(navigationService: navigation);

        viewModel.PayInvoiceCommand.Execute(null);
        Assert.Equal(typeof(PaymentViewModel), navigation.LastViewModelType);

        viewModel.CreateTicketCommand.Execute(null);
        Assert.Equal(typeof(SupportTicketListViewModel), navigation.LastViewModelType);

        viewModel.OpenForumCommand.Execute(null);
        Assert.Equal(typeof(ForumHomeViewModel), navigation.LastViewModelType);

        viewModel.RegisterRoomCommand.Execute(null);
        Assert.Equal(typeof(RoomRegistrationViewModel), navigation.LastViewModelType);
    }

    private static StudentDashboardViewModel CreateViewModel(
        StudentDashboardDto? dashboard = null,
        IReadOnlyList<RoomDto>? availableRooms = null,
        IReadOnlyList<RoomRegistrationDto>? currentRegistrations = null,
        IRoomRegistrationService? registrationService = null,
        INavigationService? navigationService = null) =>
        new(
            new StubScopeFactory(
                new StubDashboardService(dashboard ?? new StudentDashboardDto()),
                registrationService ?? new StubRegistrationService(currentRegistrations ?? Array.Empty<RoomRegistrationDto>()),
                new StubRoomService(availableRooms ?? Array.Empty<RoomDto>())),
            new StubCurrentUser(RoleNames.Student, Guid.NewGuid()),
            navigationService ?? new RecordingNavigationService());

    private static async Task<bool> WaitUntilAsync(Func<bool> condition)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
        while (!cts.IsCancellationRequested)
        {
            if (condition())
            {
                return true;
            }

            await Task.Delay(10, cts.Token);
        }

        return false;
    }

    private sealed class StubDashboardService : IDashboardService
    {
        private readonly StudentDashboardDto _dashboard;
        public StubDashboardService(StudentDashboardDto dashboard) => _dashboard = dashboard;
        public Task<AdminDashboardDto> GetAdminDashboardAsync(CancellationToken ct = default) => throw new NotSupportedException();
        public Task<StudentDashboardDto> GetStudentDashboardAsync(Guid studentId, CancellationToken ct = default) => Task.FromResult(_dashboard);
        public Task<IReadOnlyList<ChartPointDto>> GetRevenueByMonthAsync(int year, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<IReadOnlyList<RoomOccupancyDto>> GetRoomOccupancyAsync(CancellationToken ct = default) => throw new NotSupportedException();
        public Task<DebtSummaryDto> GetDebtSummaryAsync(CancellationToken ct = default) => throw new NotSupportedException();
        public Task<IReadOnlyList<DashboardRegistrationDto>> GetPendingRegistrationsAsync(CancellationToken ct = default) => throw new NotSupportedException();
        public Task<IReadOnlyList<DashboardInvoiceDto>> GetOverdueInvoicesAsync(CancellationToken ct = default) => throw new NotSupportedException();
        public Task<IReadOnlyList<DashboardTicketDto>> GetOpenSupportTicketsAsync(CancellationToken ct = default) => throw new NotSupportedException();
        public Task<IReadOnlyList<DashboardActivityDto>> GetRecentActivitiesAsync(CancellationToken ct = default) => throw new NotSupportedException();
    }

    private sealed class StubRegistrationService : IRoomRegistrationService
    {
        private readonly IReadOnlyList<RoomRegistrationDto> _registrations;
        public StubRegistrationService(IReadOnlyList<RoomRegistrationDto> registrations) => _registrations = registrations;
        public CreateRoomRegistrationRequest? LastRequest { get; private set; }
        public Task<Guid> CreateRegistrationAsync(CreateRoomRegistrationRequest request, CancellationToken ct = default)
        {
            LastRequest = request;
            return Task.FromResult(Guid.NewGuid());
        }
        public Task<IReadOnlyList<RoomRegistrationDto>> GetPendingRegistrationsAsync(CancellationToken ct = default) => Task.FromResult<IReadOnlyList<RoomRegistrationDto>>(Array.Empty<RoomRegistrationDto>());
        public Task<IReadOnlyList<RoomRegistrationDto>> GetCurrentStudentRegistrationsAsync(CancellationToken ct = default) => Task.FromResult(_registrations);
        public Task ApproveRegistrationAsync(ApproveRoomRegistrationRequest request, CancellationToken ct = default) => Task.CompletedTask;
        public Task RejectRegistrationAsync(RejectRoomRegistrationRequest request, CancellationToken ct = default) => Task.CompletedTask;
        public Task CancelRegistrationAsync(Guid registrationId, CancellationToken ct = default) => Task.CompletedTask;
    }

    private sealed class StubRoomService : IRoomService
    {
        private readonly IReadOnlyList<RoomDto> _rooms;
        public StubRoomService(IReadOnlyList<RoomDto> rooms) => _rooms = rooms;
        public Task<PagedResult<RoomDto>> GetRoomsAsync(RoomFilterRequest? request = null, CancellationToken ct = default) => Task.FromResult(PagedResult<RoomDto>.Empty());
        public Task<IReadOnlyList<RoomDto>> GetAvailableRoomsAsync(RoomFilterRequest? request = null, CancellationToken ct = default) => Task.FromResult(_rooms);
        public Task<RoomDto> CreateRoomAsync(CreateRoomRequest request, CancellationToken ct = default) => Task.FromResult(new RoomDto());
        public Task<RoomDto> UpdateRoomAsync(Guid roomId, CreateRoomRequest request, CancellationToken ct = default) => Task.FromResult(new RoomDto { Id = roomId });
        public Task ChangeRoomStatusAsync(Guid roomId, RoomStatus status, CancellationToken ct = default) => Task.CompletedTask;
    }

    private sealed class StubScopeFactory : IServiceScopeFactory
    {
        private readonly IDashboardService _dashboardService;
        private readonly IRoomRegistrationService _registrationService;
        private readonly IRoomService _roomService;

        public StubScopeFactory(IDashboardService dashboardService, IRoomRegistrationService registrationService, IRoomService roomService)
        {
            _dashboardService = dashboardService;
            _registrationService = registrationService;
            _roomService = roomService;
        }

        public IServiceScope CreateScope() => new StubScope(_dashboardService, _registrationService, _roomService);
    }

    private sealed class StubScope : IServiceScope
    {
        public StubScope(IDashboardService dashboardService, IRoomRegistrationService registrationService, IRoomService roomService)
        {
            ServiceProvider = new StubServiceProvider(dashboardService, registrationService, roomService);
        }

        public IServiceProvider ServiceProvider { get; }
        public void Dispose() { }
    }

    private sealed class StubServiceProvider : IServiceProvider
    {
        private readonly IDashboardService _dashboardService;
        private readonly IRoomRegistrationService _registrationService;
        private readonly IRoomService _roomService;

        public StubServiceProvider(IDashboardService dashboardService, IRoomRegistrationService registrationService, IRoomService roomService)
        {
            _dashboardService = dashboardService;
            _registrationService = registrationService;
            _roomService = roomService;
        }

        public object? GetService(Type serviceType) => serviceType == typeof(IDashboardService)
            ? _dashboardService
            : serviceType == typeof(RoomRegistrationViewModel)
                ? new RoomRegistrationViewModel(_registrationService, _roomService)
                : null;
    }

    private sealed class RecordingNavigationService : INavigationService
    {
        public Type? LastViewModelType { get; private set; }
        public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase => LastViewModelType = typeof(TViewModel);
    }

    private sealed class StubCurrentUser : ICurrentUserService
    {
        public StubCurrentUser(string roleName, Guid studentId)
        {
            CurrentUser = new CurrentUserDto
            {
                UserId = Guid.NewGuid(),
                Username = roleName,
                Email = roleName + "@ktx.local",
                FullName = "Nguyễn Văn A",
                RoleName = roleName,
                StudentId = studentId
            };
        }

        public CurrentUserDto? CurrentUser { get; }
        public Guid? UserId => CurrentUser?.UserId;
        public string? UserName => CurrentUser?.Username;
        public string? Email => CurrentUser?.Email;
        public string? FullName => CurrentUser?.FullName;
        public IReadOnlyCollection<string> Roles => CurrentUser?.RoleName is { Length: > 0 } roleName ? new[] { roleName } : Array.Empty<string>();
        public bool IsAuthenticated => CurrentUser is not null;
        public bool IsInRole(string roleName) => Roles.Contains(roleName, StringComparer.OrdinalIgnoreCase);
    }
}
