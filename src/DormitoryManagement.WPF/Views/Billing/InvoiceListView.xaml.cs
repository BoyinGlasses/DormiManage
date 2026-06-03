using System.Windows.Controls;
using DormitoryManagement.WPF.ViewModels.Billing;

namespace DormitoryManagement.WPF.Views.Billing;

public partial class InvoiceListView : UserControl
{
    public InvoiceListView()
    {
        InitializeComponent();
    }

    private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is InvoiceListViewModel viewModel && viewModel.LoadCommand.CanExecute(null))
        {
            viewModel.LoadCommand.Execute(null);
        }
    }
}
