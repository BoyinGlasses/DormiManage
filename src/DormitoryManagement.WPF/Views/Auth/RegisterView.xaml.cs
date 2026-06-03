using System.Windows;
using System.Windows.Controls;
using DormitoryManagement.WPF.ViewModels.Auth;

namespace DormitoryManagement.WPF.Views.Auth;

public partial class RegisterView : UserControl
{
    private const double CompactLayoutThreshold = 1180;
    private RegisterViewModel? _viewModel;

    public RegisterView()
    {
        InitializeComponent();
        DataContextChanged += RegisterView_OnDataContextChanged;
    }

    private void UserControl_OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdateResponsiveLayout();
    }

    private void UserControl_OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateResponsiveLayout();
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

    private void UpdateResponsiveLayout()
    {
        var splitCanvasGrid = FindElement<Grid>("SplitCanvasGrid");
        var heroPanel = FindElement<Grid>("HeroPanel");
        var heroCardBorder = FindElement<Border>("HeroCardBorder");
        var brandRow = FindElement<StackPanel>("BrandRow");
        var formScrollViewer = FindElement<ScrollViewer>("FormScrollViewer");
        var formContentHost = FindElement<Grid>("FormContentHost");
        if (splitCanvasGrid is null
            || heroPanel is null
            || heroCardBorder is null
            || brandRow is null
            || formScrollViewer is null
            || formContentHost is null)
        {
            return;
        }

        var availableWidth = ActualWidth > 0 ? ActualWidth : Width;
        var useCompactLayout = availableWidth > 0 && availableWidth < CompactLayoutThreshold;

        splitCanvasGrid.RowDefinitions[0].Height = useCompactLayout ? new GridLength(320) : new GridLength(1, GridUnitType.Star);
        splitCanvasGrid.RowDefinitions[1].Height = useCompactLayout ? GridLength.Auto : new GridLength(0);
        splitCanvasGrid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
        splitCanvasGrid.ColumnDefinitions[1].Width = useCompactLayout ? new GridLength(0) : new GridLength(1, GridUnitType.Star);

        Grid.SetRow(heroPanel, 0);
        Grid.SetColumn(heroPanel, 0);
        Grid.SetRow(formScrollViewer, useCompactLayout ? 1 : 0);
        Grid.SetColumn(formScrollViewer, useCompactLayout ? 0 : 1);

        heroPanel.MinWidth = useCompactLayout ? 0 : 420;
        heroCardBorder.Margin = useCompactLayout ? new Thickness(24) : new Thickness(48);
        brandRow.Margin = useCompactLayout ? new Thickness(24, 24, 24, 0) : new Thickness(40, 32, 40, 0);
        formContentHost.Margin = useCompactLayout ? new Thickness(24, 16, 24, 24) : new Thickness(40, 20, 40, 32);
        formContentHost.MaxWidth = useCompactLayout ? 720 : 460;
    }

    private T? FindElement<T>(string name)
        where T : DependencyObject
    {
        return FindName(name) as T;
    }
}
