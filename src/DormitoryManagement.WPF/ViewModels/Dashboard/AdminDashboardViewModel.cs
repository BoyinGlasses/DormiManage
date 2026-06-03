using System.Collections.ObjectModel;
using System.Windows.Input;
using DormitoryManagement.Application.Services.Dashboard;
using DormitoryManagement.Application.Services.Rooms;
using DormitoryManagement.Domain.Enums;
using DormitoryManagement.WPF.Common;
using DormitoryManagement.WPF.Navigation;
using DormitoryManagement.WPF.ViewModels.Billing;
using DormitoryManagement.WPF.ViewModels.Registrations;
using DormitoryManagement.WPF.ViewModels.SupportTickets;

namespace DormitoryManagement.WPF.ViewModels.Dashboard;

public sealed class AdminDashboardViewModel : ViewModelBase
{
    private readonly IDashboardService _dashboardService;
    private readonly IRoomService _roomService;
    private readonly INavigationService _navigationService;
    private int _totalStudents;
    private int _occupiedRooms;
    private int _availableRooms;
    private decimal _monthlyRevenue;
    private int _unpaidInvoices;
    private int _openTickets;

    public AdminDashboardViewModel(
        IDashboardService dashboardService,
        IRoomService roomService,
        INavigationService navigationService)
    {
        _dashboardService = dashboardService;
        _roomService = roomService;
        _navigationService = navigationService;
        RefreshCommand = new AsyncRelayCommand(LoadAsync);
        ReviewRegistrationsCommand = new RelayCommand(_ => _navigationService.NavigateTo<RegistrationApprovalViewModel>());
        OpenBillingCommand = new RelayCommand(_ => _navigationService.NavigateTo<InvoiceListViewModel>());
        OpenTicketsCommand = new RelayCommand(_ => _navigationService.NavigateTo<SupportTicketListViewModel>());
    }

    public ICommand RefreshCommand { get; }
    public ICommand ReviewRegistrationsCommand { get; }
    public ICommand OpenBillingCommand { get; }
    public ICommand OpenTicketsCommand { get; }
    public ObservableCollection<RevenueChartPoint> RevenuePoints { get; private set; } = new();
    public ObservableCollection<DashboardActivityItem> RecentActivities { get; } = new();
    public ObservableCollection<OverdueInvoiceItem> OverdueInvoices { get; } = new();
    public ObservableCollection<PendingRegistrationItem> PendingRegistrations { get; } = new();
    public ObservableCollection<OpenTicketItem> OpenSupportTickets { get; } = new();
    public ObservableCollection<OccupancySummaryItem> OccupancySummaries { get; } = new();

    public int TotalStudents { get => _totalStudents; private set => SetProperty(ref _totalStudents, value); }
    public int OccupiedRooms { get => _occupiedRooms; private set => SetProperty(ref _occupiedRooms, value); }
    public int AvailableRooms { get => _availableRooms; private set => SetProperty(ref _availableRooms, value); }
    public decimal MonthlyRevenue { get => _monthlyRevenue; private set { if (SetProperty(ref _monthlyRevenue, value)) OnPropertyChanged(nameof(MonthlyRevenueText)); } }
    public int UnpaidInvoices { get => _unpaidInvoices; private set => SetProperty(ref _unpaidInvoices, value); }
    public int OpenTickets { get => _openTickets; private set => SetProperty(ref _openTickets, value); }
    public string MonthlyRevenueText => MonthlyRevenue.ToString("N0") + " VND";
    public string MonthlyRevenueTrendText => MonthlyRevenue > 0 ? "+12%" : "No payments";
    public string UnpaidInvoicesTrendText => UnpaidInvoices > 0 ? UnpaidInvoices + " need follow-up" : "Clear";
    public string OpenTicketsTrendText => OpenTickets > 0 ? "Needs triage" : "No open ticket";
    public bool HasRevenueData => RevenuePoints.Any(point => point.Revenue > 0);

