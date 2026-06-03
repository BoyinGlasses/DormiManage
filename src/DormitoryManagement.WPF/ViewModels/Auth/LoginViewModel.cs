using System.Windows.Input;
using DormitoryManagement.Application.Abstractions.Auth;
using DormitoryManagement.Application.DTOs.Auth;
using DormitoryManagement.Application.Services.Auth;
using DormitoryManagement.Domain.Constants;
using DormitoryManagement.WPF.Common;
using DormitoryManagement.WPF.Navigation;
using DormitoryManagement.WPF.ViewModels.Dashboard;
using DormitoryManagement.WPF.ViewModels.SupportTickets;

namespace DormitoryManagement.WPF.ViewModels.Auth;

public sealed class LoginViewModel : ViewModelBase
{
    private readonly IAuthService _authService;
    private readonly INavigationService _navigationService;
    private readonly SessionState _sessionState;
    private readonly IRememberedLoginService _rememberedLoginService;
    private readonly ILoginPrefillState _loginPrefillState;
    private string _emailOrStudentCode = string.Empty;
    private string _password = string.Empty;
    private bool _rememberMe;
    private string? _accountError;
    private string? _passwordError;

    public LoginViewModel(
        IAuthService authService,
        INavigationService navigationService,
        SessionState sessionState,
        IRememberedLoginService rememberedLoginService,
        ILoginPrefillState loginPrefillState)
    {
        _authService = authService;
        _navigationService = navigationService;
        _sessionState = sessionState;
        _rememberedLoginService = rememberedLoginService;
        _loginPrefillState = loginPrefillState;
        LoginCommand = new AsyncRelayCommand(LoginAsync, () => !IsBusy);
        CreateAccountCommand = new RelayCommand(() => _navigationService.NavigateTo<RegisterViewModel>());
        LoadInitialFields();
    }

    public event EventHandler? ClearPasswordRequested;

    public string EmailOrStudentCode
    {
        get => _emailOrStudentCode;
        set
        {
            if (SetProperty(ref _emailOrStudentCode, value))
            {
                AccountError = null;
            }
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            if (SetProperty(ref _password, value))
            {
                PasswordError = null;
            }
        }
    }

    public bool RememberMe
    {
        get => _rememberMe;
        set => SetProperty(ref _rememberMe, value);
    }

    public string? AccountError
    {
        get => _accountError;
        private set
        {
            if (SetProperty(ref _accountError, value))
            {
                OnPropertyChanged(nameof(HasAccountError));
            }
        }
    }

    public string? PasswordError
    {
        get => _passwordError;
        private set
        {
            if (SetProperty(ref _passwordError, value))
            {
                OnPropertyChanged(nameof(HasPasswordError));
            }
        }
    }

    public bool HasAccountError => !string.IsNullOrWhiteSpace(AccountError);
    public bool HasPasswordError => !string.IsNullOrWhiteSpace(PasswordError);
    public string SubmitButtonText => IsBusy ? "Đang đăng nhập..." : "Đăng nhập vào Bảng điều khiển";
    public ICommand LoginCommand { get; }
    public ICommand CreateAccountCommand { get; }

    private async Task LoginAsync()
    {
        ClearError();
        AccountError = null;
        PasswordError = null;

        if (string.IsNullOrWhiteSpace(EmailOrStudentCode))
        {
            AccountError = "Nhập mã sinh viên.";
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            PasswordError = "Nhập mật khẩu.";
        }

        if (HasAccountError || HasPasswordError)
        {
            return;
        }

        IsBusy = true;
        OnPropertyChanged(nameof(SubmitButtonText));
        try
        {
            var result = await _authService.LoginAsync(new LoginRequest
            {
                EmailOrUsernameOrStudentCode = EmailOrStudentCode.Trim(),
                Password = Password
            });

            if (!result.Succeeded)
            {
                ApplyLoginFailure(result);
                return;
            }

            var rememberedAccount = result.User?.Email ?? EmailOrStudentCode.Trim();
            if (RememberMe)
            {
                _rememberedLoginService.SaveFullLogin(rememberedAccount, Password);
            }
            else
            {
                _rememberedLoginService.Clear();
            }

            _sessionState.NotifyChanged();
            if (result.User?.RoleName.Equals(RoleNames.Student, StringComparison.OrdinalIgnoreCase) == true)
            {
                _navigationService.NavigateTo<StudentDashboardViewModel>();
            }
            else if (result.User?.RoleName.Equals(RoleNames.Staff, StringComparison.OrdinalIgnoreCase) == true)
            {
                _navigationService.NavigateTo<SupportTicketListViewModel>();
            }
            else
            {
                _navigationService.NavigateTo<AdminDashboardViewModel>();
            }
        }
        catch (Exception ex)
        {
            SetError(ex.Message);
        }
        finally
        {
            IsBusy = false;
            OnPropertyChanged(nameof(SubmitButtonText));
        }
    }

    private void LoadInitialFields()
    {
        var prefilledEmail = _loginPrefillState.ConsumeEmail();
        if (!string.IsNullOrWhiteSpace(prefilledEmail))
        {
            EmailOrStudentCode = prefilledEmail;
            Password = string.Empty;
            RememberMe = false;
            return;
        }

        var remembered = _rememberedLoginService.Load();
        if (!remembered.HasEmailOrStudentCode)
        {
            return;
        }

        EmailOrStudentCode = remembered.EmailOrStudentCode;
        RememberMe = true;
        if (remembered.HasPassword)
        {
            Password = remembered.Password;
        }
    }

    private void ApplyLoginFailure(LoginResult result)
    {
        switch (result.FailureReason)
        {
            case LoginFailureReason.AccountNotFound:
                AccountError = "Mã sinh viên hoặc email không tồn tại.";
                Password = string.Empty;
                ClearPasswordRequested?.Invoke(this, EventArgs.Empty);
                break;
            case LoginFailureReason.InvalidPassword:
                PasswordError = "Mật khẩu không đúng.";
                break;
            case LoginFailureReason.Disabled:
            case LoginFailureReason.Locked:
                AccountError = result.ErrorMessage ?? "Tài khoản hiện không thể đăng nhập.";
                break;
            default:
                SetError(result.ErrorMessage ?? "Đăng nhập thất bại. Vui lòng thử lại.");
                break;
        }
    }
}
