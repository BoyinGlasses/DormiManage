using DormitoryManagement.WPF.Common;

namespace DormitoryManagement.WPF.Navigation;

public sealed class NavigationStore : ObservableObject
{
    private ViewModelBase? _currentViewModel;

    public ViewModelBase? CurrentViewModel
    {
        get => _currentViewModel;
        set => SetProperty(ref _currentViewModel, value);
    }
}
