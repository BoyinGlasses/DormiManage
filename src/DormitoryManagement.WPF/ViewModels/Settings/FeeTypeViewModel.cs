using System.Collections.ObjectModel;
using System.Windows.Input;
using DormitoryManagement.Application.DTOs.Settings;
using DormitoryManagement.Application.Services.Settings;
using DormitoryManagement.WPF.Common;

namespace DormitoryManagement.WPF.ViewModels.Settings;

public sealed class FeeTypeViewModel : ViewModelBase
{
    private readonly IFeeTypeService _feeTypeService;

    public FeeTypeViewModel(IFeeTypeService feeTypeService)
    {
        _feeTypeService = feeTypeService;
        LoadCommand = new AsyncRelayCommand(LoadAsync);
        SaveDraftCommand = new RelayCommand(() => StatusMessage = "Fee type draft is ready for service implementation.");
    }

    public ObservableCollection<FeeTypeDto> FeeTypes { get; } = new();
    public ICommand LoadCommand { get; }
    public ICommand SaveDraftCommand { get; }

    private string _code = string.Empty;
    public string Code
    {
        get => _code;
        set => SetProperty(ref _code, value);
    }

    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    private string? _unit;
    public string? Unit
    {
        get => _unit;
        set => SetProperty(ref _unit, value);
    }

    private bool _isRecurring;
    public bool IsRecurring
    {
        get => _isRecurring;
        set => SetProperty(ref _isRecurring, value);
    }

    private string? _statusMessage;
    public string? StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    private async Task LoadAsync()
    {
        var result = await _feeTypeService.GetFeeTypesAsync();
        FeeTypes.Clear();
        foreach (var feeType in result.Items)
        {
            FeeTypes.Add(feeType);
        }
    }
}
