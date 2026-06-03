using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using DormitoryManagement.Application.Abstractions.Auth;
using DormitoryManagement.Application.DTOs.Billing;
using DormitoryManagement.Application.DTOs.Payments;
using DormitoryManagement.Application.DTOs.Rooms;
using DormitoryManagement.Application.Services.Billing;
using DormitoryManagement.Application.Services.Rooms;
using DormitoryManagement.Domain.Constants;
using DormitoryManagement.WPF.Common;
using DormitoryManagement.WPF.Navigation;

namespace DormitoryManagement.WPF.ViewModels.Billing;

public sealed class InvoiceListViewModel : ViewModelBase
{
    private readonly IBillingService _service;
    private readonly ICurrentUserService _currentUser;
    private readonly IRoomService? _roomService;
    private readonly INavigationService? _navigationService;
    private readonly PaymentNavigationState? _paymentNavigationState;
    private readonly List<RoomDto> _allRooms = new();
    private bool _hasLoaded;
    private string _billingPeriod = DateTime.Today.ToString("yyyy-MM");
    private DateTime? _dueDate;
    private InvoiceDto? _selectedInvoice;
    private string _selectedBuildingType = "All buildings";
    private RoomDto? _selectedRoom;
    private decimal _electricityCurrent;
    private string _electricityCurrentText = string.Empty;
    private decimal _waterCurrent;
    private string _waterCurrentText = string.Empty;
    private UtilityBillingPreviewDto? _utilityPreview;
    private int _createdCount;
    private int _skippedCount;
    private int _warningCount;
    private string? _billingPeriodError;
    private string? _dueDateError;
    private string? _successMessage;
    private PaymentNavigationContextDto? _selectedPaymentContext;
    private PaymentNavigationContextDto? _selectedExtensionContext;

    public InvoiceListViewModel(
        IBillingService service,
        ICurrentUserService currentUser,
        IRoomService? roomService = null,
        INavigationService? navigationService = null,
        PaymentNavigationState? paymentNavigationState = null)
    {
        _service = service;
        _currentUser = currentUser;
        _roomService = roomService;
        _navigationService = navigationService;
        _paymentNavigationState = paymentNavigationState;
        DueDate = CalculateMonthlyUtilityDueDate(_billingPeriod);
        LoadCommand = new AsyncRelayCommand(LoadAsync);
        GenerateMonthlyInvoicesCommand = new AsyncRelayCommand(GenerateMonthlyInvoicesAsync, () => CanGenerateInvoices && !IsBusy);
        PreviewUtilityBillingCommand = new AsyncRelayCommand(RefreshUtilityPreviewAsync, () => CanPreviewUtilityBilling);
        ExtendInvoiceCommand = new RelayCommand(
            parameter => ApplyExtensionContext(parameter as StudentBillingRowDto),
            parameter => parameter is StudentBillingRowDto row && row.CanExtend && !IsBusy);
        PayInvoiceCommand = new RelayCommand(
            parameter => ApplyPaymentContext(parameter as StudentBillingRowDto),
            parameter => parameter is StudentBillingRowDto row && row.CanPay && !IsBusy);
        BuildingTypeOptions.Add(_selectedBuildingType);
    }

