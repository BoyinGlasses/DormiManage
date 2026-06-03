using DormitoryManagement.WPF.Common;

namespace DormitoryManagement.WPF.Navigation;

public interface INavigationService
{
    void NavigateTo<TViewModel>() where TViewModel : ViewModelBase;
}
