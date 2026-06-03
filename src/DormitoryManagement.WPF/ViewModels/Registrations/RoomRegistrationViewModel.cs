using System.Collections.ObjectModel;
using System.Windows.Input;
using DormitoryManagement.Application.DTOs.Registrations;
using DormitoryManagement.Application.DTOs.Rooms;
using DormitoryManagement.Application.Services.Registrations;
using DormitoryManagement.Application.Services.Rooms;
using DormitoryManagement.Domain.Enums;
using DormitoryManagement.WPF.Common;

namespace DormitoryManagement.WPF.ViewModels.Registrations;

public sealed class RoomRegistrationViewModel : ViewModelBase
{
    private const string AllBuildings = "Chọn khu vực";
    private const string AllFloors = "Chọn tầng";
    private const string AllRoomTypes = "Chọn loại phòng";
    private const string AllGenders = "All genders";
    private const string PriceDefault = "Room number";
    private const string PriceLowToHigh = "Price: low to high";
    private const string PriceHighToLow = "Price: high to low";
    private readonly IRoomRegistrationService _registrationService;
    private readonly IRoomService _roomService;
    private readonly AsyncRelayCommand _submitCommand;
    private readonly List<RoomDto> _allAvailableRooms = new();
    private bool _hasLoaded;
    private RoomDto? _selectedRoom;
    private string _selectedBuilding = AllBuildings;
    private string _selectedFloor = AllFloors;
    private string _selectedRoomType = AllRoomTypes;
    private string _selectedGender = AllGenders;
    private string _selectedPriceSort = PriceDefault;
    private int _contractTermMonths = 12;
    private bool _includesInternet;
    private bool _acceptsDormRules;
    private string? _externalLockMessage;
    private string? _note;
    private string? _statusMessage;

    public RoomRegistrationViewModel(IRoomRegistrationService registrationService, IRoomService roomService)
    {
        _registrationService = registrationService;
        _roomService = roomService;
        LoadCommand = new AsyncRelayCommand(LoadAsync);
        ApplyFiltersCommand = new RelayCommand(ApplyLocalFilters);
        ClearFiltersCommand = new RelayCommand(ClearFilters);
        _submitCommand = new AsyncRelayCommand(SubmitAsync, () => CanSubmitRegistration);
        SubmitCommand = _submitCommand;

        BuildingOptions.Add(AllBuildings);
        FloorOptions.Add(AllFloors);
        RoomTypeOptions.Add(AllRoomTypes);
        GenderOptions.Add(AllGenders);
        GenderOptions.Add(RoomGenderType.Male.ToString());
        GenderOptions.Add(RoomGenderType.Female.ToString());
        PriceSortOptions.Add(PriceDefault);
        PriceSortOptions.Add(PriceLowToHigh);
        PriceSortOptions.Add(PriceHighToLow);
    }

    public event EventHandler? RegistrationSubmitted;

