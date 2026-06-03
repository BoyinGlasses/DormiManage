using System.Windows.Controls;
using DormitoryManagement.WPF.ViewModels.Registrations;

namespace DormitoryManagement.WPF.Views.Registrations;

public partial class RoomRegistrationView : UserControl
{
    public RoomRegistrationView()
    {
        InitializeComponent();
    }

    private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is RoomRegistrationViewModel viewModel && viewModel.LoadCommand.CanExecute(null))
        {
            viewModel.LoadCommand.Execute(null);
        }
    }
}
