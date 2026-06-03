using DormitoryManagement.Application.Abstractions.Auth;
using DormitoryManagement.Application.Common;
using DormitoryManagement.Application.DTOs.Auth;
using DormitoryManagement.Application.DTOs.Students;
using DormitoryManagement.Application.DTOs.Vehicles;
using DormitoryManagement.Application.Services.Students;
using DormitoryManagement.Application.Services.Vehicles;
using DormitoryManagement.Domain.Constants;
using DormitoryManagement.WPF.Common;
using DormitoryManagement.WPF.Navigation;
using DormitoryManagement.WPF.ViewModels;
using DormitoryManagement.WPF.ViewModels.Billing;
using DormitoryManagement.WPF.ViewModels.Dashboard;
using DormitoryManagement.WPF.ViewModels.Forum;
using DormitoryManagement.WPF.ViewModels.Students;
using DormitoryManagement.WPF.ViewModels.Vehicles;
using Microsoft.Extensions.DependencyInjection;

namespace DormitoryManagement.WPF.Tests;

public sealed class ShellViewModelTests
{
    [Fact]
    public void Student_menu_hides_direct_payments_item_but_keeps_billing()
    {
        var navigation = new RecordingNavigationService();
        var shell = new ShellViewModel(
            new NavigationStore(),
            navigation,
            StudentUser(),
            new StubSessionService(),
            new StubRememberedLoginService(),
            new ThrowingScopeFactory(),
            new SessionState());

        Assert.Contains(shell.MenuItems, item => item.Key == "Invoices");
        Assert.DoesNotContain(shell.MenuItems, item => item.Key == "Payments");
        Assert.True(shell.NavigateCommand.CanExecute("Payments"));

        shell.NavigateCommand.Execute("Payments");

        Assert.Equal(typeof(PaymentViewModel), navigation.LastViewModelType);
    }

    [Theory]
    [InlineData(RoleNames.Admin, true)]
    [InlineData(RoleNames.Manager, true)]
    [InlineData(RoleNames.BuildingManager, true)]
    [InlineData(RoleNames.Staff, false)]
    public void Non_student_payment_menu_behavior_is_unchanged(string roleName, bool expectedPaymentsMenu)
    {
        var shell = new ShellViewModel(
            new NavigationStore(),
            new RecordingNavigationService(),
            User(roleName),
            new StubSessionService(),
            new StubRememberedLoginService(),
            new ThrowingScopeFactory(),
            new SessionState());

        Assert.Equal(expectedPaymentsMenu, shell.MenuItems.Any(item => item.Key == "Payments"));
    }

    [Fact]
    public void Student_dashboard_route_enables_student_dashboard_chrome_mode()
    {
        var navigationStore = new NavigationStore();
        var shell = new ShellViewModel(
            navigationStore,
            new RecordingNavigationService(),
            StudentUser(),
            new StubSessionService(),
            new StubRememberedLoginService(),
            new ThrowingScopeFactory(),
            new SessionState());

        navigationStore.CurrentViewModel = new StudentDashboardViewModel(new ThrowingScopeFactory(), StudentUser(), new RecordingNavigationService());

        Assert.True(shell.IsStudentDashboardChrome);
        Assert.True(shell.IsTopBarVisible);
        Assert.Equal("Dashboard", shell.CurrentPageTitle);
    }

    [Fact]
    public void Student_profile_navigation_key_routes_to_student_profile_view_model()
    {
        var navigation = new RecordingNavigationService();
        var shell = new ShellViewModel(
            new NavigationStore(),
            navigation,
            StudentUser(),
            new StubSessionService(),
            new StubRememberedLoginService(),
            new ThrowingScopeFactory(),
            new SessionState());

        shell.NavigateCommand.Execute("StudentProfile");

        Assert.Equal(typeof(StudentProfileViewModel), navigation.LastViewModelType);
    }

    [Fact]
    public void Student_profile_route_keeps_student_chrome_and_dashboard_menu_context()
    {
        var navigationStore = new NavigationStore();
        var shell = new ShellViewModel(
            navigationStore,
            new RecordingNavigationService(),
            StudentUser(),
            new StubSessionService(),
            new StubRememberedLoginService(),
            new ThrowingScopeFactory(),
            new SessionState());

        navigationStore.CurrentViewModel = new StudentProfileViewModel(new StubStudentService());

        Assert.True(shell.IsStudentDashboardChrome);
        Assert.True(shell.IsTopBarVisible);
        Assert.Equal("Hồ sơ cá nhân", shell.CurrentPageTitle);
        Assert.Contains(shell.MenuItems, item => item.Key == "StudentDashboard" && item.IsActive);
    }

