using DormitoryManagement.WPF.Common;
using Microsoft.Extensions.DependencyInjection;

namespace DormitoryManagement.WPF.Navigation;

public sealed class NavigationService : INavigationService, IDisposable
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly NavigationStore _navigationStore;
    private IServiceScope? _currentScope;

    public NavigationService(IServiceScopeFactory scopeFactory, NavigationStore navigationStore)
    {
        _scopeFactory = scopeFactory;
        _navigationStore = navigationStore;
    }

    public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase
    {
        var scope = _scopeFactory.CreateScope();
        try
        {
            var viewModel = scope.ServiceProvider.GetRequiredService<TViewModel>();
            var oldScope = _currentScope;
            _currentScope = scope;
            _navigationStore.CurrentViewModel = viewModel;
            oldScope?.Dispose();
        }
        catch
        {
            scope.Dispose();
            throw;
        }
    }

    public void Dispose()
    {
        _currentScope?.Dispose();
    }
}
