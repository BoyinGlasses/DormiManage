using System.Collections.ObjectModel;
using System.Windows.Input;
using DormitoryManagement.Application.Abstractions.Auth;
using DormitoryManagement.Application.DTOs.Billing;
using DormitoryManagement.Application.DTOs.Payments;
using DormitoryManagement.Application.Services.Payments;
using DormitoryManagement.Domain.Constants;
using DormitoryManagement.Domain.Enums;
using DormitoryManagement.WPF.Common;

namespace DormitoryManagement.WPF.ViewModels.Billing;

public sealed class PaymentViewModel : ViewModelBase
{
    private readonly IPaymentService _service;
    private readonly IPaymentExtensionService? _extensionService;
    private readonly ICurrentUserService _currentUser;
    private readonly PaymentNavigationState? _paymentNavigationState;
    private readonly AsyncRelayCommand _createPaymentCommand;
    private readonly AsyncRelayCommand _confirmPaymentCommand;
    private readonly AsyncRelayCommand _requestExtensionCommand;
    private readonly AsyncRelayCommand _refreshQrStatusCommand;
    private bool _hasLoaded;
    private OutstandingInvoiceDto? _selectedInvoice;
    private InvoicePaymentQrDto? _selectedInvoiceQr;
    private PaymentDto? _selectedPendingPayment;
    private decimal _amount;
    private PaymentMethod _method = PaymentMethod.QrBanking;
    private string _confirmationReference = string.Empty;
    private DateTime? _extensionRequestedDueDate;
    private DateTime? _extensionMaxDueDate;
    private string _extensionReason = string.Empty;
    private string? _successMessage;
    private string? _amountError;
    private string? _extensionDueDateError;
    private string? _qrStatusMessage;
    private PaymentNavigationContextDto? _paymentContext;
    private PaymentNavigationContextDto? _extensionContext;

    public PaymentViewModel(
        IPaymentService service,
        ICurrentUserService currentUser,
        IPaymentExtensionService? extensionService = null,
        PaymentNavigationState? paymentNavigationState = null)
    {
        _service = service;
        _extensionService = extensionService;
        _currentUser = currentUser;
        _paymentNavigationState = paymentNavigationState;
        LoadCommand = new AsyncRelayCommand(LoadAsync);
        _createPaymentCommand = new AsyncRelayCommand(CreatePaymentAsync, () => CanCreatePayment);
        _confirmPaymentCommand = new AsyncRelayCommand(ConfirmPaymentAsync, () => CanConfirmPayment);
        _requestExtensionCommand = new AsyncRelayCommand(RequestExtensionAsync, () => CanRequestExtension);
        _refreshQrStatusCommand = new AsyncRelayCommand(RefreshQrStatusAsync, () => CanRefreshQrStatus);
        CreatePaymentCommand = _createPaymentCommand;
        ConfirmPaymentCommand = _confirmPaymentCommand;
        RequestExtensionCommand = _requestExtensionCommand;
        RefreshQrStatusCommand = _refreshQrStatusCommand;
    }

    public ObservableCollection<OutstandingInvoiceDto> OutstandingInvoices { get; } = new();
    public ObservableCollection<PaymentDto> Payments { get; } = new();
    public Array PaymentMethods => Enum.GetValues<PaymentMethod>();
    public ICommand LoadCommand { get; }
    public ICommand RefreshCommand => LoadCommand;
    public ICommand CreatePaymentCommand { get; }
    public ICommand ConfirmPaymentCommand { get; }
    public ICommand RequestExtensionCommand { get; }
    public ICommand RefreshQrStatusCommand { get; }
    public bool IsAdmin => _currentUser.IsInRole(RoleNames.Admin)
        || _currentUser.IsInRole(RoleNames.Manager);
    public bool IsStudent => _currentUser.IsInRole(RoleNames.Student);
    public bool HasOutstandingInvoices => OutstandingInvoices.Count > 0;
    public bool IsOutstandingInvoicesEmpty => _hasLoaded && !IsBusy && IsStudent && OutstandingInvoices.Count == 0;
    public bool HasPendingPayments => Payments.Count > 0;
    public bool IsPendingPaymentsEmpty => _hasLoaded && !IsBusy && IsAdmin && Payments.Count == 0;
    public bool CanCreatePayment => IsStudent
        && SelectedInvoice is not null
        && Amount == SelectedInvoice.RemainingAmount
        && Amount > 0m
        && !IsBusy;
    public bool CanConfirmPayment => IsAdmin && SelectedPendingPayment is not null && !IsBusy;
    public bool CanRefreshQrStatus => IsStudent && SelectedInvoice is not null && !IsBusy;
    public bool CanRequestExtension => IsStudent
        && _extensionService is not null
        && SelectedInvoice?.InvoiceKind == InvoiceKind.MonthlyUtility
        && SelectedInvoice.Status != InvoiceStatus.Paid
        && SelectedInvoice.RemainingAmount > 0m
        && ExtensionRequestedDueDate.HasValue
        && (!ExtensionMaxDueDate.HasValue || ExtensionRequestedDueDate.Value.Date <= ExtensionMaxDueDate.Value.Date)
        && !string.IsNullOrWhiteSpace(ExtensionReason)
        && !IsBusy;

