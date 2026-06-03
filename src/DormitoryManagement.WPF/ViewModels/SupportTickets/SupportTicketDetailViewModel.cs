using System.Collections.ObjectModel;
using System.Windows.Input;
using DormitoryManagement.Application.DTOs.SupportTickets;
using DormitoryManagement.Application.Services.SupportTickets;
using DormitoryManagement.Domain.Enums;
using DormitoryManagement.WPF.Common;

namespace DormitoryManagement.WPF.ViewModels.SupportTickets;

public sealed class SupportTicketDetailViewModel : ViewModelBase
{
    private readonly ISupportTicketService _service;
    private string _ticketId = string.Empty;
    private string _title = "No ticket selected";
    private string _responseMessage = string.Empty;
    private SupportTicketStatus _status = SupportTicketStatus.New;
    private string? _statusNote;
    private string? _successMessage;

    public SupportTicketDetailViewModel(ISupportTicketService service)
    {
        _service = service;
        AddResponseCommand = new AsyncRelayCommand(AddResponseAsync);
        UpdateStatusCommand = new AsyncRelayCommand(UpdateStatusAsync);
    }

    public ObservableCollection<TicketResponseRow> Responses { get; } = new();
    public Array StatusOptions => Enum.GetValues<SupportTicketStatus>();
    public ICommand AddResponseCommand { get; }
    public ICommand UpdateStatusCommand { get; }

    public string TicketId { get => _ticketId; set => SetProperty(ref _ticketId, value); }
    public string Title { get => _title; set => SetProperty(ref _title, value); }
    public string ResponseMessage { get => _responseMessage; set => SetProperty(ref _responseMessage, value); }
    public SupportTicketStatus Status { get => _status; set => SetProperty(ref _status, value); }
    public string? StatusNote { get => _statusNote; set => SetProperty(ref _statusNote, value); }

    public string? SuccessMessage
    {
        get => _successMessage;
        private set
        {
            if (SetProperty(ref _successMessage, value)) OnPropertyChanged(nameof(HasSuccessMessage));
        }
    }

    public bool HasSuccessMessage => !string.IsNullOrWhiteSpace(SuccessMessage);

    private async Task AddResponseAsync()
    {
        ClearError();
        SuccessMessage = null;
        if (!Guid.TryParse(TicketId, out var ticketId))
        {
            SetError("Enter a valid ticket id before adding a response.");
            return;
        }

        if (string.IsNullOrWhiteSpace(ResponseMessage))
        {
            SetError("Enter a response message.");
            return;
        }

        IsBusy = true;
        try
        {
            await _service.AddResponseAsync(ticketId, ResponseMessage.Trim());
            Responses.Add(new TicketResponseRow { Author = "Current user", Message = ResponseMessage.Trim(), CreatedAt = DateTime.Now });
            ResponseMessage = string.Empty;
            SuccessMessage = "Response added.";
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

    private async Task UpdateStatusAsync()
    {
        ClearError();
        SuccessMessage = null;
        if (!Guid.TryParse(TicketId, out var ticketId))
        {
            SetError("Enter a valid ticket id before updating status.");
            return;
        }

        IsBusy = true;
        try
        {
            await _service.UpdateStatusAsync(new UpdateSupportTicketStatusRequest
            {
                TicketId = ticketId,
                Status = Status,
                Note = StatusNote
            });
            SuccessMessage = "Ticket status updated.";
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
}

public sealed class TicketResponseRow
{
    public string Author { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
