using System.Windows.Controls;
using DormitoryManagement.WPF.ViewModels.SupportTickets;

namespace DormitoryManagement.WPF.Views.SupportTickets;

public partial class SupportTicketListView : UserControl
{
    public SupportTicketListView()
    {
        InitializeComponent();
    }

    private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is SupportTicketListViewModel viewModel && viewModel.LoadCommand.CanExecute(null))
        {
            viewModel.LoadCommand.Execute(null);
        }
    }
}
