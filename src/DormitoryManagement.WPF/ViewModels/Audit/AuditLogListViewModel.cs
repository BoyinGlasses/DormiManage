using System.Collections.ObjectModel;
using System.Windows.Input;
using DormitoryManagement.Application.Abstractions.Services;
using DormitoryManagement.Application.DTOs.Audit;
using DormitoryManagement.WPF.Common;

namespace DormitoryManagement.WPF.ViewModels.Audit;

public sealed class AuditLogListViewModel : ViewModelBase
{
    private readonly IAuditLogService _auditLogService;
    private bool _hasLoaded;
    private string? _searchText;

    public AuditLogListViewModel(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
        LoadCommand = new AsyncRelayCommand(LoadAsync);
        SearchCommand = new AsyncRelayCommand(LoadAsync);
        ClearSearchCommand = new RelayCommand(ClearSearch);
    }

    public ObservableCollection<AuditLogDto> Logs { get; } = new();
    public ICommand LoadCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand ClearSearchCommand { get; }
    public bool HasLogs => Logs.Count > 0;
    public bool IsLogsEmpty => _hasLoaded && !IsBusy && Logs.Count == 0;

    public string? SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    private async Task LoadAsync()
    {
        ClearError();
        IsBusy = true;
        NotifyState();
        try
        {
            var logs = await _auditLogService.GetRecentAsync(SearchText, 200);
            Logs.Clear();
            foreach (var log in logs)
            {
                Logs.Add(log);
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
            NotifyState();
        }
    }

    private void ClearSearch()
    {
        SearchText = null;
        LoadCommand.Execute(null);
    }

    private void NotifyState()
    {
        OnPropertyChanged(nameof(HasLogs));
        OnPropertyChanged(nameof(IsLogsEmpty));
    }
}
