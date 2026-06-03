using System.Windows.Controls;
using DormitoryManagement.WPF.ViewModels.Audit;

namespace DormitoryManagement.WPF.Views.Audit;

public partial class AuditLogListView : UserControl
{
    public AuditLogListView()
    {
        InitializeComponent();
    }

    private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is AuditLogListViewModel viewModel && viewModel.LoadCommand.CanExecute(null))
        {
            viewModel.LoadCommand.Execute(null);
        }
    }
}
