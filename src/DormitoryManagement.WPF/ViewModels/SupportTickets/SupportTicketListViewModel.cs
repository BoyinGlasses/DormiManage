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
    private readonly ISupportTicketService _service;
    private readonly ICurrentUserService _currentUser;
    private readonly List<SupportTicketDto> _allTickets = new();
    private bool _hasLoaded;
    private bool _isCreateFormOpen;
    private bool _areFiltersOpen;
    private SupportTicketDto? _selectedTicket;
    private string _title = string.Empty;
    private string _description = string.Empty;
    private SupportTicketCategory _category = SupportTicketCategory.Other;
    private PriorityLevel _priority = PriorityLevel.Medium;
    private SupportTicketStatus _statusToApply = SupportTicketStatus.InProgress;
    private string? _statusNote;
    private string _selectedStatusFilter = "All statuses";
    private string _selectedCategoryFilter = "All categories";
    private string _selectedPriorityFilter = "All priorities";
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
        ToggleCreateFormCommand = new RelayCommand(() => IsCreateFormOpen = !IsCreateFormOpen);
        ToggleFiltersCommand = new RelayCommand(() => AreFiltersOpen = !AreFiltersOpen);
        SelectTicketCommand = new RelayCommand(parameter =>
        {
            if (parameter is SupportTicketDto ticket)
            {
                SelectedTicket = ticket;
            }
        });
        SecondaryTicketActionCommand = new RelayCommand(_ => { });

        StatusFilters.Add("All statuses");
        foreach (var value in Enum.GetNames<SupportTicketStatus>()) StatusFilters.Add(value);
        CategoryFilters.Add("All categories");
        foreach (var value in Enum.GetNames<SupportTicketCategory>()) CategoryFilters.Add(value);
        PriorityFilters.Add("All priorities");
        foreach (var value in Enum.GetNames<PriorityLevel>()) PriorityFilters.Add(value);
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
    public ICommand ToggleCreateFormCommand { get; }
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
        : $"Hiển thị 1-{Tickets.Count} trên {TotalTicketCount} yêu cầu";

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

    public SupportTicketCategory Category { get => _category; set => SetProperty(ref _category, value); }
    public PriorityLevel Priority { get => _priority; set => SetProperty(ref _priority, value); }
    public SupportTicketStatus StatusToApply { get => _statusToApply; set => SetProperty(ref _statusToApply, value); }
    public string? StatusNote { get => _statusNote; set => SetProperty(ref _statusNote, value); }
    public SupportTicketDto? SelectedTicket
    {
        get => _selectedTicket;
        set
        {
            if (SetProperty(ref _selectedTicket, value))
            {
                if (value is not null)
                {
                    StatusToApply = value.Status;
                }
            }
        }
    }
    public string SelectedStatusFilter { get => _selectedStatusFilter; set => SetProperty(ref _selectedStatusFilter, value); }
    public string SelectedCategoryFilter { get => _selectedCategoryFilter; set => SetProperty(ref _selectedCategoryFilter, value); }
    public string SelectedPriorityFilter { get => _selectedPriorityFilter; set => SetProperty(ref _selectedPriorityFilter, value); }

    public string? SuccessMessage
    {
        get => _successMessage;
        private set
        {
            if (SetProperty(ref _successMessage, value)) OnPropertyChanged(nameof(HasSuccessMessage));
        }
    }

    public bool HasSuccessMessage => !string.IsNullOrWhiteSpace(SuccessMessage);

    public string? TitleError
    {
        get => _titleError;
        private set
        {
            if (SetProperty(ref _titleError, value)) OnPropertyChanged(nameof(HasTitleError));
        }
    }

    public string? DescriptionError
    {
        get => _descriptionError;
        private set
        {
            if (SetProperty(ref _descriptionError, value)) OnPropertyChanged(nameof(HasDescriptionError));
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
        ClearError();
        SuccessMessage = null;
        TitleError = null;
        DescriptionError = null;
        IsCreateFormOpen = true;
        if (string.IsNullOrWhiteSpace(Title))
        {
            TitleError = "Enter a ticket title.";
        }

        if (string.IsNullOrWhiteSpace(Description))
        {
            DescriptionError = "Enter a ticket description.";
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
                Category = Category,
                Priority = Priority
            });
            _allTickets.Insert(0, ticket);
            ApplyFilters();
            SelectedTicket = ticket;
            Title = string.Empty;
            Description = string.Empty;
            SuccessMessage = "Support ticket created.";
            IsCreateFormOpen = false;
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
            SetError("Select a ticket before updating status.");
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
            StatusNote = null;
            SuccessMessage = "Ticket status updated.";
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

    private void ApplyFilters()
    {
        IEnumerable<SupportTicketDto> tickets = _allTickets;
        if (Enum.TryParse<SupportTicketStatus>(SelectedStatusFilter, out var status)) tickets = tickets.Where(ticket => ticket.Status == status);
        if (Enum.TryParse<SupportTicketCategory>(SelectedCategoryFilter, out var category)) tickets = tickets.Where(ticket => ticket.Category == category);
        if (Enum.TryParse<PriorityLevel>(SelectedPriorityFilter, out var priority)) tickets = tickets.Where(ticket => ticket.Priority == priority);

        Tickets.Clear();
        foreach (var ticket in tickets) Tickets.Add(ticket);
        NotifyUiState();
    }

    private void ClearFilters()
    {
        SelectedStatusFilter = "All statuses";
        SelectedCategoryFilter = "All categories";
        SelectedPriorityFilter = "All priorities";
        ApplyFilters();
    }

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
    }
}
