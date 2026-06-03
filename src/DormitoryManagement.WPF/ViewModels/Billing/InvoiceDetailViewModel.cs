using System.Collections.ObjectModel;
using System.Windows.Input;
using DormitoryManagement.Application.Services.Billing;
using DormitoryManagement.Domain.Enums;
using DormitoryManagement.WPF.Common;
using DormitoryManagement.WPF.Navigation;

namespace DormitoryManagement.WPF.ViewModels.Billing;

public sealed class InvoiceDetailViewModel : ViewModelBase
{
    private readonly IBillingService _service;
    private readonly INavigationService _navigationService;
    private string _invoiceId = string.Empty;
    private string _invoiceNumber = "No invoice selected";
    private string _billingPeriod = "-";
    private InvoiceStatus _status = InvoiceStatus.Draft;
    private decimal _subtotal;
    private decimal _discount;
    private decimal _penalty;
    private decimal _paidAmount;
    private decimal _adjustmentAmount;
    private string _adjustmentReason = string.Empty;
    private string? _actionMessage;
    private string? _adjustmentReasonError;

    public InvoiceDetailViewModel(IBillingService service, INavigationService navigationService)
    {
        _service = service;
        _navigationService = navigationService;
        AdjustCommand = new AsyncRelayCommand(AdjustAsync);
        PayCommand = new RelayCommand(() => _navigationService.NavigateTo<PaymentViewModel>());
        PrintCommand = new RelayCommand(() => ActionMessage = "Print preview is a demo placeholder.");
        ExportCommand = new RelayCommand(() => ActionMessage = "Export is a demo placeholder.");
    }

    public ObservableCollection<InvoiceItemRow> Items { get; } = new();
    public ICommand AdjustCommand { get; }
    public ICommand PayCommand { get; }
    public ICommand PrintCommand { get; }
    public ICommand ExportCommand { get; }

    public string InvoiceId { get => _invoiceId; set => SetProperty(ref _invoiceId, value); }
    public string InvoiceNumber { get => _invoiceNumber; set => SetProperty(ref _invoiceNumber, value); }
    public string BillingPeriod { get => _billingPeriod; set => SetProperty(ref _billingPeriod, value); }
    public InvoiceStatus Status { get => _status; set => SetProperty(ref _status, value); }

    public decimal Subtotal { get => _subtotal; set { if (SetProperty(ref _subtotal, value)) NotifyTotals(); } }
    public decimal Discount { get => _discount; set { if (SetProperty(ref _discount, value)) NotifyTotals(); } }
    public decimal Penalty { get => _penalty; set { if (SetProperty(ref _penalty, value)) NotifyTotals(); } }
    public decimal PaidAmount { get => _paidAmount; set { if (SetProperty(ref _paidAmount, value)) NotifyTotals(); } }
    public decimal AdjustmentAmount { get => _adjustmentAmount; set => SetProperty(ref _adjustmentAmount, value); }
    public string AdjustmentReason
    {
        get => _adjustmentReason;
        set
        {
            if (SetProperty(ref _adjustmentReason, value))
            {
                AdjustmentReasonError = null;
            }
        }
    }

    public decimal TotalAmount => Subtotal - Discount + Penalty;
    public decimal RemainingAmount => Math.Max(0, TotalAmount - PaidAmount);
    public string SubtotalText => Subtotal.ToString("N0") + " VND";
    public string DiscountText => Discount.ToString("N0") + " VND";
    public string PenaltyText => Penalty.ToString("N0") + " VND";
    public string PaidText => PaidAmount.ToString("N0") + " VND";
    public string RemainingText => RemainingAmount.ToString("N0") + " VND";
    public bool CanPay => RemainingAmount > 0;

    public string? ActionMessage
    {
        get => _actionMessage;
        private set
        {
            if (SetProperty(ref _actionMessage, value)) OnPropertyChanged(nameof(HasActionMessage));
        }
    }

    public bool HasActionMessage => !string.IsNullOrWhiteSpace(ActionMessage);

    public string? AdjustmentReasonError
    {
        get => _adjustmentReasonError;
        private set
        {
            if (SetProperty(ref _adjustmentReasonError, value)) OnPropertyChanged(nameof(HasAdjustmentReasonError));
        }
    }

    public bool HasAdjustmentReasonError => !string.IsNullOrWhiteSpace(AdjustmentReasonError);

    private async Task AdjustAsync()
    {
        ClearError();
        ActionMessage = null;
        AdjustmentReasonError = null;
        if (!Guid.TryParse(InvoiceId, out var invoiceId))
        {
            SetError("Select a valid invoice before adjusting.");
            return;
        }

        if (string.IsNullOrWhiteSpace(AdjustmentReason))
        {
            AdjustmentReasonError = "Enter an adjustment reason.";
            return;
        }

        IsBusy = true;
        try
        {
            await _service.AdjustInvoiceAsync(invoiceId, AdjustmentAmount, AdjustmentReason.Trim());
            ActionMessage = "Invoice adjustment saved.";
        }
        catch (Exception ex)
        {
            SetError(ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void NotifyTotals()
    {
        OnPropertyChanged(nameof(TotalAmount));
        OnPropertyChanged(nameof(RemainingAmount));
        OnPropertyChanged(nameof(SubtotalText));
        OnPropertyChanged(nameof(DiscountText));
        OnPropertyChanged(nameof(PenaltyText));
        OnPropertyChanged(nameof(PaidText));
        OnPropertyChanged(nameof(RemainingText));
        OnPropertyChanged(nameof(CanPay));
    }
}

public sealed class InvoiceItemRow
{
    public string FeeType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Amount { get; set; }
}
