using System.ComponentModel;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using DormitoryManagement.WPF.ViewModels.Dashboard;

using MediaColor = System.Windows.Media.Color;
using MediaSolidColorBrush = System.Windows.Media.SolidColorBrush;
using ScottPlotBar = ScottPlot.Bar;
using ScottPlotColor = ScottPlot.Color;

namespace DormitoryManagement.WPF.Views.Dashboard;

public partial class AdminDashboardView : UserControl
{
    private AdminDashboardViewModel? _subscribedViewModel;

    public AdminDashboardView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        RenderRevenueChart();

        if (DataContext is AdminDashboardViewModel viewModel && viewModel.RefreshCommand.CanExecute(null))
        {
            viewModel.RefreshCommand.Execute(null);
        }
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (_subscribedViewModel is not null)
        {
            _subscribedViewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }

        _subscribedViewModel = e.NewValue as AdminDashboardViewModel;

        if (_subscribedViewModel is not null)
        {
            _subscribedViewModel.PropertyChanged += OnViewModelPropertyChanged;
        }

        RenderRevenueChart();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (_subscribedViewModel is not null)
        {
            _subscribedViewModel.PropertyChanged -= OnViewModelPropertyChanged;
            _subscribedViewModel = null;
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(AdminDashboardViewModel.RevenuePoints))
        {
            RenderRevenueChart();
        }
    }

    private void RenderRevenueChart()
    {
        if (DataContext is not AdminDashboardViewModel viewModel || !viewModel.HasRevenueData)
        {
            RevenuePlot.Plot.Clear();
            RevenuePlot.Refresh();
            return;
        }

        var points = viewModel.RevenuePoints.ToArray();
        var positions = Enumerable.Range(1, points.Length).Select(static index => (double)index).ToArray();
        var labels = points.Select(static point => point.Month).ToArray();
        var accentColor = ToResourceColor("AccentBrush", "#0F766E");
        var bars = positions.Zip(points, (position, point) => new ScottPlotBar
        {
            Position = position,
            Value = point.Revenue,
            Size = 0.56,
            FillColor = accentColor,
            LineColor = ToScottPlotColor("#0B5D56"),
            LineWidth = 1,
            ValueLabel = FormatRevenue(point.Revenue),
            LabelOnTop = true,
        }).ToList();

        var plot = RevenuePlot.Plot;
        plot.Clear();
        plot.FigureBackground.Color = ScottPlot.Colors.Transparent;
        plot.DataBackground.Color = ToScottPlotColor("#F8FAFC");
        plot.Axes.Color(ToScottPlotColor("#52616F"));
        plot.Axes.Bottom.Label.Text = "Month";
        plot.Axes.Left.Label.Text = "Revenue (VND)";
        plot.Axes.Bottom.SetTicks(positions, labels);
        plot.Grid.MajorLineColor = ToScottPlotColor("#D9E2EC");
        plot.Grid.MinorLineColor = ToScottPlotColor("#E9EEF5");

        var barPlot = plot.Add.Bars(bars);
        barPlot.ValueLabelStyle.ForeColor = ToScottPlotColor("#172033");
        barPlot.ValueLabelStyle.FontSize = 11;

        plot.Axes.SetLimitsX(0.35, points.Length + 0.65);
        plot.Axes.SetLimitsY(0, Math.Max(points.Max(static point => point.Revenue) * 1.18, 1));
        RevenuePlot.Refresh();
    }

    private ScottPlotColor ToResourceColor(string resourceKey, string fallbackHex)
    {
        return ToScottPlotColor(TryFindColorResource(resourceKey) ?? ToMediaColor(fallbackHex));
    }

    private static ScottPlotColor ToScottPlotColor(string hex)
    {
        return ToScottPlotColor(ToMediaColor(hex));
    }

    private static ScottPlotColor ToScottPlotColor(MediaColor color)
    {
        var hex = color.A == byte.MaxValue
            ? $"#{color.R:X2}{color.G:X2}{color.B:X2}"
            : $"#{color.R:X2}{color.G:X2}{color.B:X2}{color.A:X2}";
        return ScottPlotColor.FromHex(hex);
    }

    private MediaColor? TryFindColorResource(string resourceKey)
    {
        return TryFindResource(resourceKey) is MediaSolidColorBrush brush ? brush.Color : null;
    }

    private static MediaColor ToMediaColor(string hex)
    {
        return (MediaColor)ColorConverter.ConvertFromString(hex);
    }

    private static string FormatRevenue(double value)
    {
        return value >= 1_000_000 ? $"{value / 1_000_000:N1}M" : value.ToString("N0");
    }
}
