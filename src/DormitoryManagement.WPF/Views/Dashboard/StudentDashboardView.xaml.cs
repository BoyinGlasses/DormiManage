using System.Windows;
using System.Windows.Controls;
using DormitoryManagement.WPF.ViewModels.Dashboard;

namespace DormitoryManagement.WPF.Views.Dashboard;

public partial class StudentDashboardView : UserControl
{
    public StudentDashboardView()
    {
        InitializeComponent();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is StudentDashboardViewModel viewModel && viewModel.RefreshCommand.CanExecute(null))
        {
            viewModel.RefreshCommand.Execute(null);
        }
    }
}