    private async Task LoadAsync()
    {
        IsBusy = true;
        ClearError();
        try
        {
            var dashboard = await _dashboardService.GetAdminDashboardAsync();
            var debt = await _dashboardService.GetDebtSummaryAsync();
            var revenue = await _dashboardService.GetRevenueByMonthAsync(DateTime.Today.Year);
            var registrations = await _dashboardService.GetPendingRegistrationsAsync();
            var overdueInvoices = await _dashboardService.GetOverdueInvoicesAsync();
            var supportTickets = await _dashboardService.GetOpenSupportTicketsAsync();
            var recentActivities = await _dashboardService.GetRecentActivitiesAsync();
            var occupancy = await _dashboardService.GetRoomOccupancyAsync();
            var rooms = await _roomService.GetRoomsAsync();

            TotalStudents = dashboard.TotalStudents;
            OccupiedRooms = dashboard.OccupiedRooms > 0 ? dashboard.OccupiedRooms : rooms.Items.Sum(room => room.CurrentOccupancy);
            AvailableRooms = rooms.Items.Count(room => room.Status == RoomStatus.Available);
            MonthlyRevenue = dashboard.MonthlyRevenue;
            UnpaidInvoices = debt.UnpaidInvoiceCount;
            OpenTickets = dashboard.OpenTickets;
            OnPropertyChanged(nameof(MonthlyRevenueTrendText));
            OnPropertyChanged(nameof(UnpaidInvoicesTrendText));
            OnPropertyChanged(nameof(OpenTicketsTrendText));

            RevenuePoints = new ObservableCollection<RevenueChartPoint>(
                revenue.Select(point => new RevenueChartPoint(point.Label, (double)point.Value)));
            OnPropertyChanged(nameof(RevenuePoints));
            OnPropertyChanged(nameof(HasRevenueData));

            RecentActivities.Clear();
            foreach (var activity in recentActivities)
            {
                RecentActivities.Add(new DashboardActivityItem
                {
                    Time = activity.Time,
                    Title = activity.Title,
                    Description = activity.Description
                });
            }
            OnPropertyChanged(nameof(RecentActivities));

            OverdueInvoices.Clear();
            foreach (var invoice in overdueInvoices)
            {
                OverdueInvoices.Add(new OverdueInvoiceItem
                {
                    InvoiceNumber = invoice.InvoiceNumber,
                    Student = invoice.Student,
                    DueDate = invoice.DueDate,
                    Remaining = invoice.Remaining,
                    Status = invoice.Status
                });
            }
            OnPropertyChanged(nameof(OverdueInvoices));

            PendingRegistrations.Clear();
            foreach (var registration in registrations)
            {
                PendingRegistrations.Add(new PendingRegistrationItem
                {
                    RegistrationId = registration.RegistrationId,
                    Student = registration.Student,
                    PreferredRoom = registration.PreferredRoom,
                    SubmittedAt = registration.SubmittedAt,
                    Status = registration.Status
                });
            }
            OnPropertyChanged(nameof(PendingRegistrations));

            OpenSupportTickets.Clear();
            foreach (var ticket in supportTickets)
            {
                OpenSupportTickets.Add(new OpenTicketItem
                {
                    Ticket = ticket.Ticket,
                    Priority = ticket.Priority,
                    Status = ticket.Status,
                    AssignedStaff = ticket.AssignedStaff
                });
            }
            OnPropertyChanged(nameof(OpenSupportTickets));

            OccupancySummaries.Clear();
            foreach (var building in occupancy)
            {
                OccupancySummaries.Add(new OccupancySummaryItem
                {
                    Building = "Building " + building.BuildingCode,
                    Occupied = building.Occupied,
                    Capacity = building.Capacity
                });
            }
            OnPropertyChanged(nameof(OccupancySummaries));
        }
        catch (Exception ex)
        {
            SetError(ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }
}

public sealed record RevenueChartPoint(string Month, double Revenue);

public sealed class DashboardActivityItem
{
    public string Time { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public sealed class OverdueInvoiceItem
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public string Student { get; set; } = string.Empty;
    public string DueDate { get; set; } = string.Empty;
    public decimal Remaining { get; set; }
    public string Status { get; set; } = "Overdue";
}

public sealed class PendingRegistrationItem
{
    public Guid RegistrationId { get; set; }
    public string Student { get; set; } = string.Empty;
    public string PreferredRoom { get; set; } = string.Empty;
    public string SubmittedAt { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
}

public sealed class OpenTicketItem
{
    public string Ticket { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string AssignedStaff { get; set; } = string.Empty;
}

public sealed class OccupancySummaryItem
{
    public string Building { get; set; } = string.Empty;
    public int Occupied { get; set; }
    public int Capacity { get; set; }
    public double OccupancyPercent => Capacity == 0 ? 0 : Math.Round(Occupied * 100d / Capacity, 1);
    public string OccupancyText => Occupied + " / " + Capacity + " beds";
}
