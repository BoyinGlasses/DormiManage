using System.Collections.ObjectModel;
using System.Windows.Input;
using DormitoryManagement.Application.DTOs.Registrations;
using DormitoryManagement.Application.Services.Registrations;
using DormitoryManagement.WPF.Common;
using Microsoft.Extensions.DependencyInjection;

namespace DormitoryManagement.WPF.ViewModels.Registrations;

public sealed class RegistrationApprovalViewModel : ViewModelBase
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly AsyncRelayCommand _approveCommand;
    private readonly RelayCommand _openRejectDialogCommand;
    private readonly AsyncRelayCommand _confirmRejectCommand;
    private bool _hasLoaded;
    private RegistrationApprovalRow? _selectedRegistration;
    private DateTime? _startDate;
    private string _rejectReason = string.Empty;
    private bool _isRejectDialogOpen;
    private string? _actionMessage;
    private string? _rejectReasonError;

    public RegistrationApprovalViewModel(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
        LoadCommand = new AsyncRelayCommand(LoadAsync);
        _approveCommand = new AsyncRelayCommand(ApproveAsync, () => CanApproveRegistration);
        _openRejectDialogCommand = new RelayCommand(OpenRejectDialog, () => CanRejectRegistration);
        _confirmRejectCommand = new AsyncRelayCommand(RejectAsync, () => CanRejectRegistration);
        ApproveCommand = _approveCommand;
        OpenRejectDialogCommand = _openRejectDialogCommand;
        ConfirmRejectCommand = _confirmRejectCommand;
        CancelRejectCommand = new RelayCommand(CancelReject);
        StartDate = DateTime.Today;
    }

    public ObservableCollection<RegistrationApprovalRow> Registrations { get; } = new();
    public ObservableCollection<RegistrationApprovalRow> PendingRegistrations => Registrations;
    public ICommand LoadCommand { get; }
    public ICommand ApproveCommand { get; }
    public ICommand OpenRejectDialogCommand { get; }
    public ICommand ConfirmRejectCommand { get; }
    public ICommand CancelRejectCommand { get; }
    public bool HasRegistrations => Registrations.Count > 0;
    public bool IsRegistrationsEmpty => _hasLoaded && !IsBusy && Registrations.Count == 0;
    public bool HasSelection => SelectedRegistration is not null;
    public bool CanApproveRegistration => SelectedRegistration is not null && StartDate is not null && !IsBusy;
    public bool CanRejectRegistration => SelectedRegistration is not null && !IsBusy;

    public RegistrationApprovalRow? SelectedRegistration
    {
        get => _selectedRegistration;
        set
        {
            if (SetProperty(ref _selectedRegistration, value))
            {
                ActionMessage = null;
                NotifyUiState();
            }
        }
    }

    public DateTime? StartDate
    {
        get => _startDate;
        set
        {
            if (SetProperty(ref _startDate, value))
            {
                NotifyUiState();
            }
        }
    }

    public string RejectReason
    {
        get => _rejectReason;
        set
        {
            if (SetProperty(ref _rejectReason, value))
            {
                RejectReasonError = null;
            }
        }
    }

    public bool IsRejectDialogOpen
    {
        get => _isRejectDialogOpen;
        set => SetProperty(ref _isRejectDialogOpen, value);
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

    public bool HasActionMessage => !string.IsNullOrWhiteSpace(ActionMessage);

    public string? RejectReasonError
    {
        get => _rejectReasonError;
        private set
        {
            if (SetProperty(ref _rejectReasonError, value)) OnPropertyChanged(nameof(HasRejectReasonError));
        }
    }

    public bool HasRejectReasonError => !string.IsNullOrWhiteSpace(RejectReasonError);

    private async Task LoadAsync()
    {
        IsBusy = true;
        ClearError();
        NotifyUiState();
        try
        {
            Registrations.Clear();
            using var scope = _scopeFactory.CreateScope();
            var registrationService = scope.ServiceProvider.GetRequiredService<IRoomRegistrationService>();
            var registrations = await registrationService.GetPendingRegistrationsAsync();
            foreach (var registration in registrations)
            {
                Registrations.Add(new RegistrationApprovalRow
                {
                    Id = registration.Id,
                    RegistrationCode = registration.Id.ToString("N")[..8].ToUpperInvariant(),
                    StudentCode = registration.StudentCode,
                    StudentName = registration.StudentName,
                    BuildingName = registration.BuildingName,
                    PreferredRoom = registration.RoomNumber,
                    TermMonths = 12,
                    StartDate = DateTime.Today,
                    RequestedAt = registration.RequestedAt,
                    Status = registration.Status.ToString()
                });
            }

            SelectedRegistration = null;
            _hasLoaded = true;
        }
        catch (Exception ex)
        {
            SetError(ex.Message);
        }
        finally
        {
            IsBusy = false;
            NotifyUiState();
        }
    }

    private async Task ApproveAsync()
    {
        ClearError();
        ActionMessage = null;
        if (SelectedRegistration is null)
        {
            SetError("Select a pending registration before approving.");
            return;
        }

        if (StartDate is null)
        {
            SetError("Choose a contract start date before approving.");
            return;
        }

        IsBusy = true;
        NotifyUiState();
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var registrationService = scope.ServiceProvider.GetRequiredService<IRoomRegistrationService>();
            await registrationService.ApproveRegistrationAsync(new ApproveRoomRegistrationRequest
            {
                RegistrationId = SelectedRegistration.Id,
                StartDate = StartDate.Value
            });
            ActionMessage = "Registration approved.";
            await LoadAsync();
        }
        catch (Exception ex)
        {
            SetError(ex.Message);
            await LoadAsync();
            SetError(ex.Message);
        }
        finally
        {
            IsBusy = false;
            NotifyUiState();
        }
    }

    private void OpenRejectDialog()
    {
        ClearError();
        if (SelectedRegistration is null)
        {
            SetError("Select a pending registration before rejecting.");
            return;
        }

        RejectReason = string.Empty;
        RejectReasonError = null;
        IsRejectDialogOpen = true;
        NotifyUiState();
    }

    private void CancelReject()
    {
        IsRejectDialogOpen = false;
        RejectReason = string.Empty;
        RejectReasonError = null;
        NotifyUiState();
    }

    private async Task RejectAsync()
    {
        ClearError();
        ActionMessage = null;
        RejectReasonError = null;
        if (SelectedRegistration is null)
        {
            SetError("Select a pending registration before rejecting.");
            IsRejectDialogOpen = false;
            return;
        }

        if (string.IsNullOrWhiteSpace(RejectReason))
        {
            RejectReasonError = "Enter a rejection reason.";
            return;
        }

        IsBusy = true;
        NotifyUiState();
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var registrationService = scope.ServiceProvider.GetRequiredService<IRoomRegistrationService>();
            await registrationService.RejectRegistrationAsync(new RejectRoomRegistrationRequest
            {
                RegistrationId = SelectedRegistration.Id,
                Reason = RejectReason.Trim()
            });
            ActionMessage = "Registration rejected.";
            IsRejectDialogOpen = false;
            await LoadAsync();
        }
        catch (Exception ex)
        {
            SetError(ex.Message);
            await LoadAsync();
            SetError(ex.Message);
        }
        finally
        {
            IsBusy = false;
            NotifyUiState();
        }
    }

    private void NotifyUiState()
    {
        OnPropertyChanged(nameof(HasRegistrations));
        OnPropertyChanged(nameof(IsRegistrationsEmpty));
        OnPropertyChanged(nameof(HasSelection));
        OnPropertyChanged(nameof(CanApproveRegistration));
        OnPropertyChanged(nameof(CanRejectRegistration));
        _approveCommand.RaiseCanExecuteChanged();
        _openRejectDialogCommand.RaiseCanExecuteChanged();
        _confirmRejectCommand.RaiseCanExecuteChanged();
    }
}

public sealed class RegistrationApprovalRow
{
    public Guid Id { get; set; }
    public string RegistrationCode { get; set; } = string.Empty;
    public string StudentCode { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string BuildingName { get; set; } = string.Empty;
    public string PreferredRoom { get; set; } = string.Empty;
    public int TermMonths { get; set; }
    public DateTime StartDate { get; set; } = DateTime.Today;
    public DateTime RequestedAt { get; set; }
    public string Note { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
}