    public ObservableCollection<InvoiceDto> Invoices { get; } = new();
    public ObservableCollection<StudentBillingRowDto> StudentBillingRows { get; } = new();
    public ObservableCollection<InvoiceItemDto> SelectedInvoiceItems { get; } = new();
    public ObservableCollection<string> Warnings { get; } = new();
    public ObservableCollection<string> BuildingTypeOptions { get; } = new();
    public ObservableCollection<RoomDto> RoomOptions { get; } = new();
    public ObservableCollection<string> PreviewValidationMessages { get; } = new();
    public ICommand LoadCommand { get; }
    public ICommand RefreshCommand => LoadCommand;
    public ICommand GenerateMonthlyInvoicesCommand { get; }
    public ICommand PreviewUtilityBillingCommand { get; }
    public ICommand ExtendInvoiceCommand { get; }
    public ICommand PayInvoiceCommand { get; }
    public bool HasInvoices => Invoices.Count > 0;
    public bool HasStudentBillingRows => StudentBillingRows.Count > 0;
    public bool HasStaffInvoiceRows => !IsStudent && Invoices.Count > 0;
    public bool IsInvoicesEmpty => _hasLoaded && !IsBusy && Invoices.Count == 0;
    public bool IsStudentInvoicesEmpty => _hasLoaded && !IsBusy && IsStudent && StudentBillingRows.Count == 0;
    public bool IsStaffInvoicesEmpty => _hasLoaded && !IsBusy && !IsStudent && Invoices.Count == 0;
    public bool HasSelectedInvoice => SelectedInvoice is not null;
    public bool HasSelectedInvoiceItems => SelectedInvoiceItems.Count > 0;
    public bool IsSelectedInvoiceItemsEmpty => HasSelectedInvoice && !IsBusy && SelectedInvoiceItems.Count == 0;
    public bool HasWarnings => Warnings.Count > 0;
    public bool HasRoomOptions => RoomOptions.Count > 0;
    public bool HasUtilityPreview => UtilityPreview is not null;
    public bool HasPreviewValidationMessages => PreviewValidationMessages.Count > 0;
    public bool CanPreviewUtilityBilling => CanGenerateInvoices && SelectedRoom is not null && !IsBusy;
    public decimal PreviewTotalAmount => UtilityPreview?.TotalAmount ?? 0m;
    public bool CanGenerateInvoices => _currentUser.IsInRole(RoleNames.Admin)
        || _currentUser.IsInRole(RoleNames.Manager);
    public bool IsStudent => _currentUser.IsInRole(RoleNames.Student);

    public string BillingPeriod
    {
        get => _billingPeriod;
        set
        {
            if (SetProperty(ref _billingPeriod, value))
            {
                BillingPeriodError = null;
                DueDate = CalculateMonthlyUtilityDueDate(value);
            }
        }
    }

    public DateTime? DueDate
    {
        get => _dueDate;
        set
        {
            if (SetProperty(ref _dueDate, value))
            {
                DueDateError = null;
            }
        }
    }

    public InvoiceDto? SelectedInvoice
    {
        get => _selectedInvoice;
        set
        {
            if (SetProperty(ref _selectedInvoice, value))
            {
                RebuildSelectedInvoiceItems();
            }
        }
    }

    public string SelectedBuildingType
    {
        get => _selectedBuildingType;
        set
        {
            if (SetProperty(ref _selectedBuildingType, value))
            {
                RebuildRoomOptions();
            }
        }
    }

    public RoomDto? SelectedRoom
    {
        get => _selectedRoom;
        set
        {
            if (SetProperty(ref _selectedRoom, value))
            {
                UtilityPreview = null;
                NotifyPreviewState();
            }
        }
    }

    public decimal ElectricityCurrent
    {
        get => _electricityCurrent;
        set
        {
            if (SetProperty(ref _electricityCurrent, value))
            {
                ElectricityCurrentText = value.ToString("0.##", CultureInfo.CurrentCulture);
                UtilityPreview = null;
                NotifyPreviewState();
            }
        }
    }

    public string ElectricityCurrentText
    {
        get => _electricityCurrentText;
        set
        {
            if (SetProperty(ref _electricityCurrentText, value ?? string.Empty))
            {
                UtilityPreview = null;
                NotifyPreviewState();
            }
        }
    }

    public decimal WaterCurrent
    {
        get => _waterCurrent;
        set
        {
            if (SetProperty(ref _waterCurrent, value))
            {
                WaterCurrentText = value.ToString("0.##", CultureInfo.CurrentCulture);
                UtilityPreview = null;
                NotifyPreviewState();
            }
        }
    }

    public string WaterCurrentText
    {
        get => _waterCurrentText;
        set
        {
            if (SetProperty(ref _waterCurrentText, value ?? string.Empty))
            {
                UtilityPreview = null;
                NotifyPreviewState();
            }
        }
    }

