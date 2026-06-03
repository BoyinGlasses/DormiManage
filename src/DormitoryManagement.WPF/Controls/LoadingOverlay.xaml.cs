using System.Windows;
using System.Windows.Controls;

namespace DormitoryManagement.WPF.Controls;

public partial class LoadingOverlay : UserControl
{
    public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(nameof(Message), typeof(string), typeof(LoadingOverlay), new PropertyMetadata("Loading..."));

    public LoadingOverlay()
    {
        InitializeComponent();
    }

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }
}
