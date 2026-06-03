using System.Windows.Input;
using DormitoryManagement.Application.DTOs.Auth;
using DormitoryManagement.Application.Services.Auth;
using DormitoryManagement.WPF.Common;
using DormitoryManagement.WPF.Navigation;

namespace DormitoryManagement.WPF.ViewModels.Auth;

public sealed class RegisterViewModel : ViewModelBase
{
    private readonly IAccountRegistrationService _registrationService;
    private readonly INavigationService _navigationService;
    private readonly ILoginPrefillState _loginPrefillState;
    private readonly AsyncRelayCommand _registerCommand;
    private readonly AsyncRelayCommand _verifyOtpCommand;
    private readonly AsyncRelayCommand _resendOtpCommand;
    private readonly RelayCommand _backToLoginCommand;
    private string _fullName = string.Empty;
    private string _email = string.Empty;
    private string _username = string.Empty;
    private string _studentCode = string.Empty;
    private string? _phoneNumber;
    private string? _selectedGender;
    private DateTime? _dateOfBirth;
    private string _password = string.Empty;
    private string _confirmPassword = string.Empty;
    private string _otpCode = string.Empty;
    private string? _successMessage;
    private string? _otpMessage;
    private Guid? _pendingRegistrationId;

    public RegisterViewModel(
        IAccountRegistrationService registrationService,
        INavigationService navigationService,
        ILoginPrefillState loginPrefillState)
    {
        _registrationService = registrationService;
        _navigationService = navigationService;
        _loginPrefillState = loginPrefillState;
        _registerCommand = new AsyncRelayCommand(RegisterAsync, () => !IsBusy && !IsOtpStep);
        _verifyOtpCommand = new AsyncRelayCommand(VerifyOtpAsync, () => !IsBusy && IsOtpStep);
        _resendOtpCommand = new AsyncRelayCommand(ResendOtpAsync, () => !IsBusy && IsOtpStep);
        _backToLoginCommand = new RelayCommand(() => _navigationService.NavigateTo<LoginViewModel>(), () => !IsBusy);
        RegisterCommand = _registerCommand;
        VerifyOtpCommand = _verifyOtpCommand;
        ResendOtpCommand = _resendOtpCommand;
        BackToLoginCommand = _backToLoginCommand;
    }

    public event EventHandler? ClearPasswordRequested;

    public IReadOnlyList<string> GenderOptions { get; } = new[] { "Nam", "Nữ" };
    public ICommand RegisterCommand { get; }
    public ICommand VerifyOtpCommand { get; }
    public ICommand ResendOtpCommand { get; }
    public ICommand BackToLoginCommand { get; }

    public string FullName { get => _fullName; set => SetProperty(ref _fullName, value); }
    public string Email { get => _email; set => SetProperty(ref _email, value); }
    public string Username { get => _username; set => SetProperty(ref _username, value); }
    public string StudentCode { get => _studentCode; set => SetProperty(ref _studentCode, value); }
    public string? PhoneNumber { get => _phoneNumber; set => SetProperty(ref _phoneNumber, value); }
    public string? SelectedGender { get => _selectedGender; set => SetProperty(ref _selectedGender, value); }
    public DateTime? DateOfBirth { get => _dateOfBirth; set => SetProperty(ref _dateOfBirth, value); }
    public string Password { get => _password; set => SetProperty(ref _password, value); }
    public string ConfirmPassword { get => _confirmPassword; set => SetProperty(ref _confirmPassword, value); }
    public string OtpCode { get => _otpCode; set => SetProperty(ref _otpCode, value); }

    public string? SuccessMessage
    {
        get => _successMessage;
        private set
        {
            if (SetProperty(ref _successMessage, value))
            {
                OnPropertyChanged(nameof(HasSuccessMessage));
            }
        }
    }

    public string? OtpMessage
    {
        get => _otpMessage;
        private set
        {
            if (SetProperty(ref _otpMessage, value))
            {
                OnPropertyChanged(nameof(HasOtpMessage));
            }
        }
    }

    public bool HasSuccessMessage => !string.IsNullOrWhiteSpace(SuccessMessage);
    public bool HasOtpMessage => !string.IsNullOrWhiteSpace(OtpMessage);
    public bool IsOtpStep => _pendingRegistrationId.HasValue;
    public bool IsFormLocked => IsOtpStep;

    private async Task RegisterAsync()
    {
        ClearError();
        SuccessMessage = null;
        OtpMessage = null;
        var validationError = ValidateBasicFields();
        if (validationError is not null)
        {
            SetError(validationError);
            return;
        }

        IsBusy = true;
        RaiseCanExecuteChanged();
        try
        {
            var result = await _registrationService.StartStudentAccountRegistrationAsync(new RegisterAccountRequest
            {
                FullName = FullName,
                Email = Email,
                Username = Username,
                StudentCode = StudentCode,
                PhoneNumber = PhoneNumber,
                Gender = SelectedGender,
                DateOfBirth = DateOfBirth,
                Password = Password,
                ConfirmPassword = ConfirmPassword
            });

            if (!result.Succeeded)
            {
                SetError(WithDiagnostic(result.ErrorMessage ?? "Đăng ký tài khoản thất bại.", result.DiagnosticMessage));
                return;
            }

            _pendingRegistrationId = result.PendingRegistrationId;
            OtpCode = string.Empty;
            SuccessMessage = $"Mã xác minh đã được gửi tới {result.MaskedEmail}.";
            OtpMessage = FormatOtpMessage(result.ExpiresAtUtc, result.ResendAvailableAtUtc);
            RaiseStepChanged();
        }
        catch (Exception ex)
        {
            SetError(ex.Message);
        }
        finally
        {
            IsBusy = false;
            RaiseCanExecuteChanged();
        }
    }

