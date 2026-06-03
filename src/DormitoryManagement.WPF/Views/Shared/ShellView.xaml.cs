using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using DormitoryManagement.WPF.ViewModels;

namespace DormitoryManagement.WPF.Views.Shared;

public partial class ShellView : UserControl
{
    private readonly DispatcherTimer _closeTimer;
    private bool _isSidebarOpen;
    private INotifyPropertyChanged? _observedViewModel;

    public ShellView()
    {
        InitializeComponent();

        SidebarPanel.IsHitTestVisible = false;
        _closeTimer = new DispatcherTimer { Interval = ShellViewAnimationSettings.CloseDelay };
        _closeTimer.Tick += OnCloseTimerTick;
        DataContextChanged += OnShellDataContextChanged;
        Loaded += (_, _) => UpdateContentHostBackground();
    }

    private void ShowSidebar()
    {
        _closeTimer.Stop();
        if (_isSidebarOpen)
        {
            return;
        }

        _isSidebarOpen = true;
        SidebarPanel.IsHitTestVisible = true;
        AnimateShellTo(
            ShellViewAnimationSettings.OpenOffset,
            ShellViewAnimationSettings.ContentOpenOffset,
            ShellViewAnimationSettings.OpenDuration,
            EasingMode.EaseOut);
    }

    private void ScheduleCloseSidebar()
    {
        _closeTimer.Stop();
        _closeTimer.Start();
    }

    private void CloseSidebar()
    {
        _closeTimer.Stop();
        if (!_isSidebarOpen && SidebarTransform.X == ShellViewAnimationSettings.HiddenOffset)
        {
            return;
        }

        _isSidebarOpen = false;
        AnimateShellTo(
            ShellViewAnimationSettings.HiddenOffset,
            ShellViewAnimationSettings.ContentClosedOffset,
            ShellViewAnimationSettings.CloseDuration,
            EasingMode.EaseIn,
            () =>
            {
                if (!_isSidebarOpen)
                {
                    SidebarPanel.IsHitTestVisible = false;
                }
            });
    }

    private void OnSidebarHotspotMouseEnter(object sender, MouseEventArgs e) => ShowSidebar();

    private void OnSidebarMouseEnter(object sender, MouseEventArgs e) => ShowSidebar();

    private void OnSidebarMouseLeave(object sender, MouseEventArgs e) => ScheduleCloseSidebar();

    private void OnSidebarKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) => ShowSidebar();

    private void OnCloseTimerTick(object? sender, EventArgs e)
    {
        if (IsPointerInsideSidebarOrHotspot())
        {
            ShowSidebar();
            return;
        }

        CloseSidebar();
    }

    private void AnimateShellTo(double sidebarOffset, double contentOffset, TimeSpan duration, EasingMode easingMode, Action? completed = null)
    {
        var sidebarAnimation = new DoubleAnimation
        {
            To = sidebarOffset,
            Duration = new Duration(duration),
            EasingFunction = new CubicEase { EasingMode = easingMode },
            FillBehavior = FillBehavior.HoldEnd
        };
        var contentAnimation = new ThicknessAnimation
        {
            To = new Thickness(contentOffset, 0, 0, 0),
            Duration = new Duration(duration),
            EasingFunction = new CubicEase { EasingMode = easingMode },
            FillBehavior = FillBehavior.HoldEnd
        };

        if (completed is not null)
        {
            sidebarAnimation.Completed += (_, _) => completed();
        }

        SidebarTransform.BeginAnimation(TranslateTransform.XProperty, sidebarAnimation, HandoffBehavior.SnapshotAndReplace);
        MainContentHost.BeginAnimation(MarginProperty, contentAnimation, HandoffBehavior.SnapshotAndReplace);
    }

    private void OnShellDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (_observedViewModel is not null)
        {
            _observedViewModel.PropertyChanged -= OnShellViewModelPropertyChanged;
        }

        _observedViewModel = e.NewValue as INotifyPropertyChanged;
        if (_observedViewModel is not null)
        {
            _observedViewModel.PropertyChanged += OnShellViewModelPropertyChanged;
        }

        UpdateContentHostBackground();
    }

    private void OnShellViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(ShellViewModel.IsStudentDashboardChrome))
        {
            Dispatcher.Invoke(UpdateContentHostBackground);
        }
    }

    private void UpdateContentHostBackground()
    {
        if (ContentHostBorder is null)
        {
            return;
        }

        var resourceKey = DataContext is ShellViewModel shellViewModel && shellViewModel.IsStudentDashboardChrome
            ? "StudentDashboardCanvasBrush"
            : "BackgroundBrush";

        ContentHostBorder.Background = TryFindResource(resourceKey) as Brush ?? Brushes.Transparent;
    }

    private bool IsPointerInsideSidebarOrHotspot()
    {
        var pointerTarget = Mouse.DirectlyOver as DependencyObject;

        return IsSelfOrDescendant(SidebarPanel, pointerTarget)
            || IsSelfOrDescendant(SidebarHotspot, pointerTarget);
    }

    private static bool IsSelfOrDescendant(DependencyObject root, DependencyObject? candidate)
    {
        while (candidate is not null)
        {
            if (ReferenceEquals(root, candidate))
            {
                return true;
            }

            candidate = VisualTreeHelper.GetParent(candidate);
        }

        return false;
    }
}

public static class ShellViewAnimationSettings
{
    public const double SidebarWidth = 312d;
    public const double HiddenOffset = -SidebarWidth;
    public const double OpenOffset = 0d;
    public const double HotspotWidth = 8d;
    public const double ContentClosedOffset = 0d;
    public const double ContentOpenOffset = SidebarWidth;

    public static TimeSpan OpenDuration { get; } = TimeSpan.FromMilliseconds(300);
    public static TimeSpan CloseDuration { get; } = TimeSpan.FromMilliseconds(250);
    public static TimeSpan CloseDelay { get; } = TimeSpan.FromMilliseconds(120);
}