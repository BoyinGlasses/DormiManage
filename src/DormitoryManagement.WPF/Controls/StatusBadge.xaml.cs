using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DormitoryManagement.WPF.Converters;

namespace DormitoryManagement.WPF.Controls;

public partial class StatusBadge : UserControl
{
    private static readonly Thickness DefaultPadding = new(8, 4, 8, 4);
    private static readonly CornerRadius DefaultCornerRadius = new(10);
    private static readonly Thickness DashboardPadding = new(12, 6, 12, 6);
    private static readonly CornerRadius DashboardCornerRadius = new(999);
    private static readonly StatusToBrushConverter BrushConverter = new();

    public static readonly DependencyProperty StatusProperty = DependencyProperty.Register(nameof(Status), typeof(object), typeof(StatusBadge), new PropertyMetadata(string.Empty, OnBadgePropertyChanged));
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(StatusBadge), new PropertyMetadata(string.Empty, OnBadgePropertyChanged));
    public static readonly DependencyProperty ToneProperty = DependencyProperty.Register(nameof(Tone), typeof(string), typeof(StatusBadge), new PropertyMetadata(string.Empty, OnBadgePropertyChanged));

    public StatusBadge()
    {
        InitializeComponent();
        UpdateVisualState();
    }

    public object Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string Tone
    {
        get => (string)GetValue(ToneProperty);
        set => SetValue(ToneProperty, value);
    }

    private string StatusText => string.IsNullOrWhiteSpace(Text) ? Status?.ToString() ?? string.Empty : Text;

    private static void OnBadgePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((StatusBadge)d).UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        if (BadgeLabel is null || BadgeIcon is null || BadgeBorder is null)
        {
            return;
        }

        var text = StatusText;
        BadgeLabel.Text = text;
        BadgeIcon.Text = GetIcon(text);
        ToolTip = text;

        var normalizedTone = Tone?.Trim().ToLowerInvariant();
        if (normalizedTone is "danger" or "success" or "neutral")
        {
            ApplyToneResources(normalizedTone);
            return;
        }

        BadgeBorder.Padding = DefaultPadding;
        BadgeBorder.CornerRadius = DefaultCornerRadius;
        BadgeBorder.Background = ResolveBrush("Background");
        BadgeBorder.BorderBrush = ResolveBrush("Border");

        var foreground = ResolveBrush("Foreground");
        BadgeIcon.Foreground = foreground;
        BadgeLabel.Foreground = foreground;
    }

    private void ApplyToneResources(string tone)
    {
        BadgeBorder.Padding = DashboardPadding;
        BadgeBorder.CornerRadius = DashboardCornerRadius;
        BadgeBorder.Background = FindBrush($"StudentDashboardBadge{ToPascalCase(tone)}BackgroundBrush") ?? ResolveBrush("Background");
        BadgeBorder.BorderBrush = FindBrush($"StudentDashboardBadge{ToPascalCase(tone)}BorderBrush") ?? ResolveBrush("Border");

        var foreground = FindBrush($"StudentDashboardBadge{ToPascalCase(tone)}ForegroundBrush") ?? ResolveBrush("Foreground");
        BadgeIcon.Foreground = foreground;
        BadgeLabel.Foreground = foreground;
    }

    private Brush ResolveBrush(string part)
    {
        return BrushConverter.Convert(Status, typeof(Brush), part, System.Globalization.CultureInfo.CurrentUICulture) as Brush
            ?? Brushes.Transparent;
    }

    private Brush? FindBrush(string resourceKey)
    {
        return TryFindResource(resourceKey) as Brush;
    }

    private static string ToPascalCase(string value)
    {
        return string.IsNullOrEmpty(value)
            ? string.Empty
            : char.ToUpperInvariant(value[0]) + value[1..];
    }

    private static string GetIcon(string status)
    {
        var key = status
            .Replace(" ", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("_", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("-", string.Empty, StringComparison.OrdinalIgnoreCase);

        return key switch
        {
            "Đãthanhtoán" or "Đãthanhtoánvàápdụng" => "\uE73E",
            "Chưathanhtoán" => "\uEA39",
            "Hếthạn" => "\uE711",
            "Available" or "Active" or "Approved" or "Paid" or "Success" or "Resolved" or "Staying" => "\uE73E",
            "Pending" or "Draft" or "Unpaid" or "InProgress" or "Maintenance" or "Paymentdue" => "\uE823",
            "Overdue" or "Rejected" or "Failed" or "Locked" or "Urgent" or "Full" => "\uEA39",
            "Closed" or "Cancelled" or "Inactive" or "Disabled" => "\uE711",
            "Open" or "New" or "Info" or "Low" => "\uE8BD",
            _ => "\uE946"
        };
    }
}