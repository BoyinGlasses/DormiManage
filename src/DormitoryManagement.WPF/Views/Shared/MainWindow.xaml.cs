using System.Windows;
using DormitoryManagement.WPF.ViewModels;

namespace DormitoryManagement.WPF.Views.Shared;

public partial class MainWindow : Window
{
    public MainWindow(ShellViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
