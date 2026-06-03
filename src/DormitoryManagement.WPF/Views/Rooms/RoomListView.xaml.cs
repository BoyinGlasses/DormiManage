using System.Windows;
using System.Windows.Controls;
using DormitoryManagement.WPF.ViewModels.Rooms;

namespace DormitoryManagement.WPF.Views.Rooms;

public partial class RoomListView : UserControl
{
    public RoomListView()
    {
        InitializeComponent();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is RoomListViewModel viewModel && viewModel.LoadCommand.CanExecute(null))
        {
            viewModel.LoadCommand.Execute(null);
        }
    }
}
