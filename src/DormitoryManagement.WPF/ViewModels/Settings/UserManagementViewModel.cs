using System.Collections.ObjectModel;
using System.Windows.Input;
using DormitoryManagement.Application.DTOs.Users;
using DormitoryManagement.Application.Services.Users;
using DormitoryManagement.WPF.Common;

namespace DormitoryManagement.WPF.ViewModels.Settings;

public sealed class UserManagementViewModel : ViewModelBase
{
    private readonly IUserManagementService _userManagementService;

    public UserManagementViewModel(IUserManagementService userManagementService)
    {
        _userManagementService = userManagementService;
        LoadCommand = new AsyncRelayCommand(LoadAsync);
    }

    public ObservableCollection<UserListItemDto> Users { get; } = new();
    public ICommand LoadCommand { get; }

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    private async Task LoadAsync()
    {
        var result = await _userManagementService.GetUsersAsync();
        Users.Clear();
        foreach (var user in result.Items)
        {
            Users.Add(user);
        }
    }
}
