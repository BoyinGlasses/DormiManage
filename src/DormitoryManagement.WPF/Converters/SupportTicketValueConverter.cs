using System;
using System.Globalization;
using System.Windows.Data;
using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.WPF.Converters;

public sealed class SupportTicketValueConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (parameter is string converterMode
            && string.Equals(converterMode, "TicketReference", StringComparison.Ordinal)
            && value is Guid ticketId)
        {
            return $"#SP-{ticketId.ToString("N", CultureInfo.InvariantCulture)[..8].ToUpperInvariant()}";
        }

        return value switch
        {
            SupportTicketStatus.New => "Chờ xử lý",
            SupportTicketStatus.Assigned => "Đang thực hiện",
            SupportTicketStatus.InProgress => "Đang thực hiện",
            SupportTicketStatus.Resolved => "Đã giải quyết",
            SupportTicketStatus.Rejected => "Từ chối",
            SupportTicketStatus.Closed => "Đã giải quyết",
            SupportTicketCategory.Complaint => "Khiếu nại",
            SupportTicketCategory.Maintenance => "Cơ sở vật chất",
            SupportTicketCategory.Billing => "Thanh toán",
            SupportTicketCategory.Vehicle => "Xe cộ",
            SupportTicketCategory.Account => "Tài khoản",
            SupportTicketCategory.Security => "An ninh",
            SupportTicketCategory.Other => "Khác",
            PriorityLevel.Low => "Thấp",
            PriorityLevel.Medium => "Trung bình",
            PriorityLevel.High => "Cao",
            PriorityLevel.Critical => "Khẩn cấp",
            PriorityLevel.Urgent => "Khẩn ngay",
            _ => value?.ToString() ?? string.Empty
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}
