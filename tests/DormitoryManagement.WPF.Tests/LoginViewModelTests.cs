using System.Windows.Input;
using DormitoryManagement.Application.Abstractions.Auth;
using DormitoryManagement.Application.DTOs.Auth;
using DormitoryManagement.Application.Services.Auth;
using DormitoryManagement.Domain.Constants;
using DormitoryManagement.WPF.Common;
using DormitoryManagement.WPF.Navigation;
using DormitoryManagement.WPF.ViewModels.Auth;
using DormitoryManagement.WPF.ViewModels.Dashboard;

namespace DormitoryManagement.WPF.Tests;

public sealed class LoginViewModelTests
{
    [Fact]
    public void Empty_login_submit_uses_vietnamese_validation_copy_and_skips_auth_call()
    {
        var auth = new StubAuthService();
        var viewModel = CreateViewModel(auth: auth);

        viewModel.LoginCommand.Execute(null);

        Assert.Equal("Nhập mã sinh viên.", viewModel.AccountError);
        Assert.Equal("Nhập mật khẩu.", viewModel.PasswordError);
        Assert.False(auth.LoginCalled);
    }

    [Fact]
    public void Registration_handoff_remains_the_secondary_functional_affordance()
    {
        var navigation = new RecordingNavigationService();
        var viewModel = CreateViewModel(navigation: navigation);

        viewModel.CreateAccountCommand.Execute(null);

        Assert.Equal(typeof(RegisterViewModel), navigation.LastViewModelType);
    }

    [Fact]
    public void Invalid_password_failure_uses_vietnamese_error_copy()
    {
        var auth = new StubAuthService
        {
            Result = LoginResult.Failed("invalid", LoginFailureReason.InvalidPassword)
        };
        var viewModel = CreateViewModel(auth: auth);
        viewModel.EmailOrStudentCode = "SV001";
        viewModel.Password = "wrong";

        viewModel.LoginCommand.Execute(null);

        WaitUntil(() => !viewModel.IsLoading);
        Assert.Equal("Mật khẩu không đúng.", viewModel.PasswordError);
    }


    [Fact]
    public void Remembered_login_prefills_account_and_enables_remember_me()
    {
        var remembered = new StubRememberedLoginService
        {
            State = new RememberedLoginState("SV20248901", string.Empty, false)
        };

        var viewModel = CreateViewModel(remembered: remembered);

        Assert.Equal("SV20248901", viewModel.EmailOrStudentCode);
        Assert.True(viewModel.RememberMe);
        Assert.Equal(string.Empty, viewModel.Password);
    }

    [Fact]
    public void Account_not_found_failure_clears_password_and_uses_localized_copy()
    {
        var auth = new StubAuthService
        {
            Result = LoginResult.Failed("missing", LoginFailureReason.AccountNotFound)
        };
        var viewModel = CreateViewModel(auth: auth);
        viewModel.EmailOrStudentCode = "SV404";
        viewModel.Password = "secret";

        viewModel.LoginCommand.Execute(null);

        WaitUntil(() => !viewModel.IsLoading);
        Assert.Equal("Mã sinh viên hoặc email không tồn tại.", viewModel.AccountError);
        Assert.Equal(string.Empty, viewModel.Password);
    }

    [Fact]
    public void Successful_student_login_navigates_to_student_dashboard_and_persists_remembered_credentials()
    {
        var auth = new StubAuthService();
        var navigation = new RecordingNavigationService();
        var remembered = new StubRememberedLoginService();
        var viewModel = CreateViewModel(auth: auth, navigation: navigation, remembered: remembered);
        viewModel.EmailOrStudentCode = "student01";
        viewModel.Password = "123456";
        viewModel.RememberMe = true;

        viewModel.LoginCommand.Execute(null);

        WaitUntil(() => !viewModel.IsLoading);
        Assert.True(auth.LoginCalled);
        Assert.Equal(typeof(StudentDashboardViewModel), navigation.LastViewModelType);
        Assert.Equal("student01@ktx.local", remembered.State.EmailOrStudentCode);
        Assert.Equal("123456", remembered.State.Password);
        Assert.True(remembered.State.HasPassword);
    }


    private static LoginViewModel CreateViewModel(
        StubAuthService? auth = null,
        RecordingNavigationService? navigation = null,
        StubRememberedLoginService? remembered = null,
        StubLoginPrefillState? prefill = null)
    {
        return new LoginViewModel(
            auth ?? new StubAuthService(),
            navigation ?? new RecordingNavigationService(),
            new SessionState(),
            remembered ?? new StubRememberedLoginService(),
            prefill ?? new StubLoginPrefillState());
    }

    private static void WaitUntil(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow.AddSeconds(2);
        while (!condition() && DateTime.UtcNow < deadline)
        {
            Thread.Sleep(10);
        }

        Assert.True(condition());
    }

    private sealed class StubAuthService : IAuthService
    {
        public bool LoginCalled { get; private set; }

        public LoginResult Result { get; set; } = LoginResult.Success(new CurrentUserDto
        {
            UserId = Guid.NewGuid(),
            Username = "student01",
            Email = "student01@ktx.local",
            FullName = "Sinh viên",
            RoleName = RoleNames.Student,
            StudentId = Guid.NewGuid()
        });

        public Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken ct = default)
        {
            LoginCalled = true;
            return Task.FromResult(Result);
        }

        public Task LogoutAsync(CancellationToken ct = default) => Task.CompletedTask;

        public Task ChangePasswordAsync(ChangePasswordRequest request, CancellationToken ct = default) => Task.CompletedTask;
    }

    private sealed class RecordingNavigationService : INavigationService
    {
        public Type? LastViewModelType { get; private set; }

        public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase
        {
            LastViewModelType = typeof(TViewModel);
        }
    }

    private sealed class StubRememberedLoginService : IRememberedLoginService
    {
        public RememberedLoginState State { get; set; } = RememberedLoginState.Empty;
        public RememberedLoginState Load() => State;
        public void SaveFullLogin(string emailOrStudentCode, string password) => State = new RememberedLoginState(emailOrStudentCode, password, true);
        public void SaveEmailOnly(string emailOrStudentCode) => State = new RememberedLoginState(emailOrStudentCode, string.Empty, false);
        public void DowngradeToEmailOnly(string emailOrStudentCode) => State = new RememberedLoginState(emailOrStudentCode, string.Empty, false);
        public void Clear() => State = RememberedLoginState.Empty;
    }

    private sealed class StubLoginPrefillState : ILoginPrefillState
    {
        public string? Email { get; private set; }
        public void SetEmail(string email) => Email = email;
        public string? ConsumeEmail()
        {
            var value = Email;
            Email = null;
            return value;
        }
    }
}




