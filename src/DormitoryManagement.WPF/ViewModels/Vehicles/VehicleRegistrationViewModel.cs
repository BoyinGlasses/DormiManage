using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using DormitoryManagement.Application.DTOs.Vehicles;
using DormitoryManagement.Application.Services.Vehicles;
using DormitoryManagement.Domain.Enums;
using DormitoryManagement.WPF.Common;

namespace DormitoryManagement.WPF.ViewModels.Vehicles;

public sealed class VehicleRegistrationViewModel : ViewModelBase
{
    private readonly IVehicleService _service;
    private readonly AsyncRelayCommand _loadCommand;
    private readonly AsyncRelayCommand _submitCommand;
    private readonly RelayCommand _selectDurationCommand;
    private string _licensePlate = string.Empty;
    private int _selectedMonthCount = 1;
    private bool _hasLoaded;
    private string? _licensePlateError;
    private string? _successMessage;

    public VehicleRegistrationViewModel(IVehicleService service)
    {
        _service = service;
        _loadCommand = new AsyncRelayCommand(LoadAsync, () => !IsBusy);
        _submitCommand = new AsyncRelayCommand(SubmitAsync, () => !IsBusy);
        _selectDurationCommand = new RelayCommand(SelectDuration, CanSelectDuration);
        LoadCommand = _loadCommand;
        SubmitCommand = _submitCommand;
        SelectDurationCommand = _selectDurationCommand;
        DurationOptions = new ObservableCollection<VehicleRegistrationDurationOptionViewModel>(
            MonthOptions.Select(months => new VehicleRegistrationDurationOptionViewModel(months, $"{months} tháng", months == _selectedMonthCount)));
    }

    public int[] MonthOptions { get; } = { 1, 2, 3, 6 };
    public string[] HistoryColumnHeaders { get; } =
    [
        "Ngày đăng ký",
        "Biển số xe",
        "Thời hạn",
        "Tổng tiền",
        "Trạng thái"
    ];

    public ObservableCollection<VehicleRegistrationDto> Vehicles { get; } = new();
    public ObservableCollection<VehicleRegistrationDurationOptionViewModel> DurationOptions { get; }
    public ObservableCollection<VehicleRegistrationHistoryReviewRowViewModel> HistoryReviewRows { get; } = new();

    public ICommand LoadCommand { get; }
    public ICommand RefreshCommand => LoadCommand;
    public ICommand SubmitCommand { get; }
    public ICommand SelectDurationCommand { get; }

    public bool HasVehicles => Vehicles.Count > 0;
    public bool IsVehiclesEmpty => _hasLoaded && !IsBusy && Vehicles.Count == 0;
    public bool HasHistoryReviewRows => HistoryReviewRows.Count > 0;
    public bool IsHistoryEmpty => _hasLoaded && !IsBusy && HistoryReviewRows.Count == 0;
    public decimal PreviewAmount => SelectedMonthCount * 40000m;
    public string SubtotalAmountText => FormatCurrency(PreviewAmount);
    public string RegistrationTitle => "Đăng ký gửi xe";
    public string RegistrationSectionTitle => "Thông tin đăng ký";
    public string LicensePlateLabel => "Biển số xe";
    public string LicensePlatePlaceholder => "VD: 59A1-23456";
    public string LicensePlateHelperText => "Nhập chính xác biển số xe để bảo vệ tài sản của bạn.";
    public string DurationLabel => "Thời hạn đăng ký";
    public string SubtotalLabel => "Tiền tạm tính:";
    public string PrimaryActionLabel => "Đăng ký ngay";
    public string PaymentTitle => "Thanh toán chuyển khoản";
    public string PaymentSupportingText => "Quét mã để hoàn tất thanh toán";
    public string PaymentVerifiedBadgeText => "Mã QR tự động cập nhật số tiền";
    public string HistoryTitle => "Lịch sử đăng ký";
    public string HistoryActionLabel => "Xem tất cả";

