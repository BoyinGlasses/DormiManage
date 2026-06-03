using System.Windows;
using System.Windows.Controls;
using DormitoryManagement.WPF.ViewModels.Registrations;

namespace DormitoryManagement.WPF.Views.Registrations;

public partial class RegistrationApprovalView : UserControl
{
    public RegistrationApprovalView()
    {
        InitializeComponent();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is RegistrationApprovalViewModel viewModel && viewModel.LoadCommand.CanExecute(null))
        {
            viewModel.LoadCommand.Execute(null);
        }
    }
}