    public OutstandingInvoiceDto? SelectedInvoice
    {
        get => _selectedInvoice;
        set
        {
            if (SetProperty(ref _selectedInvoice, value))
            {
                Amount = value?.RemainingAmount ?? Amount;
                OnPropertyChanged(nameof(InvoiceNumber));
                OnPropertyChanged(nameof(InvoiceRemaining));
                OnPropertyChanged(nameof(InvoiceRemainingText));
                OnPropertyChanged(nameof(SelectedInvoiceKind));
                OnPropertyChanged(nameof(SelectedInvoiceRoomNumber));
                SelectedInvoiceQr = null;
                QrStatusMessage = value is null ? null : "Loading QR payment details...";
                ExtensionMaxDueDate = value is { InvoiceKind: InvoiceKind.MonthlyUtility }
                    ? CalculateMaxExtensionDueDate(value)
                    : null;
                NotifyUiState();
            }
        }
    }

    public PaymentDto? SelectedPendingPayment
    {
        get => _selectedPendingPayment;
        set
        {
            if (SetProperty(ref _selectedPendingPayment, value))
            {
                NotifyUiState();
            }
        }
    }

    public string InvoiceNumber => SelectedInvoice?.InvoiceNumber ?? "No invoice selected";
    public InvoiceKind? SelectedInvoiceKind => SelectedInvoice?.InvoiceKind;
    public string SelectedInvoiceRoomNumber => MatchingContext?.RoomNumber ?? string.Empty;
    public decimal InvoiceRemaining => SelectedInvoice?.RemainingAmount ?? 0m;
    public decimal Amount
    {
        get => _amount;
        set
        {
            if (SetProperty(ref _amount, value))
            {
                AmountError = null;
                NotifyUiState();
            }
        }
    }

    public PaymentMethod Method { get => _method; set => SetProperty(ref _method, value); }
    public string InvoiceRemainingText => InvoiceRemaining.ToString("N0") + " VND";

    public InvoicePaymentQrDto? SelectedInvoiceQr
    {
        get => _selectedInvoiceQr;
        private set
        {
            if (SetProperty(ref _selectedInvoiceQr, value))
            {
                NotifyQrState();
            }
        }
    }

    public bool HasQrPaymentDetails => SelectedInvoiceQr is not null && !string.IsNullOrWhiteSpace(SelectedInvoiceQr.TransferContent);
    public bool HasQrImage => SelectedInvoiceQr is not null && !string.IsNullOrWhiteSpace(SelectedInvoiceQr.QrDataUrl);
    public bool IsQrMissing => SelectedInvoice is not null && !HasQrPaymentDetails && !IsBusy;
    public string QrTransferContent => SelectedInvoiceQr?.TransferContent ?? "-";
    public string QrDataUrl => SelectedInvoiceQr?.QrDataUrl ?? string.Empty;
    public string QrAmountText => (SelectedInvoiceQr?.Amount ?? InvoiceRemaining).ToString("N0") + " VND";
    public string QrStatus => SelectedInvoiceQr?.Status ?? SelectedInvoice?.Status.ToString() ?? "-";
    public string QrDueDateText => (SelectedInvoiceQr?.DueDate ?? SelectedInvoice?.DueDate)?.ToString("d") ?? "-";
    public string QrPaidAtText => SelectedInvoiceQr?.PaidAt?.ToString("g") ?? "-";
    public bool HasQrPaidAt => SelectedInvoiceQr?.PaidAt is not null;