    public int SelectedMonthCount
    {
        get => _selectedMonthCount;
        set
        {
            if (SetProperty(ref _selectedMonthCount, value))
            {
                SyncDurationSelection();
                OnPropertyChanged(nameof(PreviewAmount));
                OnPropertyChanged(nameof(SubtotalAmountText));
                _selectDurationCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string LicensePlate
    {
        get => _licensePlate;
        set
        {
            if (SetProperty(ref _licensePlate, value))
            {
                LicensePlateError = null;
            }
        }
    }

    public string? LicensePlateError
    {
        get => _licensePlateError;
        private set
        {
            if (SetProperty(ref _licensePlateError, value))
            {
                OnPropertyChanged(nameof(HasLicensePlateError));
            }
        }
    }

    public bool HasLicensePlateError => !string.IsNullOrWhiteSpace(LicensePlateError);

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

    public bool HasSuccessMessage => !string.IsNullOrWhiteSpace(SuccessMessage);

    private async Task LoadAsync()
    {
        ClearError();
        SuccessMessage = null;
        IsBusy = true;
        NotifyState();
        try
        {
            Vehicles.Clear();
            var registrations = await _service.GetCurrentStudentVehicleRegistrationsAsync();
            foreach (var registration in registrations)
            {
                Vehicles.Add(registration);
            }

            _hasLoaded = true;
            RebuildHistoryReviewRows();
        }
        catch (Exception ex)
        {
            SetError(ex.Message);
        }
        finally
        {
            IsBusy = false;
            NotifyState();
        }
    }

    private async Task SubmitAsync()
    {
        ClearError();
        LicensePlateError = null;
        SuccessMessage = null;
        if (string.IsNullOrWhiteSpace(LicensePlate))
        {
            LicensePlateError = "Nhập biển số xe.";
            return;
        }

        IsBusy = true;
        NotifyState();
        try
        {
            var vehicle = await _service.RegisterVehicleAsync(new CreateVehicleRegistrationRequest
            {
                LicensePlate = LicensePlate,
                MonthCount = SelectedMonthCount
            });
            vehicle.RowNumber = Vehicles.Count + 1;
            Vehicles.Insert(0, vehicle);
            LicensePlate = string.Empty;
            SuccessMessage = "Đăng ký giữ xe thành công. Hóa đơn đã được tạo trong Billing.";
            _hasLoaded = true;
            RebuildHistoryReviewRows();
        }
        catch (Exception ex)
        {
            SetError(ex.Message);
        }
        finally
        {
            IsBusy = false;
            NotifyState();
        }
    }

    private void SelectDuration(object? parameter)
    {
        if (TryGetDurationMonths(parameter, out var months))
        {
            SelectedMonthCount = months;
        }
    }

    private bool CanSelectDuration(object? parameter) =>
        TryGetDurationMonths(parameter, out var months)
        && months != SelectedMonthCount
        && !IsBusy;

    private static bool TryGetDurationMonths(object? parameter, out int months)
    {
        switch (parameter)
        {
            case int intValue:
                months = intValue;
                return true;
            case string text when int.TryParse(text, out var parsedMonths):
                months = parsedMonths;
                return true;
            default:
                months = 0;
                return false;
        }
    }

    private void SyncDurationSelection()
    {
        foreach (var option in DurationOptions)
        {
            option.IsSelected = option.Months == SelectedMonthCount;
        }
    }

    private void RebuildHistoryReviewRows()
    {
        HistoryReviewRows.Clear();
        foreach (var registration in Vehicles
                     .OrderByDescending(item => item.RegisteredAt)
                     .ThenByDescending(item => item.RowNumber))
        {
            HistoryReviewRows.Add(new VehicleRegistrationHistoryReviewRowViewModel(
                registeredAtText: registration.RegisteredAt == default ? string.Empty : registration.RegisteredAt.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                licensePlateText: string.IsNullOrWhiteSpace(registration.NormalizedPlate) ? registration.LicensePlate : registration.NormalizedPlate,
                durationText: $"{registration.MonthCount} tháng",
                totalAmountText: FormatCurrency(registration.Amount),
                statusLabel: GetStatusLabel(registration),
                statusTone: GetStatusTone(registration)));
        }

        OnPropertyChanged(nameof(HasHistoryReviewRows));
        OnPropertyChanged(nameof(IsHistoryEmpty));
    }

    private static string GetStatusLabel(VehicleRegistrationDto registration)
    {
        if (registration.Status == VehicleStatus.Approved)
        {
            return "Đã duyệt";
        }

        if (registration.Status == VehicleStatus.Expired)
        {
            return "Hết hạn";
        }

        return !string.IsNullOrWhiteSpace(registration.StatusText)
            ? registration.StatusText
            : registration.Status switch
            {
                VehicleStatus.Pending => "Chưa thanh toán",
                VehicleStatus.Rejected => "Từ chối",
                VehicleStatus.Cancelled => "Đã hủy",
                _ => "Chưa cập nhật"
            };
    }

    private static string GetStatusTone(VehicleRegistrationDto registration)
    {
        if (registration.Status == VehicleStatus.Approved)
        {
            return "approved";
        }

        if (registration.Status == VehicleStatus.Expired)
        {
            return "expired";
        }

        return registration.Status switch
        {
            VehicleStatus.Rejected => "rejected",
            VehicleStatus.Pending => "pending",
            VehicleStatus.Cancelled => "neutral",
            _ => "neutral"
        };
    }

    private static string FormatCurrency(decimal amount) =>
        string.Format(CultureInfo.GetCultureInfo("vi-VN"), "{0:N0} ₫", amount);

    private void NotifyState()
    {
        OnPropertyChanged(nameof(HasVehicles));
        OnPropertyChanged(nameof(IsVehiclesEmpty));
        OnPropertyChanged(nameof(HasHistoryReviewRows));
        OnPropertyChanged(nameof(IsHistoryEmpty));
        OnPropertyChanged(nameof(SubtotalAmountText));
        _loadCommand.RaiseCanExecuteChanged();
        _submitCommand.RaiseCanExecuteChanged();
        _selectDurationCommand.RaiseCanExecuteChanged();
    }
}

public sealed class VehicleRegistrationDurationOptionViewModel : ObservableObject
{
    private bool _isSelected;

    public VehicleRegistrationDurationOptionViewModel(int months, string label, bool isSelected)
    {
        Months = months;
        Label = label;
        _isSelected = isSelected;
    }

    public int Months { get; }
    public string Label { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}

public sealed class VehicleRegistrationHistoryReviewRowViewModel
{
    public VehicleRegistrationHistoryReviewRowViewModel(
        string registeredAtText,
        string licensePlateText,
        string durationText,
        string totalAmountText,
        string statusLabel,
        string statusTone)
    {
        RegisteredAtText = registeredAtText;
        LicensePlateText = licensePlateText;
        DurationText = durationText;
        TotalAmountText = totalAmountText;
        StatusLabel = statusLabel;
        StatusTone = statusTone;
    }

    public string RegisteredAtText { get; }
    public string LicensePlateText { get; }
    public string DurationText { get; }
    public string TotalAmountText { get; }
    public string StatusLabel { get; }
    public string StatusTone { get; }
}

