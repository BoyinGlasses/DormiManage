using System.Windows.Controls;
using DormitoryManagement.WPF.ViewModels.Students;

namespace DormitoryManagement.WPF.Views.Students;

public partial class StudentProfileView : UserControl
{
    public StudentProfileView()
    {
        InitializeComponent();
    }

    private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is StudentProfileViewModel viewModel && viewModel.LoadCommand.CanExecute(null))
        {
            viewModel.LoadCommand.Execute(null);
        }
    }
}