    public UtilityBillingPreviewDto? UtilityPreview
    {
        get => _utilityPreview;
        private set
        {
            if (SetProperty(ref _utilityPreview, value))
            {
                OnPropertyChanged(nameof(HasUtilityPreview));
                OnPropertyChanged(nameof(PreviewTotalAmount));
            }
        }
    }

    public int CreatedCount { get => _createdCount; private set => SetProperty(ref _createdCount, value); }
    public int SkippedCount { get => _skippedCount; private set => SetProperty(ref _skippedCount, value); }
    public int WarningCount { get => _warningCount; private set => SetProperty(ref _warningCount, value); }

    public string? BillingPeriodError
    {
        get => _billingPeriodError;
        private set
        {
            if (SetProperty(ref _billingPeriodError, value)) OnPropertyChanged(nameof(HasBillingPeriodError));
        }
    }

    public string? DueDateError
    {
        get => _dueDateError;
        private set
        {
            if (SetProperty(ref _dueDateError, value)) OnPropertyChanged(nameof(HasDueDateError));
        }
    }

    public string? SuccessMessage
    {
        get => _successMessage;
        private set
        {
            if (SetProperty(ref _successMessage, value)) OnPropertyChanged(nameof(HasSuccessMessage));
        }
    }

    public bool HasBillingPeriodError => !string.IsNullOrWhiteSpace(BillingPeriodError);
    public bool HasDueDateError => !string.IsNullOrWhiteSpace(DueDateError);
    public bool HasSuccessMessage => !string.IsNullOrWhiteSpace(SuccessMessage);
    public PaymentNavigationContextDto? SelectedPaymentContext
    {
        get => _selectedPaymentContext;
        private set => SetProperty(ref _selectedPaymentContext, value);
    }

    public PaymentNavigationContextDto? SelectedExtensionContext
    {
        get => _selectedExtensionContext;
        private set => SetProperty(ref _selectedExtensionContext, value);
    }

