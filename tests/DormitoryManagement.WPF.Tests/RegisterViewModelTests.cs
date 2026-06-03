using System.Windows.Input;
using DormitoryManagement.Application.DTOs.Auth;
using DormitoryManagement.Application.Services.Auth;
using DormitoryManagement.WPF.Common;
using DormitoryManagement.WPF.Navigation;
using DormitoryManagement.WPF.ViewModels.Auth;

namespace DormitoryManagement.WPF.Tests;

public sealed class RegisterViewModelTests
{
    [Fact]
    public void Consent_gate_blocks_registration_until_terms_are_accepted()
    {
        var service = new RecordingRegistrationService();
        var viewModel = CreateViewModel(service: service);
        PopulateValidForm(viewModel);

        Assert.False(viewModel.CanSubmitRegistration);
        Assert.False(viewModel.RegisterCommand.CanExecute(null));

        viewModel.AcceptsTerms = true;

        Assert.True(viewModel.CanSubmitRegistration);
        Assert.True(viewModel.RegisterCommand.CanExecute(null));
    }

    [Fact]
    public void Successful_registration_switches_to_otp_step_and_enables_verify_and_resend()
    {
        var service = new RecordingRegistrationService();
        var viewModel = CreateViewModel(service: service);
        PopulateValidForm(viewModel);
        viewModel.AcceptsTerms = true;

        viewModel.RegisterCommand.Execute(null);

        WaitUntil(() => service.StartCalled);
        Assert.True(viewModel.IsOtpStep);
        Assert.True(viewModel.ShowOtpSection);
        Assert.True(viewModel.IsFormLocked);
        Assert.False(viewModel.CanSubmitRegistration);
        Assert.True(viewModel.CanVerifyOtp);
        Assert.True(viewModel.CanResendOtp);
        Assert.Contains("Mã xác minh đã được gửi", viewModel.SuccessMessage);
        Assert.Equal("Xác minh email", viewModel.OtpSectionTitle);
    }

    [Fact]
    public void Successful_verification_clears_otp_step_prefills_login_and_navigates_back()
    {
        var service = new RecordingRegistrationService();
        var navigation = new RecordingNavigationService();
        var prefill = new StubLoginPrefillState();
        var viewModel = CreateViewModel(service, navigation, prefill);
        PopulateValidForm(viewModel);
        viewModel.AcceptsTerms = true;

        viewModel.RegisterCommand.Execute(null);
        WaitUntil(() => viewModel.IsOtpStep);

        viewModel.OtpCode = "123456";
        viewModel.VerifyOtpCommand.Execute(null);

        WaitUntil(() => service.VerifyCalled);
        WaitUntil(() => navigation.LastViewModelType == typeof(LoginViewModel));

        Assert.False(viewModel.IsOtpStep);
        Assert.False(viewModel.ShowOtpSection);
        Assert.Equal("student@example.edu.vn", prefill.Email);
        Assert.Contains("Tạo tài khoản thành công", viewModel.SuccessMessage);
    }

    [Fact]
    public void Resend_and_verify_commands_remain_unavailable_before_otp_step()
    {
        var viewModel = CreateViewModel();

        Assert.False(viewModel.VerifyOtpCommand.CanExecute(null));
        Assert.False(viewModel.ResendOtpCommand.CanExecute(null));
    }

    [Fact]
    public void Back_to_login_handoff_remains_available_from_registration_route()
    {
        var navigation = new RecordingNavigationService();
        var viewModel = CreateViewModel(navigation: navigation);

        viewModel.BackToLoginCommand.Execute(null);

        Assert.Equal(typeof(LoginViewModel), navigation.LastViewModelType);
    }

    private static RegisterViewModel CreateViewModel(
        RecordingRegistrationService? service = null,
        RecordingNavigationService? navigation = null,
        StubLoginPrefillState? prefill = null)
    {
        return new RegisterViewModel(
            service ?? new RecordingRegistrationService(),
            navigation ?? new RecordingNavigationService(),
            prefill ?? new StubLoginPrefillState());
    }

    private static void PopulateValidForm(RegisterViewModel viewModel)
    {
        viewModel.FullName = "Nguyễn Văn A";
        viewModel.StudentCode = "20230001";
        viewModel.Username = "nguyenvana";
        viewModel.DateOfBirth = new DateTime(2004, 1, 1);
        viewModel.SelectedGender = "Nam";
        viewModel.PhoneNumber = "0901234567";
        viewModel.Email = "student@example.edu.vn";
        viewModel.Password = "123456";
        viewModel.ConfirmPassword = "123456";
    }

    private static void WaitUntil(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow.AddSeconds(3);
        while (!condition() && DateTime.UtcNow < deadline)
        {
            Thread.Sleep(10);
        }

        Assert.True(condition());
    }

    private sealed class RecordingRegistrationService : IAccountRegistrationService
    {
        public bool StartCalled { get; private set; }
        public bool VerifyCalled { get; private set; }
        public bool ResendCalled { get; private set; }

        public Task<StartAccountRegistrationResult> StartStudentAccountRegistrationAsync(RegisterAccountRequest request, CancellationToken ct = default)
        {
            StartCalled = true;
            return Task.FromResult(StartAccountRegistrationResult.Success(Guid.NewGuid(), "s****@example.edu.vn", DateTime.UtcNow.AddMinutes(5), DateTime.UtcNow.AddMinutes(1)));
        }

        public Task<RegisterAccountResult> VerifyStudentAccountOtpAsync(Guid pendingRegistrationId, string otpCode, CancellationToken ct = default)
        {
            VerifyCalled = true;
            return Task.FromResult(RegisterAccountResult.Success(Guid.NewGuid(), Guid.NewGuid()));
        }

        public Task<StartAccountRegistrationResult> ResendStudentAccountOtpAsync(Guid pendingRegistrationId, CancellationToken ct = default)
        {
            ResendCalled = true;
            return Task.FromResult(StartAccountRegistrationResult.Success(pendingRegistrationId, "s****@example.edu.vn", DateTime.UtcNow.AddMinutes(5), DateTime.UtcNow.AddMinutes(1)));
        }
    }

    private sealed class RecordingNavigationService : INavigationService
    {
        public Type? LastViewModelType { get; private set; }

        public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase
        {
            LastViewModelType = typeof(TViewModel);
        }
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
