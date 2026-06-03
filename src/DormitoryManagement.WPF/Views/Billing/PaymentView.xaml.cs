using System.Windows.Controls;
using DormitoryManagement.WPF.ViewModels.Billing;

namespace DormitoryManagement.WPF.Views.Billing;

public partial class PaymentView : UserControl
{
    public PaymentView()
    {
        InitializeComponent();
    }

    private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is PaymentViewModel viewModel && viewModel.LoadCommand.CanExecute(null))
        {
            viewModel.LoadCommand.Execute(null);
        }
    }
}
