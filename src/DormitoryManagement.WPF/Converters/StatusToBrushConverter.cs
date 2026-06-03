using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace DormitoryManagement.WPF.Converters;

public sealed class StatusToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var text = value?.ToString() ?? string.Empty;
        var key = text
            .Replace(" ", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("_", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("-", string.Empty, StringComparison.OrdinalIgnoreCase);
        var tone = key switch
        {
            "Đãthanhtoán" or "Đãthanhtoánvàápdụng" => "success",
            "Chưathanhtoán" => "danger",
            "Hếthạn" => "neutral",
            "Active" or "Available" or "Paid" or "Success" or "Approved" or "Visible" or "Resolved" or "Completed" or "Staying" or "Nooutstandingbalance" or "Noopenticket" => "success",
            "Pending" or "Partial" or "InProgress" or "Assigned" or "Draft" or "NotRegistered" or "Maintenance" or "Medium" or "Unpaid" or "Paymentdue" => "warning",
            "Locked" or "Overdue" or "Rejected" or "Failed" or "Hidden" or "Urgent" or "Critical" or "High" or "Full" => "danger",
            "New" or "Open" or "Info" or "Low" => "info",
            "Closed" or "Cancelled" or "Inactive" or "Disabled" => "neutral",
            _ => "neutral"
        };
        var part = parameter?.ToString() ?? "Background";

        return (tone, part) switch
        {
            ("success", "Foreground") => Brush(15, 122, 85),
            ("success", "Border") => Brush(167, 243, 208),
            ("success", _) => Brush(220, 252, 231),
            ("warning", "Foreground") => Brush(180, 83, 9),
            ("warning", "Border") => Brush(252, 211, 77),
            ("warning", _) => Brush(254, 243, 199),
            ("danger", "Foreground") => Brush(185, 28, 28),
            ("danger", "Border") => Brush(252, 165, 165),
            ("danger", _) => Brush(254, 226, 226),
            ("info", "Foreground") => Brush(47, 95, 152),
            ("info", "Border") => Brush(191, 219, 254),
            ("info", _) => Brush(228, 238, 248),
            (_, "Foreground") => Brush(82, 97, 111),
            (_, "Border") => Brush(217, 226, 236),
            _ => Brush(238, 242, 246)
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;

    private static SolidColorBrush Brush(byte r, byte g, byte b) => new(Color.FromRgb(r, g, b));
}