    [Fact]
    public void Vehicle_registration_route_enables_vehicle_registration_chrome_mode()
    {
        var navigationStore = new NavigationStore();
        var shell = new ShellViewModel(
            navigationStore,
            new RecordingNavigationService(),
            StudentUser(),
            new StubSessionService(),
            new StubRememberedLoginService(),
            new ThrowingScopeFactory(),
            new SessionState());

        navigationStore.CurrentViewModel = new VehicleRegistrationViewModel(new StubVehicleService());

        Assert.True(shell.IsVehicleRegistrationChrome);
        Assert.False(shell.IsStudentDashboardChrome);
        Assert.True(shell.IsTopBarVisible);
        Assert.Equal("Vehicle registration", shell.CurrentPageTitle);
        Assert.Contains(shell.MenuItems, item => item.Key == "Vehicles" && item.IsActive);
    }

    [Fact]
    public void Vehicle_registration_chrome_mode_is_scoped_to_vehicle_route_only()
    {
        var navigationStore = new NavigationStore();
        var shell = new ShellViewModel(
            navigationStore,
            new RecordingNavigationService(),
            StudentUser(),
            new StubSessionService(),
            new StubRememberedLoginService(),
            new ThrowingScopeFactory(),
            new SessionState());

        navigationStore.CurrentViewModel = new ForumHomeViewModel();

        Assert.False(shell.IsVehicleRegistrationChrome);
        Assert.False(shell.IsStudentDashboardChrome);
        Assert.False(shell.IsTopBarVisible);
        Assert.Equal("Forum", shell.CurrentPageTitle);
    }

    private static ICurrentUserService StudentUser() => new StubCurrentUser(RoleNames.Student, Guid.NewGuid());
    private static ICurrentUserService User(string roleName) => new StubCurrentUser(roleName, Guid.NewGuid());

    private sealed class RecordingNavigationService : INavigationService
    {
        public Type? LastViewModelType { get; private set; }

        public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase
        {
            LastViewModelType = typeof(TViewModel);
        }
    }

    private sealed class StubSessionService : ISessionService
    {
        public CurrentUserDto? CurrentUser { get; private set; }
        public void SetCurrentUser(CurrentUserDto user) => CurrentUser = user;
        public void Clear() => CurrentUser = null;
    }

    private sealed class StubRememberedLoginService : IRememberedLoginService
    {
        public RememberedLoginState Load() => RememberedLoginState.Empty;
        public void SaveFullLogin(string emailOrStudentCode, string password) { }
        public void SaveEmailOnly(string emailOrStudentCode) { }
        public void DowngradeToEmailOnly(string emailOrStudentCode) { }
        public void Clear() { }
    }

    private sealed class ThrowingScopeFactory : IServiceScopeFactory
    {
        public IServiceScope CreateScope() => throw new NotSupportedException();
    }

    private sealed class StubVehicleService : IVehicleService
    {
        public Task<IReadOnlyList<VehicleRegistrationDto>> GetCurrentStudentVehicleRegistrationsAsync(DateTime? asOfDate = null, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<VehicleRegistrationDto>>(Array.Empty<VehicleRegistrationDto>());

        public Task<VehicleRegistrationDto> RegisterVehicleAsync(CreateVehicleRegistrationRequest request, CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task ApproveVehicleAsync(Guid registrationId, CancellationToken ct = default) => Task.CompletedTask;
        public Task RejectVehicleAsync(Guid registrationId, string reason, CancellationToken ct = default) => Task.CompletedTask;
        public Task CancelVehicleAsync(Guid registrationId, CancellationToken ct = default) => Task.CompletedTask;
    }

    private sealed class StubStudentService : IStudentService
    {
        public Task<PagedResult<StudentDto>> GetStudentsAsync(int pageNumber = 1, int pageSize = 20, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<StudentDto?> GetStudentByIdAsync(Guid id, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<StudentProfileDto> GetCurrentStudentProfileAsync(CancellationToken ct = default) => Task.FromResult(new StudentProfileDto { FullName = "Nguyễn Văn A", StudentCode = "24521111" });
        public Task<StudentDto> CreateStudentAsync(CreateStudentRequest request, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<StudentDto> UpdateStudentAsync(UpdateStudentRequest request, CancellationToken ct = default) => throw new NotSupportedException();
        public Task DeactivateStudentAsync(Guid id, CancellationToken ct = default) => throw new NotSupportedException();
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
                FullName = roleName,
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
