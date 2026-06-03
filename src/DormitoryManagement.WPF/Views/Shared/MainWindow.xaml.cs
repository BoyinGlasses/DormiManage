using MahApps.Metro.Controls;
using DormitoryManagement.WPF.ViewModels;

namespace DormitoryManagement.WPF.Views.Shared;

public partial class MainWindow : MetroWindow
{
    public MainWindow(ShellViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
