using System.Windows.Controls;
using DormitoryManagement.WPF.ViewModels.Vehicles;

namespace DormitoryManagement.WPF.Views.Vehicles;

public partial class VehicleRegistrationView : UserControl
{
    public VehicleRegistrationView()
    {
        InitializeComponent();
    }

    private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is VehicleRegistrationViewModel viewModel && viewModel.LoadCommand.CanExecute(null))
        {
            viewModel.LoadCommand.Execute(null);
        }
    }
}
