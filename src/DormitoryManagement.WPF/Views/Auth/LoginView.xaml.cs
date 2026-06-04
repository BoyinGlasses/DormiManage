using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using DormitoryManagement.WPF.ViewModels.Auth;

namespace DormitoryManagement.WPF.Views.Auth;

public partial class LoginView : UserControl
{
    private const double CompactLayoutThreshold = 980;
    private LoginViewModel? _viewModel;
    private bool _isPasswordVisible;
    private bool _isSynchronizingPasswordInputs;

    public LoginView()
    {
        InitializeComponent();
        DataContextChanged += LoginView_OnDataContextChanged;
    }

    private void UserControl_OnLoaded(object sender, RoutedEventArgs e)
    {
        UpdateResponsiveLayout();
        LoadHeroImage();
        ApplyViewModelToInputs();
        SyncPasswordVisualState();
    }

    private void UserControl_OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateResponsiveLayout();
    }

    private void VisiblePasswordInput_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isSynchronizingPasswordInputs || sender is not TextBox visiblePasswordInput)
        {
            return;
        }

        try
        {
            _isSynchronizingPasswordInputs = true;
            if (DataContext is LoginViewModel viewModel)
            {
                viewModel.Password = visiblePasswordInput.Text;
            }

            if (!string.Equals(PasswordInput.Password, visiblePasswordInput.Text, StringComparison.Ordinal))
            {
                PasswordInput.Password = visiblePasswordInput.Text;
            }
        }
        finally
        {
            _isSynchronizingPasswordInputs = false;
        }

        SyncPasswordVisualState();
    }

    private void PasswordVisibilityToggleButton_OnClick(object sender, RoutedEventArgs e)
    {
        _isPasswordVisible = !_isPasswordVisible;
        SyncPasswordVisualState();

        if (_isPasswordVisible)
        {
            VisiblePasswordInput.Focus();
            VisiblePasswordInput.CaretIndex = VisiblePasswordInput.Text.Length;
        }
        else
        {
            PasswordInput.Focus();
        }

        e.Handled = true;
    }

    private void ClickSafeLinkButton_OnClick(object sender, RoutedEventArgs e)
    {
        Keyboard.ClearFocus();
        e.Handled = true;
    }

    private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (_isSynchronizingPasswordInputs || sender is not PasswordBox passwordBox)
        {
            return;
        }

        try
        {
            _isSynchronizingPasswordInputs = true;
            if (DataContext is LoginViewModel viewModel)
            {
                viewModel.Password = passwordBox.Password;
            }

            if (!string.Equals(VisiblePasswordInput.Text, passwordBox.Password, StringComparison.Ordinal))
            {
                VisiblePasswordInput.Text = passwordBox.Password;
            }
        }
        finally
        {
            _isSynchronizingPasswordInputs = false;
        }

        SyncPasswordVisualState();
    }

    private void LoginView_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (_viewModel is not null)
        {
            _viewModel.ClearPasswordRequested -= ViewModel_OnClearPasswordRequested;
        }

        _viewModel = e.NewValue as LoginViewModel;
        if (_viewModel is not null)
        {
            _viewModel.ClearPasswordRequested += ViewModel_OnClearPasswordRequested;
        }

        if (!IsLoaded)
        {
            return;
        }

        ApplyViewModelToInputs();
        SyncPasswordVisualState();
    }

    private void ApplyViewModelToInputs()
    {
        if (_viewModel is not null)
        {
            PasswordInput.Password = _viewModel.Password;
            VisiblePasswordInput.Text = _viewModel.Password;
        }
        else
        {
            PasswordInput.Clear();
            VisiblePasswordInput.Clear();
        }
    }

    private void ViewModel_OnClearPasswordRequested(object? sender, EventArgs e)
    {
        PasswordInput.Clear();
        VisiblePasswordInput.Clear();
        SyncPasswordVisualState();
    }

    private void LoadHeroImage()
    {
        var heroImage = FindElement<Image>("HeroImage");
        if (heroImage is null || heroImage.Source is not null)
        {
            return;
        }

        var imagePath = ResolveHeroImagePath();
        if (imagePath is null)
        {
            return;
        }

        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
        bitmap.EndInit();
        bitmap.Freeze();
        heroImage.Source = bitmap;
    }

    private static string? ResolveHeroImagePath()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            var repoAsset = Path.Combine(current.FullName, "src", "DormitoryManagement.WPF", "Assets", "Images", "Login", "hero-image.png");
            if (File.Exists(repoAsset))
            {
                return repoAsset;
            }

            var outputAsset = Path.Combine(current.FullName, "Assets", "Images", "Login", "hero-image.png");
            if (File.Exists(outputAsset))
            {
                return outputAsset;
            }

            current = current.Parent;
        }

        return null;
    }

    private void SyncPasswordVisualState()
    {
        PasswordInput.Visibility = _isPasswordVisible ? Visibility.Collapsed : Visibility.Visible;
        VisiblePasswordInput.Visibility = _isPasswordVisible ? Visibility.Visible : Visibility.Collapsed;
        PasswordVisibilityToggleButton.Content = _isPasswordVisible ? "" : "";
    }

    private void UpdateResponsiveLayout()
    {
        var splitCanvasGrid = FindElement<Grid>("SplitCanvasGrid");
        var heroPanel = FindElement<Grid>("HeroPanel");
        var formPanel = FindElement<Border>("FormPanel");
        var mainCanvasBorder = FindElement<Border>("MainCanvasBorder");
        var headerContainer = FindElement<Grid>("HeaderContainer");
        var heroOverlayBorder = FindElement<Border>("HeroOverlayBorder");
        var formHost = FindElement<Grid>("FormHost");
        var footerGrid = FindElement<Grid>("FooterGrid");
        var footerBrandText = FindElement<TextBlock>("FooterBrandText");
        var footerLinksPanel = FindElement<WrapPanel>("FooterLinksPanel");
        var footerCopyrightText = FindElement<TextBlock>("FooterCopyrightText");
        if (splitCanvasGrid is null
            || heroPanel is null
            || formPanel is null
            || mainCanvasBorder is null
            || headerContainer is null
            || heroOverlayBorder is null
            || formHost is null
            || footerGrid is null
            || footerBrandText is null
            || footerLinksPanel is null
            || footerCopyrightText is null)
        {
            return;
        }

        var availableWidth = ActualWidth > 0 ? ActualWidth : Width;
        var useCompactLayout = availableWidth > 0 && availableWidth < CompactLayoutThreshold;

        splitCanvasGrid.RowDefinitions[0].Height = new GridLength(useCompactLayout ? 300 : 1, GridUnitType.Star);
        splitCanvasGrid.RowDefinitions[1].Height = useCompactLayout ? GridLength.Auto : new GridLength(0);
        splitCanvasGrid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
        splitCanvasGrid.ColumnDefinitions[1].Width = useCompactLayout ? new GridLength(0) : new GridLength(1, GridUnitType.Star);

        Grid.SetRow(heroPanel, 0);
        Grid.SetColumn(heroPanel, 0);
        Grid.SetRow(formPanel, useCompactLayout ? 1 : 0);
        Grid.SetColumn(formPanel, useCompactLayout ? 0 : 1);

        var horizontalInset = useCompactLayout ? 32d : 0d;
        var contentWidth = availableWidth > 0 ? Math.Min(Math.Max(availableWidth - horizontalInset, 0), 1440) : 1440;
        var footerTargetHeight = useCompactLayout ? 132d : 72d;
        var headerTargetHeight = 76d;
        var targetCanvasHeight = availableWidth > 0
            ? Math.Max(useCompactLayout ? 520d : 640d, (ActualHeight > 0 ? ActualHeight : Height) - headerTargetHeight - footerTargetHeight)
            : double.NaN;

        mainCanvasBorder.Margin = useCompactLayout ? new Thickness(16) : new Thickness(0);
        mainCanvasBorder.Width = contentWidth;
        mainCanvasBorder.Height = targetCanvasHeight;
        headerContainer.Margin = useCompactLayout ? new Thickness(16, 0, 16, 0) : new Thickness(24, 0, 24, 0);
        headerContainer.Width = contentWidth;
        heroOverlayBorder.Margin = useCompactLayout ? new Thickness(24) : new Thickness(48);
        formHost.Margin = useCompactLayout ? new Thickness(32, 24, 32, 32) : new Thickness(96, 32, 96, 32);
        footerGrid.Margin = useCompactLayout ? new Thickness(16, 0, 16, 0) : new Thickness(32, 0, 32, 0);
        footerGrid.Width = Math.Max(contentWidth - (useCompactLayout ? 32 : 64), 0);

        if (useCompactLayout)
        {
            Grid.SetRow(footerBrandText, 0);
            Grid.SetColumn(footerBrandText, 0);
            Grid.SetColumnSpan(footerBrandText, 3);
            Grid.SetRow(footerLinksPanel, 1);
            Grid.SetColumn(footerLinksPanel, 0);
            Grid.SetColumnSpan(footerLinksPanel, 3);
            Grid.SetRow(footerCopyrightText, 2);
            Grid.SetColumn(footerCopyrightText, 0);
            Grid.SetColumnSpan(footerCopyrightText, 3);

            footerGrid.RowDefinitions[0].Height = GridLength.Auto;
            footerGrid.RowDefinitions[1].Height = GridLength.Auto;
            if (footerGrid.RowDefinitions.Count == 2)
            {
                footerGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }

            footerBrandText.Margin = new Thickness(0, 0, 0, 12);
            footerLinksPanel.Margin = new Thickness(0, 0, 0, 12);
            footerCopyrightText.Margin = new Thickness(0);
            footerBrandText.HorizontalAlignment = HorizontalAlignment.Center;
            footerLinksPanel.HorizontalAlignment = HorizontalAlignment.Center;
            footerCopyrightText.HorizontalAlignment = HorizontalAlignment.Center;
        }
        else
        {
            if (footerGrid.RowDefinitions.Count > 2)
            {
                footerGrid.RowDefinitions.RemoveAt(2);
            }

            Grid.SetRow(footerBrandText, 0);
            Grid.SetColumn(footerBrandText, 0);
            Grid.SetColumnSpan(footerBrandText, 1);
            Grid.SetRow(footerLinksPanel, 0);
            Grid.SetColumn(footerLinksPanel, 1);
            Grid.SetColumnSpan(footerLinksPanel, 1);
            Grid.SetRow(footerCopyrightText, 0);
            Grid.SetColumn(footerCopyrightText, 2);
            Grid.SetColumnSpan(footerCopyrightText, 1);

            footerGrid.RowDefinitions[0].Height = GridLength.Auto;
            footerGrid.RowDefinitions[1].Height = new GridLength(0);
            footerBrandText.Margin = new Thickness(0);
            footerLinksPanel.Margin = new Thickness(0);
            footerCopyrightText.Margin = new Thickness(0);
            footerBrandText.HorizontalAlignment = HorizontalAlignment.Left;
            footerLinksPanel.HorizontalAlignment = HorizontalAlignment.Center;
            footerCopyrightText.HorizontalAlignment = HorizontalAlignment.Right;
        }
    }

    private T? FindElement<T>(string name)
        where T : DependencyObject
    {
        return FindName(name) as T;
    }
}