    public ObservableCollection<RoomDto> AvailableRooms { get; } = new();
    public ObservableCollection<RoomRegistrationDto> RecentRegistrations { get; } = new();
    public ObservableCollection<RoomRegistrationDto> MyRegistrations => RecentRegistrations;
    public ObservableCollection<string> BuildingOptions { get; } = new();
    public ObservableCollection<string> FloorOptions { get; } = new();
    public ObservableCollection<string> RoomTypeOptions { get; } = new();
    public ObservableCollection<string> GenderOptions { get; } = new();
    public ObservableCollection<string> PriceSortOptions { get; } = new();
    public IReadOnlyList<int> ContractTermOptions { get; } = new[] { 6, 12 };
    public ICommand LoadCommand { get; }
    public ICommand SubmitCommand { get; }
    public ICommand RefreshCommand => LoadCommand;
    public ICommand ApplyFiltersCommand { get; }
    public ICommand ClearFiltersCommand { get; }
    public bool HasAvailableRooms => AvailableRooms.Count > 0;
    public bool IsAvailableRoomsEmpty => _hasLoaded && !IsBusy && AvailableRooms.Count == 0;
    public bool HasRecentRegistrations => RecentRegistrations.Count > 0;
    public bool IsRecentRegistrationsEmpty => _hasLoaded && !IsBusy && RecentRegistrations.Count == 0;
    public bool HasApprovedRegistration => RecentRegistrations.Any(registration => registration.Status == RegistrationStatus.Approved);
    public bool HasPendingRegistration => RecentRegistrations.Any(registration => registration.Status == RegistrationStatus.Pending);
    public bool HasPaymentPendingRegistration => RecentRegistrations.Any(registration => registration.Status == RegistrationStatus.PaymentPending);
    public bool IsExternallyLocked => !string.IsNullOrWhiteSpace(_externalLockMessage);
    public bool IsRegistrationLocked => HasApprovedRegistration || HasPendingRegistration || HasPaymentPendingRegistration || IsExternallyLocked;
    public string? RegistrationLockMessage => IsExternallyLocked
        ? _externalLockMessage
        : HasApprovedRegistration
            ? "Bạn đã có đăng ký phòng được duyệt. Không thể gửi yêu cầu mới."
            : HasPaymentPendingRegistration
                ? "Yêu cầu đăng ký đang chờ thanh toán hợp đồng."
                : HasPendingRegistration
                    ? "Yêu cầu đăng ký đang chờ xử lí."
                    : null;
    public string? AvailabilityStatusMessage => RegistrationLockMessage ?? (IsAvailableRoomsEmpty ? "Hiện chưa có phòng trống phù hợp để đăng ký." : null);
    public bool CanSubmitRegistration => SelectedRoom is not null && AvailableRooms.Count > 0 && AcceptsDormRules && !IsBusy && !IsRegistrationLocked;

    public RoomDto? SelectedRoom
    {
        get => _selectedRoom;
        set
        {
            if (SetProperty(ref _selectedRoom, value))
            {
                NotifyUiState();
            }
        }
    }

    public string SelectedBuilding
    {
        get => _selectedBuilding;
        set
        {
            if (SetProperty(ref _selectedBuilding, value ?? AllBuildings))
            {
                RebuildFloorOptions();
                ApplyLocalFilters();
            }
        }
    }

    public string SelectedFloor
    {
        get => _selectedFloor;
        set
        {
            if (SetProperty(ref _selectedFloor, value ?? AllFloors))
            {
                RebuildRoomTypeOptions();
                ApplyLocalFilters();
            }
        }
    }

    public string SelectedRoomType
    {
        get => _selectedRoomType;
        set
        {
            if (SetProperty(ref _selectedRoomType, value ?? AllRoomTypes))
            {
                ApplyLocalFilters();
            }
        }
    }

    public string SelectedGender
    {
        get => _selectedGender;
        set
        {
            if (SetProperty(ref _selectedGender, value ?? AllGenders))
            {
                ApplyLocalFilters();
            }
        }
    }

    public string SelectedPriceSort
    {
        get => _selectedPriceSort;
        set
        {
            if (SetProperty(ref _selectedPriceSort, value ?? PriceDefault))
            {
                ApplyLocalFilters();
            }
        }
    }

    public int ContractTermMonths
    {
        get => _contractTermMonths;
        set
        {
            if (SetProperty(ref _contractTermMonths, value))
            {
                NotifyUiState();
            }
        }
    }

    public bool IncludesInternet
    {
        get => _includesInternet;
        set => SetProperty(ref _includesInternet, value);
    }

    public bool AcceptsDormRules
    {
        get => _acceptsDormRules;
        set
        {
            if (SetProperty(ref _acceptsDormRules, value))
            {
                NotifyUiState();
            }
        }
    }

    public string? Note
    {
        get => _note;
        set => SetProperty(ref _note, value);
    }

    public string? StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public void SetExternalLockMessage(string? message)
    {
        if (_externalLockMessage == message)
        {
            return;
        }

        _externalLockMessage = string.IsNullOrWhiteSpace(message) ? null : message;
        NotifyUiState();
    }