    private async Task VerifyOtpAsync()
    {
        ClearError();
        SuccessMessage = null;
        if (!_pendingRegistrationId.HasValue)
        {
            SetError("Không tìm thấy phiên xác minh. Vui lòng gửi lại biểu mẫu đăng ký.");
            return;
        }

        if (string.IsNullOrWhiteSpace(OtpCode))
        {
            SetError("Vui lòng nhập mã xác minh.");
            return;
        }

        IsBusy = true;
        RaiseCanExecuteChanged();
        try
        {
            var result = await _registrationService.VerifyStudentAccountOtpAsync(_pendingRegistrationId.Value, OtpCode);
            if (!result.Succeeded)
            {
                SetError(result.ErrorMessage ?? "Xác minh thất bại.");
                return;
            }

            Password = string.Empty;
            ConfirmPassword = string.Empty;
            OtpCode = string.Empty;
            ClearPasswordRequested?.Invoke(this, EventArgs.Empty);
            _loginPrefillState.SetEmail(Email);
            SuccessMessage = "Tạo tài khoản thành công. Đang quay lại màn hình đăng nhập...";
            await Task.Delay(900);
            _navigationService.NavigateTo<LoginViewModel>();
        }
        catch (Exception ex)
        {
            SetError(ex.Message);
        }
        finally
        {
            IsBusy = false;
            RaiseCanExecuteChanged();
        }
    }

    private async Task ResendOtpAsync()
    {
        ClearError();
        SuccessMessage = null;
        if (!_pendingRegistrationId.HasValue)
        {
            SetError("Không tìm thấy phiên xác minh. Vui lòng gửi lại biểu mẫu đăng ký.");
            return;
        }

        IsBusy = true;
        RaiseCanExecuteChanged();
        try
        {
            var result = await _registrationService.ResendStudentAccountOtpAsync(_pendingRegistrationId.Value);
            if (!result.Succeeded)
            {
                SetError(WithDiagnostic(result.ErrorMessage ?? "Không thể gửi lại mã xác minh.", result.DiagnosticMessage));
                return;
            }

            OtpCode = string.Empty;
            SuccessMessage = $"Mã xác minh mới đã được gửi tới {result.MaskedEmail}.";
            OtpMessage = FormatOtpMessage(result.ExpiresAtUtc, result.ResendAvailableAtUtc);
        }
        catch (Exception ex)
        {
            SetError(ex.Message);
        }
        finally
        {
            IsBusy = false;
            RaiseCanExecuteChanged();
        }
    }

    private string? ValidateBasicFields()
    {
        if (string.IsNullOrWhiteSpace(FullName)) return "Vui lòng nhập họ và tên.";
        if (string.IsNullOrWhiteSpace(Email)) return "Vui lòng nhập email.";
        if (string.IsNullOrWhiteSpace(Username)) return "Vui lòng nhập tên đăng nhập.";
        if (string.IsNullOrWhiteSpace(StudentCode)) return "Vui lòng nhập mã sinh viên.";
        if (string.IsNullOrWhiteSpace(Password)) return "Vui lòng nhập mật khẩu.";
        if (Password.Length < 6) return "Mật khẩu phải có ít nhất 6 ký tự.";
        if (!string.Equals(Password, ConfirmPassword, StringComparison.Ordinal)) return "Mật khẩu xác nhận phải khớp với mật khẩu.";
        return null;
    }

    private static string FormatOtpMessage(DateTime? expiresAtUtc, DateTime? resendAvailableAtUtc)
    {
        var expiry = expiresAtUtc?.ToLocalTime().ToString("HH:mm") ?? "sớm";
        var resend = resendAvailableAtUtc?.ToLocalTime().ToString("HH:mm") ?? "sau khoảng một phút";
        return $"Mã sẽ hết hạn lúc {expiry}. Bạn có thể gửi lại sau {resend}.";
    }

    private static string WithDiagnostic(string message, string? diagnostic)
    {
        return string.IsNullOrWhiteSpace(diagnostic)
            ? message
            : $"{message}{Environment.NewLine}{diagnostic}";
    }

    private void RaiseStepChanged()
    {
        OnPropertyChanged(nameof(IsOtpStep));
        OnPropertyChanged(nameof(IsFormLocked));
        RaiseCanExecuteChanged();
    }

    private void RaiseCanExecuteChanged()
    {
        _registerCommand.RaiseCanExecuteChanged();
        _verifyOtpCommand.RaiseCanExecuteChanged();
        _resendOtpCommand.RaiseCanExecuteChanged();
        _backToLoginCommand.RaiseCanExecuteChanged();
    }
}
