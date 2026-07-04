using System.Collections.ObjectModel;
using System.Windows.Input;
using DormitoryManagement.Application.Abstractions.Auth;
using DormitoryManagement.Application.DTOs.SupportTickets;
using DormitoryManagement.Application.Services.SupportTickets;
using DormitoryManagement.Domain.Constants;
using DormitoryManagement.Domain.Enums;
using DormitoryManagement.WPF.Common;

namespace DormitoryManagement.WPF.ViewModels.SupportTickets;

public sealed class SupportTicketListViewModel : ViewModelBase
{
    private const int PageSize = 4;
    private readonly ISupportTicketService _service;
    private readonly ICurrentUserService _currentUser;
    private readonly List<SupportTicketDto> _allTickets = new();
    private readonly List<SupportTicketDto> _filteredTickets = new();
    private bool _hasLoaded;
    private bool _isCreateFormOpen;
    private bool _areFiltersOpen;
    private int _currentPage = 1;
    private SupportTicketDto? _selectedTicket;
    private string _title = string.Empty;
    private string _description = string.Empty;
    private SupportTicketCategory? _category;
    private PriorityLevel _priority = PriorityLevel.Medium;
    private SupportTicketStatus _statusToApply = SupportTicketStatus.InProgress;
    private string? _statusNote;
    private string _selectedStatusFilter = "Tất cả trạng thái";
    private string _selectedCategoryFilter = "Tất cả loại vấn đề";
    private string _selectedPriorityFilter = "Tất cả mức độ ưu tiên";
    private string? _successMessage;
    private string? _titleError;
    private string? _descriptionError;

    public SupportTicketListViewModel(ISupportTicketService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
        LoadCommand = new AsyncRelayCommand(LoadAsync);
        CreateTicketCommand = new AsyncRelayCommand(CreateTicketAsync);
        UpdateStatusCommand = new AsyncRelayCommand(UpdateStatusAsync);
        ApplyFiltersCommand = new RelayCommand(ApplyFilters);
        ClearFiltersCommand = new RelayCommand(ClearFilters);
        PreviousPageCommand = new RelayCommand(_ => GoToPreviousPage());
        NextPageCommand = new RelayCommand(_ => GoToNextPage());
        ToggleCreateFormCommand = new RelayCommand(ToggleCreateForm);
        CloseCreateFormCommand = new RelayCommand(CloseCreateForm);
        ToggleFiltersCommand = new RelayCommand(() => AreFiltersOpen = !AreFiltersOpen);
        SelectTicketCommand = new RelayCommand(parameter =>
        {
            if (parameter is SupportTicketDto ticket)
            {
                SelectedTicket = ticket;
            }
        });
        SecondaryTicketActionCommand = new RelayCommand(_ => { });

        StatusFilters.Add("Tất cả trạng thái");
        foreach (var value in Enum.GetNames<SupportTicketStatus>())
        {
            StatusFilters.Add(value);
        }

        CategoryFilters.Add("Tất cả loại vấn đề");
        foreach (var value in Enum.GetNames<SupportTicketCategory>())
        {
            CategoryFilters.Add(value);
        }

        PriorityFilters.Add("Tất cả mức độ ưu tiên");
        foreach (var value in Enum.GetNames<PriorityLevel>())
        {
            PriorityFilters.Add(value);
        }
    }