    public string? QrStatusMessage
    {
        get => _qrStatusMessage;
        private set
        {
            if (SetProperty(ref _qrStatusMessage, value))
            {
                OnPropertyChanged(nameof(HasQrStatusMessage));
            }
        }
    }

    public bool HasQrStatusMessage => !string.IsNullOrWhiteSpace(QrStatusMessage);

    public string ConfirmationReference
    {
        get => _confirmationReference;
        set => SetProperty(ref _confirmationReference, value);
    }

    public DateTime? ExtensionRequestedDueDate
    {
        get => _extensionRequestedDueDate;
        set
        {
            if (SetProperty(ref _extensionRequestedDueDate, value))
            {
                ExtensionDueDateError = null;
                NotifyUiState();
            }
        }
    }

    public DateTime? ExtensionMaxDueDate
    {
        get => _extensionMaxDueDate;
        private set
        {
            if (SetProperty(ref _extensionMaxDueDate, value))
            {
                OnPropertyChanged(nameof(HasExtensionMaxDueDate));
                NotifyUiState();
            }
        }
    }

    public bool HasExtensionMaxDueDate => ExtensionMaxDueDate.HasValue;

    public string ExtensionReason
    {
        get => _extensionReason;
        set
        {
            if (SetProperty(ref _extensionReason, value ?? string.Empty))
            {
                NotifyUiState();
            }
        }
    }

    public string? AmountError
    {
        get => _amountError;
        private set
        {
            if (SetProperty(ref _amountError, value)) OnPropertyChanged(nameof(HasAmountError));
        }
    }

    public bool HasAmountError => !string.IsNullOrWhiteSpace(AmountError);
    public string? ExtensionDueDateError
    {
        get => _extensionDueDateError;
        private set
        {
            if (SetProperty(ref _extensionDueDateError, value)) OnPropertyChanged(nameof(HasExtensionDueDateError));
        }
    }

    public bool HasExtensionDueDateError => !string.IsNullOrWhiteSpace(ExtensionDueDateError);

    public PaymentNavigationContextDto? PaymentContext
    {
        get => _paymentContext;
        private set
        {
            if (SetProperty(ref _paymentContext, value))
            {
                OnPropertyChanged(nameof(SelectedInvoiceRoomNumber));
            }
        }
    }

    public PaymentNavigationContextDto? ExtensionContext
    {
        get => _extensionContext;
        private set
        {
            if (SetProperty(ref _extensionContext, value))
            {
                OnPropertyChanged(nameof(SelectedInvoiceRoomNumber));
            }
        }
    }

    private PaymentNavigationContextDto? MatchingContext =>
        SelectedInvoice is null
            ? null
            : ExtensionContext?.InvoiceId == SelectedInvoice.Id
                ? ExtensionContext
                : PaymentContext?.InvoiceId == SelectedInvoice.Id
                    ? PaymentContext
                    : null;

    public string? SuccessMessage
    {
        get => _successMessage;
        private set
        {
            if (SetProperty(ref _successMessage, value)) OnPropertyChanged(nameof(HasSuccessMessage));
        }
    }

    public bool HasSuccessMessage => !string.IsNullOrWhiteSpace(SuccessMessage);

