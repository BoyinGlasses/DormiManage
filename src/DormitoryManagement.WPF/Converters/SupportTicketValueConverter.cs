using System;
using System.Globalization;
using System.Windows.Data;
using DormitoryManagement.Application.DTOs.SupportTickets;
using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.WPF.Converters;

public sealed class SupportTicketValueConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (parameter is string converterMode
            && string.Equals(converterMode, "TicketReference", StringComparison.Ordinal))
        {
            if (value is SupportTicketDto ticket)
            {
                if (!string.IsNullOrWhiteSpace(ticket.StudentCode)
                    && ticket.StudentCode.StartsWith("#SP-", StringComparison.OrdinalIgnoreCase))
                {
                    return ticket.StudentCode;
                }

                if (ticket.Id != Guid.Empty)
                {
                    return $"#SP-{ticket.Id.ToString("N", CultureInfo.InvariantCulture)[..8].ToUpperInvariant()}";
                }
            }

            if (value is Guid ticketId)
            {
                return $"#SP-{ticketId.ToString("N", CultureInfo.InvariantCulture)[..8].ToUpperInvariant()}";
            }
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
            SupportTicketCategory.Account => "Kỹ thuật",
            SupportTicketCategory.Security => "An ninh",
            SupportTicketCategory.Other => "Dịch vụ",
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