    public ObservableCollection<SupportTicketDto> Tickets { get; } = new();
    public ObservableCollection<string> StatusFilters { get; } = new();
    public ObservableCollection<string> CategoryFilters { get; } = new();
    public ObservableCollection<string> PriorityFilters { get; } = new();
    public Array Categories => Enum.GetValues<SupportTicketCategory>();
    public IEnumerable<PriorityLevel> Priorities => Enum.GetValues<PriorityLevel>().Where(priority => priority != PriorityLevel.Urgent);
    public IEnumerable<SupportTicketStatus> StatusOptions => Enum.GetValues<SupportTicketStatus>();
    public ICommand LoadCommand { get; }
    public ICommand CreateTicketCommand { get; }
    public ICommand UpdateStatusCommand { get; }
    public ICommand ApplyFiltersCommand { get; }
    public ICommand ClearFiltersCommand { get; }
    public ICommand PreviousPageCommand { get; }
    public ICommand NextPageCommand { get; }
    public ICommand ToggleCreateFormCommand { get; }
    public ICommand CloseCreateFormCommand { get; }
    public ICommand ToggleFiltersCommand { get; }
    public ICommand SelectTicketCommand { get; }
    public ICommand SecondaryTicketActionCommand { get; }
    public bool IsStaffUser => _currentUser.IsInRole(RoleNames.Admin)
        || _currentUser.IsInRole(RoleNames.Manager);
    public bool HasTickets => Tickets.Count > 0;
    public bool IsTicketsEmpty => _hasLoaded && !IsBusy && Tickets.Count == 0;
    public int TotalTicketCount => _allTickets.Count;
    public int OpenTicketCount => _allTickets.Count(ticket => ticket.Status is SupportTicketStatus.New or SupportTicketStatus.Assigned or SupportTicketStatus.InProgress);
    public int ResolvedTicketCount => _allTickets.Count(ticket => ticket.Status is SupportTicketStatus.Resolved or SupportTicketStatus.Closed);
    public string TotalTicketCountText => TotalTicketCount.ToString();
    public string OpenTicketCountText => OpenTicketCount.ToString();
    public string ResolvedTicketCountText => ResolvedTicketCount.ToString();
    public string RecentTicketSummaryText => TotalTicketCount == 0
        ? "Chưa có yêu cầu hỗ trợ nào được tạo."
        : $"Hiển thị {Tickets.Count} trên {TotalTicketCount} yêu cầu gần đây.";
    public string TicketFooterSummaryText => TotalTicketCount == 0
        ? "Hiển thị 0-0 trên 0 yêu cầu"
        : $"Hiển thị {CurrentRangeStart}-{CurrentRangeEnd} trên {TotalFilteredTicketCount} yêu cầu";
    public int TotalFilteredTicketCount => _filteredTickets.Count;
    public int CurrentPage => _currentPage;
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalFilteredTicketCount / (double)PageSize));
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
    private int CurrentRangeStart => TotalFilteredTicketCount == 0 ? 0 : ((CurrentPage - 1) * PageSize) + 1;
    private int CurrentRangeEnd => TotalFilteredTicketCount == 0 ? 0 : Math.Min(CurrentPage * PageSize, TotalFilteredTicketCount);

    public bool IsCreateFormOpen
    {
        get => _isCreateFormOpen;
        set => SetProperty(ref _isCreateFormOpen, value);
    }

    public bool AreFiltersOpen
    {
        get => _areFiltersOpen;
        set => SetProperty(ref _areFiltersOpen, value);
    }

    public string Title
    {
        get => _title;
        set
        {
            if (SetProperty(ref _title, value))
            {
                TitleError = null;
            }
        }
    }

    public string Description
    {
        get => _description;
        set
        {
            if (SetProperty(ref _description, value))
            {
                DescriptionError = null;
            }
        }
    }

    public SupportTicketCategory? Category
    {
        get => _category;
        set => SetProperty(ref _category, value);
    }

    public PriorityLevel Priority
    {
        get => _priority;
        set => SetProperty(ref _priority, value);
    }

    public SupportTicketStatus StatusToApply
    {
        get => _statusToApply;
        set => SetProperty(ref _statusToApply, value);
    }

    public string? StatusNote
    {
        get => _statusNote;
        set => SetProperty(ref _statusNote, value);
    }

    public SupportTicketDto? SelectedTicket
    {
        get => _selectedTicket;
        set
        {
            if (SetProperty(ref _selectedTicket, value) && value is not null)
            {
                StatusToApply = value.Status;
            }
        }
    }

    public string SelectedStatusFilter
    {
        get => _selectedStatusFilter;
        set => SetProperty(ref _selectedStatusFilter, value);
    }

    public string SelectedCategoryFilter
    {
        get => _selectedCategoryFilter;
        set => SetProperty(ref _selectedCategoryFilter, value);
    }

    public string SelectedPriorityFilter
    {
        get => _selectedPriorityFilter;
        set => SetProperty(ref _selectedPriorityFilter, value);
    }

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

    public string? TitleError
    {
        get => _titleError;
        private set
        {
            if (SetProperty(ref _titleError, value))
            {
                OnPropertyChanged(nameof(HasTitleError));
            }
        }
    }

    public string? DescriptionError
    {
        get => _descriptionError;
        private set
        {
            if (SetProperty(ref _descriptionError, value))
            {
                OnPropertyChanged(nameof(HasDescriptionError));
            }
        }
    }

    public bool HasTitleError => !string.IsNullOrWhiteSpace(TitleError);
    public bool HasDescriptionError => !string.IsNullOrWhiteSpace(DescriptionError);

    private async Task LoadAsync()
    {
        ClearError();
        IsBusy = true;
        NotifyUiState();
        try
        {
            var tickets = await _service.GetTicketsAsync();
            _allTickets.Clear();
            _allTickets.AddRange(tickets);
            ApplyFilters();
            SelectedTicket = IsStaffUser ? Tickets.FirstOrDefault() : null;
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

    private async Task CreateTicketAsync()
    {
        if (!IsCreateFormOpen)
        {
            return;
        }

        ClearError();
        SuccessMessage = null;
        ClearCreateValidation();

        if (string.IsNullOrWhiteSpace(Title))
        {
            TitleError = "Vui lòng nhập chủ đề yêu cầu.";
        }

        if (string.IsNullOrWhiteSpace(Description))
        {
            DescriptionError = "Vui lòng nhập mô tả yêu cầu.";
        }

        if (HasTitleError || HasDescriptionError)
        {
            return;
        }

        IsBusy = true;
        try
        {
            var ticket = await _service.CreateTicketAsync(new CreateSupportTicketRequest
            {
                StudentId = _currentUser.CurrentUser?.StudentId,
                Title = Title.Trim(),
                Description = Description.Trim(),
                Category = Category ?? SupportTicketCategory.Other,
                Priority = PriorityLevel.Medium
            });
            _allTickets.Insert(0, ticket);
            _currentPage = 1;
            ApplyFilters();
            SelectedTicket = ticket;
            ResetCreateDraft();
            ClearCreateValidation();
            IsCreateFormOpen = false;
            SuccessMessage = "Đã tạo yêu cầu hỗ trợ.";
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

    private async Task UpdateStatusAsync()
    {
        ClearError();
        SuccessMessage = null;
        if (SelectedTicket is null)
        {
            SetError("Vui lòng chọn yêu cầu trước khi cập nhật trạng thái.");
            return;
        }

        IsBusy = true;
        try
        {
            await _service.UpdateStatusAsync(new UpdateSupportTicketStatusRequest
            {
                TicketId = SelectedTicket.Id,
                Status = StatusToApply,
                Note = StatusNote
            });

            var updatedTicket = CloneTicket(SelectedTicket, StatusToApply);
            ReplaceTicket(updatedTicket);
            ApplyFilters();
            SelectedTicket = Tickets.FirstOrDefault(ticket => ticket.Id == updatedTicket.Id);
            StatusNote = null;
            SuccessMessage = "Đã cập nhật trạng thái yêu cầu.";
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

    private void ApplyFilters()
    {
        IEnumerable<SupportTicketDto> tickets = _allTickets;
        if (TryParseFilter(SelectedStatusFilter, out SupportTicketStatus status))
        {
            tickets = tickets.Where(ticket => ticket.Status == status);
        }

        if (TryParseFilter(SelectedCategoryFilter, out SupportTicketCategory category))
        {
            tickets = tickets.Where(ticket => ticket.Category == category);
        }

        if (TryParseFilter(SelectedPriorityFilter, out PriorityLevel priority))
        {
            tickets = tickets.Where(ticket => ticket.Priority == priority);
        }

        _filteredTickets.Clear();
        _filteredTickets.AddRange(tickets);

        if (_currentPage > TotalPages)
        {
            _currentPage = TotalPages;
        }

        Tickets.Clear();
        foreach (var ticket in _filteredTickets.Skip((CurrentPage - 1) * PageSize).Take(PageSize))
        {
            Tickets.Add(ticket);
        }

        NotifyUiState();
    }

    private void ClearFilters()
    {
        SelectedStatusFilter = "Tất cả trạng thái";
        SelectedCategoryFilter = "Tất cả loại vấn đề";
        SelectedPriorityFilter = "Tất cả mức độ ưu tiên";
        _currentPage = 1;
        ApplyFilters();
    }

    private void GoToPreviousPage()
    {
        if (!HasPreviousPage)
        {
            return;
        }

        _currentPage--;
        ApplyFilters();
    }

    private void GoToNextPage()
    {
        if (!HasNextPage)
        {
            return;
        }

        _currentPage++;
        ApplyFilters();
    }

    private void ToggleCreateForm()
    {
        if (IsCreateFormOpen)
        {
            CloseCreateForm();
            return;
        }

        OpenCreateForm();
    }

    private void OpenCreateForm()
    {
        ResetCreateDraft();
        ClearCreateValidation();
        ClearError();
        SuccessMessage = null;
        IsCreateFormOpen = true;
    }

    private void CloseCreateForm()
    {
        IsCreateFormOpen = false;
        ResetCreateDraft();
        ClearCreateValidation();
        ClearError();
    }

    private void ResetCreateDraft()
    {
        Title = string.Empty;
        Description = string.Empty;
        Category = null;
        Priority = PriorityLevel.Medium;
    }

    private void ClearCreateValidation()
    {
        TitleError = null;
        DescriptionError = null;
    }

    private void ReplaceTicket(SupportTicketDto updatedTicket)
    {
        var index = _allTickets.FindIndex(ticket => ticket.Id == updatedTicket.Id);
        if (index >= 0)
        {
            _allTickets[index] = updatedTicket;
        }
    }

    private static SupportTicketDto CloneTicket(SupportTicketDto ticket, SupportTicketStatus status) =>
        new()
        {
            Id = ticket.Id,
            StudentId = ticket.StudentId,
            StudentCode = ticket.StudentCode,
            StudentName = ticket.StudentName,
            CreatedBy = ticket.CreatedBy,
            AssignedTo = ticket.AssignedTo,
            Title = ticket.Title,
            Description = ticket.Description,
            Category = ticket.Category,
            Status = status,
            Priority = ticket.Priority,
            CreatedAt = ticket.CreatedAt,
            ResolvedAt = status is SupportTicketStatus.Resolved or SupportTicketStatus.Closed
                ? ticket.ResolvedAt ?? DateTime.UtcNow
                : null,
            Responses = ticket.Responses.ToArray()
        };

    private static bool TryParseFilter<TEnum>(string value, out TEnum result)
        where TEnum : struct, Enum =>
        Enum.TryParse(value, out result);

    private void NotifyUiState()
    {
        OnPropertyChanged(nameof(HasTickets));
        OnPropertyChanged(nameof(IsTicketsEmpty));
        OnPropertyChanged(nameof(IsStaffUser));
        OnPropertyChanged(nameof(TotalTicketCount));
        OnPropertyChanged(nameof(OpenTicketCount));
        OnPropertyChanged(nameof(ResolvedTicketCount));
        OnPropertyChanged(nameof(TotalTicketCountText));
        OnPropertyChanged(nameof(OpenTicketCountText));
        OnPropertyChanged(nameof(ResolvedTicketCountText));
        OnPropertyChanged(nameof(RecentTicketSummaryText));
        OnPropertyChanged(nameof(TicketFooterSummaryText));
        OnPropertyChanged(nameof(TotalFilteredTicketCount));
        OnPropertyChanged(nameof(CurrentPage));
        OnPropertyChanged(nameof(TotalPages));
        OnPropertyChanged(nameof(HasPreviousPage));
        OnPropertyChanged(nameof(HasNextPage));
    }
}
