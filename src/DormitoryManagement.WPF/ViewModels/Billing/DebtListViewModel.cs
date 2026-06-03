using DormitoryManagement.Application.Services.Dashboard;
using DormitoryManagement.WPF.Common;

namespace DormitoryManagement.WPF.ViewModels.Billing;

public sealed class DebtListViewModel : ViewModelBase
{
    private readonly IDashboardService _service;

    public DebtListViewModel(IDashboardService service)
    {
        _service = service;
    }

    public IDashboardService Service => _service;
}
