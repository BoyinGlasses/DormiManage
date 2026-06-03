using System.Windows.Controls;
using DormitoryManagement.WPF.ViewModels.Students;

namespace DormitoryManagement.WPF.Views.Students;

public partial class StudentListView : UserControl
{
    public StudentListView()
    {
        InitializeComponent();
    }

    private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is StudentListViewModel viewModel && viewModel.LoadCommand.CanExecute(null))
        {
            viewModel.LoadCommand.Execute(null);
        }
    }
}