    private async Task LoadAsync()
    {
        ClearError();
        IsBusy = true;
        NotifyUiState();
        try
        {
            Invoices.Clear();
            StudentBillingRows.Clear();
            var invoices = await _service.GetInvoicesAsync();
            foreach (var invoice in invoices)
            {
                Invoices.Add(invoice);
            }

            if (IsStudent)
            {
                var rows = await _service.GetStudentBillingRowsAsync();
                foreach (var row in rows)
                {
                    StudentBillingRows.Add(row);
                }
            }

            SelectedInvoice = Invoices.FirstOrDefault();
            await LoadPreviewRoomsAsync();
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

    private async Task GenerateMonthlyInvoicesAsync()
    {
        if (!CanGenerateInvoices)
        {
            SetError("Only finance reviewers can generate invoices.");
            return;
        }

        ClearError();
        SuccessMessage = null;
        BillingPeriodError = null;
        DueDateError = null;
        PreviewValidationMessages.Clear();
        if (string.IsNullOrWhiteSpace(BillingPeriod))
        {
            BillingPeriodError = "Enter a billing period.";
        }

        var dueDate = DueDate;
        if (dueDate is null)
        {
            DueDateError = "Choose a due date.";
        }

        var selectedRoom = SelectedRoom;
        if (selectedRoom is null)
        {
            PreviewValidationMessages.Add("Select a room before confirming monthly utility billing.");
        }

        var hasReadings = TryParseReadings(out var electricityCurrent, out var waterCurrent);

        if (HasBillingPeriodError || dueDate is null || selectedRoom is null || !hasReadings)
        {
            NotifyPreviewState();
            return;
        }

        IsBusy = true;
        NotifyUiState();
        try
        {
            await _service.UpsertUtilityReadingAsync(new UtilityReadingRequest
            {
                RoomId = selectedRoom.Id,
                BillingPeriod = BillingPeriod.Trim(),
                ElectricityCurrent = electricityCurrent,
                WaterCurrent = waterCurrent
            });
            var result = await _service.GenerateMonthlyInvoicesAsync(new GenerateMonthlyInvoiceRequest
            {
                RoomId = selectedRoom.Id,
                BillingPeriod = BillingPeriod.Trim(),
                DueDate = dueDate.Value
            });
            CreatedCount = result.CreatedCount;
            SkippedCount = result.SkippedCount;
            WarningCount = result.MissingUtilityReadingCount;
            Warnings.Clear();
            foreach (var warning in result.Warnings)
            {
                Warnings.Add(warning);
            }

            SuccessMessage = $"Generated {result.CreatedCount}, skipped {result.SkippedCount}, missing readings {result.MissingUtilityReadingCount}.";
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

    private void RebuildSelectedInvoiceItems()
    {
        SelectedInvoiceItems.Clear();
        if (SelectedInvoice is not null)
        {
            foreach (var item in SelectedInvoice.Items)
            {
                SelectedInvoiceItems.Add(item);
            }
        }

        NotifyUiState();
    }

    private void ApplyExtensionContext(StudentBillingRowDto? row)
    {
        if (row is null || !row.CanExtend)
        {
            return;
        }

        var context = CreatePaymentContext(row);
        SelectedExtensionContext = context;
        _paymentNavigationState?.SetExtensionContext(context);
        _navigationService?.NavigateTo<PaymentViewModel>();
    }

    private void ApplyPaymentContext(StudentBillingRowDto? row)
    {
        if (row is null || !row.CanPay)
        {
            return;
        }

        var context = CreatePaymentContext(row);
        SelectedPaymentContext = context;
        _paymentNavigationState?.SetPaymentContext(context);
        _navigationService?.NavigateTo<PaymentViewModel>();
    }

    private static PaymentNavigationContextDto CreatePaymentContext(StudentBillingRowDto row) =>
        new()
        {
            InvoiceId = row.InvoiceId,
            InvoiceNumber = row.InvoiceNumber,
            InvoiceKind = row.InvoiceKind,
            RoomNumber = row.RoomNumber,
            BillingPeriod = row.BillingPeriod,
            DueDate = row.DueDate,
            TotalAmount = row.TotalAmount,
            PaidAmount = row.PaidAmount,
            RemainingAmount = row.TotalAmount - row.PaidAmount
        };

    private async Task LoadPreviewRoomsAsync()
    {
        if (!CanGenerateInvoices || _roomService is null)
        {
            return;
        }

        var result = await _roomService.GetRoomsAsync(new RoomFilterRequest { PageNumber = 1, PageSize = 500 });
        _allRooms.Clear();
        _allRooms.AddRange(result.Items);
        RebuildBuildingTypeOptions();
        RebuildRoomOptions();
    }

    private void RebuildBuildingTypeOptions()
    {
        var current = SelectedBuildingType;
        BuildingTypeOptions.Clear();
        BuildingTypeOptions.Add("All buildings");
        foreach (var building in _allRooms
            .Select(room => room.BuildingName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct()
            .OrderBy(name => name))
        {
            BuildingTypeOptions.Add(building);
        }

        SelectedBuildingType = BuildingTypeOptions.Contains(current) ? current : "All buildings";
    }

    private void RebuildRoomOptions()
    {
        var selectedRoomId = SelectedRoom?.Id;
        var rooms = _allRooms.AsEnumerable();
        if (!string.Equals(SelectedBuildingType, "All buildings", StringComparison.OrdinalIgnoreCase))
        {
            rooms = rooms.Where(room => room.BuildingName == SelectedBuildingType);
        }

        RoomOptions.Clear();
        foreach (var room in rooms.OrderBy(room => room.RoomNumber))
        {
            RoomOptions.Add(room);
        }

        SelectedRoom = selectedRoomId.HasValue
            ? RoomOptions.FirstOrDefault(room => room.Id == selectedRoomId.Value)
            : RoomOptions.FirstOrDefault();
        NotifyPreviewState();
    }

    private async Task RefreshUtilityPreviewAsync()
    {
        ClearError();
        PreviewValidationMessages.Clear();
        UtilityPreview = null;
        if (SelectedRoom is null)
        {
            PreviewValidationMessages.Add("Select a room before previewing utility billing.");
        }

        if (string.IsNullOrWhiteSpace(BillingPeriod))
        {
            PreviewValidationMessages.Add("Enter a billing period.");
        }

        if (PreviewValidationMessages.Count > 0 || SelectedRoom is null)
        {
            NotifyPreviewState();
            return;
        }

        if (!TryParseReadings(out var electricityCurrent, out var waterCurrent))
        {
            NotifyPreviewState();
            return;
        }

        IsBusy = true;
        NotifyUiState();
        try
        {
            UtilityPreview = await _service.PreviewUtilityBillingAsync(new UtilityBillingPreviewRequest
            {
                RoomId = SelectedRoom.Id,
                BillingPeriod = BillingPeriod.Trim(),
                ElectricityCurrent = electricityCurrent,
                WaterCurrent = waterCurrent
            });
            foreach (var message in UtilityPreview.ValidationMessages)
            {
                PreviewValidationMessages.Add(message);
            }
        }
        catch (Exception ex)
        {
            PreviewValidationMessages.Add(ex.Message);
        }
        finally
        {
            IsBusy = false;
            NotifyUiState();
        }
    }

    private bool TryParseReadings(out decimal electricityCurrent, out decimal waterCurrent)
    {
        var valid = true;
        if (!TryParseDecimal(ElectricityCurrentText, out electricityCurrent))
        {
            PreviewValidationMessages.Add("Enter a valid electricity reading.");
            valid = false;
        }

        if (!TryParseDecimal(WaterCurrentText, out waterCurrent))
        {
            PreviewValidationMessages.Add("Enter a valid water reading.");
            valid = false;
        }

        if (valid)
        {
            _electricityCurrent = electricityCurrent;
            _waterCurrent = waterCurrent;
        }

        return valid;
    }

    private static bool TryParseDecimal(string value, out decimal result)
    {
        var text = value.Trim();
        return decimal.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, out result)
            || decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out result);
    }

    private void NotifyUiState()
    {
        OnPropertyChanged(nameof(HasInvoices));
        OnPropertyChanged(nameof(HasStudentBillingRows));
        OnPropertyChanged(nameof(HasStaffInvoiceRows));
        OnPropertyChanged(nameof(IsInvoicesEmpty));
        OnPropertyChanged(nameof(IsStudentInvoicesEmpty));
        OnPropertyChanged(nameof(IsStaffInvoicesEmpty));
        OnPropertyChanged(nameof(HasSelectedInvoice));
        OnPropertyChanged(nameof(HasSelectedInvoiceItems));
        OnPropertyChanged(nameof(IsSelectedInvoiceItemsEmpty));
        OnPropertyChanged(nameof(HasWarnings));
        OnPropertyChanged(nameof(CanGenerateInvoices));
        OnPropertyChanged(nameof(IsStudent));
        NotifyPreviewState();
        if (GenerateMonthlyInvoicesCommand is AsyncRelayCommand command)
        {
            command.RaiseCanExecuteChanged();
        }

        if (ExtendInvoiceCommand is RelayCommand extendCommand)
        {
            extendCommand.RaiseCanExecuteChanged();
        }

        if (PayInvoiceCommand is RelayCommand payCommand)
        {
            payCommand.RaiseCanExecuteChanged();
        }
    }

    private void NotifyPreviewState()
    {
        OnPropertyChanged(nameof(HasRoomOptions));
        OnPropertyChanged(nameof(HasUtilityPreview));
        OnPropertyChanged(nameof(HasPreviewValidationMessages));
        OnPropertyChanged(nameof(CanPreviewUtilityBilling));
        OnPropertyChanged(nameof(PreviewTotalAmount));
        if (PreviewUtilityBillingCommand is AsyncRelayCommand command)
        {
            command.RaiseCanExecuteChanged();
        }
    }

    private static DateTime? CalculateMonthlyUtilityDueDate(string billingPeriod)
    {
        var parts = billingPeriod.Split('-');
        if (parts.Length != 2
            || !int.TryParse(parts[0], out var year)
            || !int.TryParse(parts[1], out var month)
            || month is < 1 or > 12)
        {
            return null;
        }

        return new DateTime(year, month, 1).AddMonths(1).AddDays(4);
    }
}

