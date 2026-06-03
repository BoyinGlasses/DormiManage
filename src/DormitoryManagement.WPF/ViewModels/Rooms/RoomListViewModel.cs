using System.Collections.ObjectModel;
using System.Windows.Input;
using DormitoryManagement.Application.DTOs.Rooms;
using DormitoryManagement.Application.Services.Rooms;
using DormitoryManagement.Domain.Enums;
using DormitoryManagement.WPF.Common;

namespace DormitoryManagement.WPF.ViewModels.Rooms;

public sealed class RoomListViewModel : ViewModelBase
{
    private readonly IRoomService _roomService;
    private readonly List<RoomDto> _allRooms = new();
    private bool _hasLoaded;
    private string _keyword = string.Empty;
    private string _selectedBuilding = "All buildings";
    private string _selectedStatus = "All statuses";
    private string _selectedGenderType = "All gender types";
    private int _totalCount;

    public RoomListViewModel(IRoomService roomService)
    {
        _roomService = roomService;
        LoadCommand = new AsyncRelayCommand(LoadAsync);
        ApplyFiltersCommand = new AsyncRelayCommand(LoadAsync);
        ClearFiltersCommand = new RelayCommand(ClearFilters);

        BuildingOptions.Add("All buildings");
        StatusOptions.Add("All statuses");
        foreach (var value in Enum.GetNames<RoomStatus>()) StatusOptions.Add(value);
        GenderTypeOptions.Add("All gender types");
        GenderTypeOptions.Add(RoomGenderType.Male.ToString());
        GenderTypeOptions.Add(RoomGenderType.Female.ToString());
    }

    public ObservableCollection<RoomDto> Rooms { get; } = new();
    public ObservableCollection<string> BuildingOptions { get; } = new();
    public ObservableCollection<string> StatusOptions { get; } = new();
    public ObservableCollection<string> GenderTypeOptions { get; } = new();

    public ICommand LoadCommand { get; }
    public ICommand RefreshCommand => LoadCommand;
    public ICommand ApplyFiltersCommand { get; }
    public ICommand ClearFiltersCommand { get; }
    public bool HasRooms => Rooms.Count > 0;
    public bool IsRoomsEmpty => _hasLoaded && !IsBusy && Rooms.Count == 0;

    public string Keyword
    {
        get => _keyword;
        set => SetProperty(ref _keyword, value);
    }

    public string SelectedBuilding
    {
        get => _selectedBuilding;
        set => SetProperty(ref _selectedBuilding, value);
    }

    public string SelectedStatus
    {
        get => _selectedStatus;
        set => SetProperty(ref _selectedStatus, value);
    }

    public string SelectedGenderType
    {
        get => _selectedGenderType;
        set => SetProperty(ref _selectedGenderType, value);
    }

    public int TotalCount
    {
        get => _totalCount;
        private set => SetProperty(ref _totalCount, value);
    }

    private async Task LoadAsync()
    {
        IsBusy = true;
        ClearError();
        NotifyUiState();
        try
        {
            var request = new RoomFilterRequest
            {
                Status = Enum.TryParse<RoomStatus>(SelectedStatus, out var status) ? status : null,
                GenderType = Enum.TryParse<RoomGenderType>(SelectedGenderType, out var genderType) ? genderType : null,
                PageNumber = 1,
                PageSize = 100
            };

            var result = await _roomService.GetRoomsAsync(request);
            _allRooms.Clear();
            _allRooms.AddRange(result.Items);
            TotalCount = result.TotalCount;

            RebuildBuildingOptions();
            ApplyLocalFilters();
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
        Keyword = string.Empty;
        SelectedBuilding = "All buildings";
        SelectedStatus = "All statuses";
        SelectedGenderType = "All gender types";
        ApplyLocalFilters();
    }

    private void RebuildBuildingOptions()
    {
        var current = SelectedBuilding;
        BuildingOptions.Clear();
        BuildingOptions.Add("All buildings");
        foreach (var building in _allRooms.Select(room => room.BuildingName).Where(name => !string.IsNullOrWhiteSpace(name)).Distinct().OrderBy(name => name))
        {
            BuildingOptions.Add(building);
        }

        SelectedBuilding = BuildingOptions.Contains(current) ? current : "All buildings";
    }

    private void ApplyLocalFilters()
    {
        IEnumerable<RoomDto> rooms = _allRooms;

        if (!string.IsNullOrWhiteSpace(Keyword))
        {
            rooms = rooms.Where(room => room.RoomNumber.Contains(Keyword, StringComparison.OrdinalIgnoreCase)
                || room.BuildingName.Contains(Keyword, StringComparison.OrdinalIgnoreCase));
        }

        if (SelectedBuilding != "All buildings")
        {
            rooms = rooms.Where(room => room.BuildingName == SelectedBuilding);
        }

        Rooms.Clear();
        foreach (var room in rooms)
        {
            Rooms.Add(room);
        }

        NotifyUiState();
    }

    private void NotifyUiState()
    {
        OnPropertyChanged(nameof(HasRooms));
        OnPropertyChanged(nameof(IsRoomsEmpty));
    }
}