    private async Task LoadAsync()
    {
        ClearError();
        IsBusy = true;
        NotifyUiState();
        try
        {
            RecentRegistrations.Clear();
            var registrations = await _registrationService.GetCurrentStudentRegistrationsAsync();
            foreach (var registration in registrations)
            {
                RecentRegistrations.Add(registration);
            }

            AvailableRooms.Clear();
            if (IsRegistrationLocked)
            {
                _allAvailableRooms.Clear();
                AvailableRooms.Clear();
                RebuildFilterOptions();
                SelectedRoom = null;
            }
            else
            {
                var rooms = await _roomService.GetAvailableRoomsAsync(new RoomFilterRequest { PageNumber = 1, PageSize = 200 });
                var selectedRoomId = SelectedRoom?.Id;
                _allAvailableRooms.Clear();
                _allAvailableRooms.AddRange(rooms);
                RebuildFilterOptions();
                ApplyLocalFilters();
                SelectedRoom = selectedRoomId.HasValue
                    ? AvailableRooms.FirstOrDefault(room => room.Id == selectedRoomId.Value) ?? AvailableRooms.FirstOrDefault()
                    : AvailableRooms.FirstOrDefault();
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

    private void ClearFilters()
    {
        SelectedBuilding = AllBuildings;
        SelectedFloor = AllFloors;
        SelectedRoomType = AllRoomTypes;
        SelectedGender = AllGenders;
        SelectedPriceSort = PriceDefault;
        ApplyLocalFilters();
    }

    private void RebuildFilterOptions()
    {
        var currentBuilding = SelectedBuilding;
        BuildingOptions.Clear();
        BuildingOptions.Add(AllBuildings);
        foreach (var building in _allAvailableRooms
            .Select(room => room.BuildingName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct()
            .OrderBy(name => name))
        {
            BuildingOptions.Add(building);
        }

        SelectedBuilding = BuildingOptions.Contains(currentBuilding) ? currentBuilding : AllBuildings;
        RebuildFloorOptions();
    }

    private void RebuildFloorOptions()
    {
        var currentFloor = SelectedFloor;
        var rooms = SelectedBuilding == AllBuildings
            ? _allAvailableRooms
            : _allAvailableRooms.Where(room => room.BuildingName == SelectedBuilding);
        FloorOptions.Clear();
        FloorOptions.Add(AllFloors);
        foreach (var floor in rooms
            .Select(room => room.FloorNumber)
            .Where(floor => floor > 0)
            .Distinct()
            .OrderBy(floor => floor))
        {
            FloorOptions.Add(FormatFloor(floor));
        }

        SelectedFloor = FloorOptions.Contains(currentFloor) ? currentFloor : AllFloors;
        RebuildRoomTypeOptions();
    }

    private void RebuildRoomTypeOptions()
    {
        var currentRoomType = SelectedRoomType;
        var rooms = _allAvailableRooms.AsEnumerable();
        if (SelectedBuilding != AllBuildings)
        {
            rooms = rooms.Where(room => room.BuildingName == SelectedBuilding);
        }

        if (TryParseFloor(SelectedFloor, out var floorNumber))
        {
            rooms = rooms.Where(room => room.FloorNumber == floorNumber);
        }

        RoomTypeOptions.Clear();
        RoomTypeOptions.Add(AllRoomTypes);
        foreach (var roomType in rooms
            .Select(room => room.Capacity)
            .Where(capacity => capacity > 0)
            .Distinct()
            .OrderBy(capacity => capacity)
            .Select(FormatRoomType))
        {
            RoomTypeOptions.Add(roomType);
        }

        SelectedRoomType = RoomTypeOptions.Contains(currentRoomType) ? currentRoomType : AllRoomTypes;
    }

    private void ApplyLocalFilters()
    {
        IEnumerable<RoomDto> rooms = _allAvailableRooms;

        if (SelectedBuilding != AllBuildings)
        {
            rooms = rooms.Where(room => room.BuildingName == SelectedBuilding);
        }

        if (TryParseFloor(SelectedFloor, out var floorNumber))
        {
            rooms = rooms.Where(room => room.FloorNumber == floorNumber);
        }

        if (TryParseRoomType(SelectedRoomType, out var capacity))
        {
            rooms = rooms.Where(room => room.Capacity == capacity);
        }

        if (Enum.TryParse<RoomGenderType>(SelectedGender, out var genderType))
        {
            rooms = rooms.Where(room => room.GenderType == genderType);
        }

        rooms = SelectedPriceSort switch
        {
            PriceLowToHigh => rooms.OrderBy(room => room.MonthlyPrice).ThenBy(room => room.BuildingName).ThenBy(room => room.RoomNumber),
            PriceHighToLow => rooms.OrderByDescending(room => room.MonthlyPrice).ThenBy(room => room.BuildingName).ThenBy(room => room.RoomNumber),
            _ => rooms.OrderBy(room => room.BuildingName).ThenBy(room => room.FloorNumber).ThenBy(room => room.RoomNumber)
        };

        var selectedRoomId = SelectedRoom?.Id;
        AvailableRooms.Clear();
        foreach (var room in rooms)
        {
            AvailableRooms.Add(room);
        }

        if (selectedRoomId.HasValue && AvailableRooms.All(room => room.Id != selectedRoomId.Value))
        {
            SelectedRoom = AvailableRooms.FirstOrDefault();
        }

        NotifyUiState();
    }

    private static string FormatFloor(int floorNumber) => $"Tầng {floorNumber}";

    private static bool TryParseFloor(string? selectedFloor, out int floorNumber)
    {
        const string prefix = "Tầng ";
        floorNumber = 0;
        return selectedFloor is not null
            && selectedFloor.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            && int.TryParse(selectedFloor[prefix.Length..], out floorNumber);
    }

    private static string FormatRoomType(int capacity) => $"Phòng {capacity} người";

    private static bool TryParseRoomType(string? selectedRoomType, out int capacity)
    {
        const string prefix = "Phòng ";
        const string suffix = " người";
        capacity = 0;
        return selectedRoomType is not null
            && selectedRoomType.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            && selectedRoomType.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)
            && int.TryParse(selectedRoomType[prefix.Length..^suffix.Length], out capacity);
    }

    private async Task SubmitAsync()
    {
        ClearError();
        StatusMessage = null;
        if (SelectedRoom is null)
        {
            SetError("Vui lòng chọn phòng còn trống trước khi gửi yêu cầu.");
            return;
        }

        if (!AcceptsDormRules)
        {
            SetError("Bạn cần đồng ý với nội quy KTX trước khi gửi yêu cầu.");
            return;
        }

        IsBusy = true;
        NotifyUiState();
        try
        {
            await _registrationService.CreateRegistrationAsync(new CreateRoomRegistrationRequest
            {
                RoomId = SelectedRoom.Id,
                ContractTermMonths = ContractTermMonths,
                IncludesInternet = false,
                Note = Note
            });

            StatusMessage = "Đăng ký phòng thành công.";
            await LoadAsync();
            RegistrationSubmitted?.Invoke(this, EventArgs.Empty);
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

    private void NotifyUiState()
    {
        OnPropertyChanged(nameof(HasAvailableRooms));
        OnPropertyChanged(nameof(IsAvailableRoomsEmpty));
        OnPropertyChanged(nameof(HasRecentRegistrations));
        OnPropertyChanged(nameof(IsRecentRegistrationsEmpty));
        OnPropertyChanged(nameof(HasApprovedRegistration));
        OnPropertyChanged(nameof(HasPendingRegistration));
        OnPropertyChanged(nameof(HasPaymentPendingRegistration));
        OnPropertyChanged(nameof(IsExternallyLocked));
        OnPropertyChanged(nameof(IsRegistrationLocked));
        OnPropertyChanged(nameof(RegistrationLockMessage));
        OnPropertyChanged(nameof(AvailabilityStatusMessage));
        OnPropertyChanged(nameof(CanSubmitRegistration));
        _submitCommand.RaiseCanExecuteChanged();
    }
}
