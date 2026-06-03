using System.Windows;
using System.Windows.Controls;
using DormitoryManagement.WPF.ViewModels.Auth;

namespace DormitoryManagement.WPF.Views.Auth;

public partial class RegisterView : UserControl
{
    private RegisterViewModel? _viewModel;

    public RegisterView()
    {
        InitializeComponent();
        DataContextChanged += RegisterView_OnDataContextChanged;
    }

    private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is RegisterViewModel viewModel && sender is PasswordBox passwordBox)
        {
            viewModel.Password = passwordBox.Password;
        }
    }

    private void ConfirmPasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is RegisterViewModel viewModel && sender is PasswordBox passwordBox)
        {
            viewModel.ConfirmPassword = passwordBox.Password;
        }
    }

    private void RegisterView_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (_viewModel is not null)
        {
            _viewModel.ClearPasswordRequested -= ViewModel_OnClearPasswordRequested;
        }

        _viewModel = e.NewValue as RegisterViewModel;
        if (_viewModel is not null)
        {
            _viewModel.ClearPasswordRequested += ViewModel_OnClearPasswordRequested;
        }
    }

    private void ViewModel_OnClearPasswordRequested(object? sender, EventArgs e)
    {
        PasswordInput.Clear();
        ConfirmPasswordInput.Clear();
    }
}