    private async Task LoadAsync()
    {
        ClearError();
        IsBusy = true;
        NotifyUiState();
        try
        {
            OutstandingInvoices.Clear();
            Payments.Clear();

            if (IsStudent)
            {
                var invoices = await _service.GetOutstandingInvoicesAsync();
                foreach (var invoice in invoices)
                {
                    OutstandingInvoices.Add(invoice);
                }

                SelectedInvoice = OutstandingInvoices.FirstOrDefault();
                ApplyStoredNavigationState();
                ApplyPendingContexts();
                await LoadSelectedInvoiceQrAsync();
            }

            if (IsAdmin)
            {
                var payments = await _service.GetPendingPaymentsAsync();
                foreach (var payment in payments)
                {
                    Payments.Add(payment);
                }

                SelectedPendingPayment = Payments.FirstOrDefault();
            }

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

    public void ApplyPaymentContext(PaymentNavigationContextDto context)
    {
        PaymentContext = context;
        if (!TrySelectInvoice(context) && _hasLoaded)
        {
            SetError("Selected invoice is not available for payment.");
        }
    }

    public void ApplyExtensionContext(PaymentNavigationContextDto context)
    {
        ExtensionContext = context;
        if (TrySelectInvoice(context))
        {
            ExtensionRequestedDueDate = ExtensionMaxDueDate ?? SelectedInvoice?.DueDate.Date;
        }
        else if (_hasLoaded)
        {
            SetError("Selected invoice is not available for extension.");
        }
    }

    private async Task CreatePaymentAsync()
    {
        ClearError();
        SuccessMessage = null;
        AmountError = null;
        if (SelectedInvoice is null)
        {
            SetError("Select an outstanding invoice before creating a payment.");
            return;
        }

        if (Amount <= 0)
        {
            AmountError = "Payment amount must be greater than zero.";
        }

        if (Amount != SelectedInvoice.RemainingAmount)
        {
            AmountError = "Payment amount must match the full invoice balance.";
        }

        if (HasAmountError)
        {
            return;
        }

        IsBusy = true;
        NotifyUiState();
        try
        {
            var payment = await _service.CreatePaymentAsync(new CreatePaymentRequest
            {
                InvoiceId = SelectedInvoice.Id,
                Amount = Amount,
                Method = Method
            });
            Payments.Insert(0, payment);
            SuccessMessage = $"Payment {payment.PaymentCode} created with status {payment.Status}.";
            await LoadAsync();
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

    private async Task RefreshQrStatusAsync()
    {
        ClearError();
        SuccessMessage = null;
        IsBusy = true;
        NotifyUiState();
        try
        {
            await LoadSelectedInvoiceQrAsync();
            SuccessMessage = "Payment status refreshed.";
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

    private async Task RequestExtensionAsync()
    {
        ClearError();
        SuccessMessage = null;
        ExtensionDueDateError = null;
        if (_extensionService is null || SelectedInvoice is null || ExtensionRequestedDueDate is null)
        {
            SetError("Select a monthly utility invoice before requesting an extension.");
            return;
        }

        if (SelectedInvoice.InvoiceKind != InvoiceKind.MonthlyUtility
            || SelectedInvoice.Status == InvoiceStatus.Paid
            || SelectedInvoice.RemainingAmount <= 0m)
        {
            SetError("Select an unpaid monthly utility invoice before requesting an extension.");
            return;
        }

        if (ExtensionMaxDueDate.HasValue && ExtensionRequestedDueDate.Value.Date > ExtensionMaxDueDate.Value.Date)
        {
            ExtensionDueDateError = $"Extension due date cannot be later than {ExtensionMaxDueDate.Value:d}.";
            return;
        }

        IsBusy = true;
        NotifyUiState();
        try
        {
            var extension = await _extensionService.RequestExtensionAsync(new CreatePaymentExtensionRequest
            {
                InvoiceId = SelectedInvoice.Id,
                RequestedDueDate = ExtensionRequestedDueDate.Value,
                Reason = ExtensionReason
            });
            SuccessMessage = $"Extension request {extension.Status}.";
            ExtensionReason = string.Empty;
            await LoadAsync();
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

    private async Task ConfirmPaymentAsync()
    {
        ClearError();
        SuccessMessage = null;
        if (SelectedPendingPayment is null)
        {
            SetError("Select a pending payment before confirming.");
            return;
        }

        IsBusy = true;
        NotifyUiState();
        try
        {
            var payment = await _service.ConfirmPaymentAsync(new ConfirmPaymentRequest
            {
                PaymentId = SelectedPendingPayment.Id,
                TransactionRef = string.IsNullOrWhiteSpace(ConfirmationReference)
                    ? $"ADMIN-{DateTime.UtcNow:yyyyMMddHHmmss}"
                    : ConfirmationReference.Trim()
            });
            SuccessMessage = $"Payment {payment.PaymentCode} confirmed.";
            ConfirmationReference = string.Empty;
            await LoadAsync();
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

    private void ApplyPendingContexts()
    {
        if (PaymentContext is not null)
        {
            TrySelectInvoice(PaymentContext);
        }

        if (ExtensionContext is not null && TrySelectInvoice(ExtensionContext))
        {
            ExtensionRequestedDueDate = ExtensionMaxDueDate ?? SelectedInvoice?.DueDate.Date;
        }
    }

    private void ApplyStoredNavigationState()
    {
        if (_paymentNavigationState?.PaymentContext is { } paymentContext)
        {
            PaymentContext = paymentContext;
        }

        if (_paymentNavigationState?.ExtensionContext is { } extensionContext)
        {
            ExtensionContext = extensionContext;
        }

        _paymentNavigationState?.Clear();
    }

    private bool TrySelectInvoice(PaymentNavigationContextDto context)
    {
        var invoice = OutstandingInvoices.FirstOrDefault(candidate => candidate.Id == context.InvoiceId);
        if (invoice is null)
        {
            return false;
        }

        SelectedInvoice = invoice;
        return true;
    }

    private static DateTime? CalculateMaxExtensionDueDate(OutstandingInvoiceDto invoice)
    {
        if (invoice.DueDate == default)
        {
            return null;
        }

        var day15 = new DateTime(invoice.DueDate.Year, invoice.DueDate.Month, 15);
        var plusFive = invoice.DueDate.Date.AddDays(5);
        return plusFive <= day15 ? plusFive : day15;
    }

    private async Task LoadSelectedInvoiceQrAsync()
    {
        if (!IsStudent || SelectedInvoice is null)
        {
            SelectedInvoiceQr = null;
            QrStatusMessage = null;
            return;
        }

        var qr = await _service.GetInvoicePaymentQrAsync(SelectedInvoice.Id);
        SelectedInvoiceQr = qr;
        QrStatusMessage = HasQrPaymentDetails
            ? "Scan the QR code or copy the transfer content exactly."
            : "QR payment details are not ready for this invoice.";
    }

    private void NotifyQrState()
    {
        OnPropertyChanged(nameof(HasQrPaymentDetails));
        OnPropertyChanged(nameof(HasQrImage));
        OnPropertyChanged(nameof(IsQrMissing));
        OnPropertyChanged(nameof(QrTransferContent));
        OnPropertyChanged(nameof(QrDataUrl));
        OnPropertyChanged(nameof(QrAmountText));
        OnPropertyChanged(nameof(QrStatus));
        OnPropertyChanged(nameof(QrDueDateText));
        OnPropertyChanged(nameof(QrPaidAtText));
        OnPropertyChanged(nameof(HasQrPaidAt));
    }

    private void NotifyUiState()
    {
        OnPropertyChanged(nameof(IsAdmin));
        OnPropertyChanged(nameof(IsStudent));
        OnPropertyChanged(nameof(HasOutstandingInvoices));
        OnPropertyChanged(nameof(IsOutstandingInvoicesEmpty));
        OnPropertyChanged(nameof(HasPendingPayments));
        OnPropertyChanged(nameof(IsPendingPaymentsEmpty));
        OnPropertyChanged(nameof(CanCreatePayment));
        OnPropertyChanged(nameof(CanConfirmPayment));
        OnPropertyChanged(nameof(CanRefreshQrStatus));
        OnPropertyChanged(nameof(CanRequestExtension));
        OnPropertyChanged(nameof(HasExtensionMaxDueDate));
        OnPropertyChanged(nameof(HasExtensionDueDateError));
        OnPropertyChanged(nameof(IsQrMissing));
        _createPaymentCommand.RaiseCanExecuteChanged();
        _confirmPaymentCommand.RaiseCanExecuteChanged();
        _requestExtensionCommand.RaiseCanExecuteChanged();
        _refreshQrStatusCommand.RaiseCanExecuteChanged();
    }
}

