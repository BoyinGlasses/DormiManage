using System.Windows;
using System.Windows.Controls;

namespace DormitoryManagement.WPF.Controls;

public partial class ErrorState : UserControl
{
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(nameof(Title), typeof(string), typeof(ErrorState), new PropertyMetadata("Something went wrong"));
    public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(nameof(Message), typeof(string), typeof(ErrorState), new PropertyMetadata(string.Empty));

    public ErrorState()
    {
        InitializeComponent();
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }
}
