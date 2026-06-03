using System.Windows.Input;
using DormitoryManagement.Application.Abstractions.Auth;
using DormitoryManagement.Application.DTOs.Auth;
using DormitoryManagement.Application.Abstractions.Services;
using DormitoryManagement.Domain.Constants;
using DormitoryManagement.WPF.Common;
using DormitoryManagement.WPF.Navigation;
using DormitoryManagement.WPF.ViewModels;
using DormitoryManagement.WPF.ViewModels.Dashboard;
using DormitoryManagement.WPF.ViewModels.Forum;
using Microsoft.Extensions.DependencyInjection;

namespace DormitoryManagement.WPF.Tests;

public sealed class ShellForumNavigationTests
{
    [Fact]
    public void Forum_menu_routes_to_forum_home_view_model()
    {
        var navigation = new RecordingNavigationService();
        var shell = CreateShell(new NavigationStore(), navigation);

        Assert.True(shell.NavigateCommand.CanExecute("Topics"));

        shell.NavigateCommand.Execute("Topics");

        Assert.Equal(typeof(ForumHomeViewModel), navigation.LastViewModelType);
    }

    [Fact]
    public void Student_dashboard_open_forum_routes_to_forum_home_view_model()
    {
        var navigation = new RecordingNavigationService();
        var dashboard = new StudentDashboardViewModel(new ThrowingScopeFactory(), new StubCurrentUser(RoleNames.Student, Guid.NewGuid()), navigation);

        Assert.True(dashboard.OpenForumCommand.CanExecute(null));

        dashboard.OpenForumCommand.Execute(null);

        Assert.Equal(typeof(ForumHomeViewModel), navigation.LastViewModelType);
    }

    [Fact]
    public void Shell_switches_to_forum_home_chrome_mode_when_forum_home_is_current_surface()
    {
        var navigationStore = new NavigationStore();
        var shell = CreateShell(navigationStore, new RecordingNavigationService());

        navigationStore.CurrentViewModel = new ForumHomeViewModel();

        Assert.Contains(shell.MenuItems, item => item.Key == "Topics" && item.IsActive);
        Assert.True(shell.IsForumHomeChrome);
        Assert.False(shell.IsTopBarVisible);
        Assert.Equal("Forum", shell.CurrentPageTitle);
    }


    [Fact]
    public void Forum_home_post_selection_routes_to_forum_post_detail_view_model()
    {
        const string viewModelTypeName = "DormitoryManagement.WPF.ViewModels.Forum.ForumHomeViewModel, DormitoryManagement.WPF";
        const string detailViewModelTypeName = "DormitoryManagement.WPF.ViewModels.Forum.ForumPostDetailViewModel, DormitoryManagement.WPF";

        var forumHomeType = Type.GetType(viewModelTypeName);
        Assert.NotNull(forumHomeType);

        var constructor = forumHomeType!.GetConstructors()
            .FirstOrDefault(candidate =>
            {
                var parameters = candidate.GetParameters();
                return parameters.Length == 1 && parameters[0].ParameterType == typeof(INavigationService);
            });
        Assert.NotNull(constructor);

        var navigation = new RecordingNavigationService();
        var forumHome = constructor!.Invoke([navigation]);
        var feedCards = forumHomeType.GetProperty("FeedCards")?.GetValue(forumHome) as System.Collections.IEnumerable;
        Assert.NotNull(feedCards);

        var firstPost = feedCards!.Cast<object?>().FirstOrDefault(item => item is not null);
        Assert.NotNull(firstPost);

        var command = forumHomeType.GetProperty("OpenPostDetailCommand")?.GetValue(forumHome) as ICommand;
        Assert.NotNull(command);
        Assert.True(command!.CanExecute(firstPost));

        command.Execute(firstPost);

        var detailViewModelType = Type.GetType(detailViewModelTypeName);
        Assert.NotNull(detailViewModelType);
        Assert.Equal(detailViewModelType, navigation.LastViewModelType);
    }

    [Fact]
    public void Shell_switches_to_forum_post_detail_chrome_mode_when_detail_surface_is_current()
    {
        const string detailViewModelTypeName = "DormitoryManagement.WPF.ViewModels.Forum.ForumPostDetailViewModel, DormitoryManagement.WPF";

        var detailViewModelType = Type.GetType(detailViewModelTypeName);
        Assert.NotNull(detailViewModelType);

        var constructor = detailViewModelType!.GetConstructor(Type.EmptyTypes);
        Assert.NotNull(constructor);

        var navigationStore = new NavigationStore();
        var shell = CreateShell(navigationStore, new RecordingNavigationService());

        navigationStore.CurrentViewModel = Assert.IsAssignableFrom<ViewModelBase>(constructor!.Invoke([]));

        Assert.Contains(shell.MenuItems, item => item.Key == "Topics" && item.IsActive);
        Assert.True(shell.IsForumHomeChrome);
        Assert.False(shell.IsTopBarVisible);
        Assert.Equal("Forum", shell.CurrentPageTitle);
    }
    private static ShellViewModel CreateShell(NavigationStore navigationStore, INavigationService navigationService) =>
        new(
            navigationStore,
            navigationService,
            new StubCurrentUser(RoleNames.Student, Guid.NewGuid()),
            new StubSessionService(),
            new StubRememberedLoginService(),
            new ThrowingScopeFactory(),
            new SessionState());

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



