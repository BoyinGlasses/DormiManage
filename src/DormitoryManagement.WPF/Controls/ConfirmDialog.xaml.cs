using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DormitoryManagement.WPF.Controls;

public partial class ConfirmDialog : UserControl
{
    public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(ConfirmDialog), new PropertyMetadata(false));
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(nameof(Title), typeof(string), typeof(ConfirmDialog), new PropertyMetadata("Confirm action"));
    public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(nameof(Message), typeof(string), typeof(ConfirmDialog), new PropertyMetadata(string.Empty));
    public static readonly DependencyProperty ReasonProperty = DependencyProperty.Register(nameof(Reason), typeof(string), typeof(ConfirmDialog), new PropertyMetadata(string.Empty));
    public static readonly DependencyProperty ErrorMessageProperty = DependencyProperty.Register(nameof(ErrorMessage), typeof(string), typeof(ConfirmDialog), new PropertyMetadata(string.Empty));
    public static readonly DependencyProperty ConfirmCommandProperty = DependencyProperty.Register(nameof(ConfirmCommand), typeof(ICommand), typeof(ConfirmDialog), new PropertyMetadata(null));
    public static readonly DependencyProperty CancelCommandProperty = DependencyProperty.Register(nameof(CancelCommand), typeof(ICommand), typeof(ConfirmDialog), new PropertyMetadata(null));

    public ConfirmDialog()
    {
        InitializeComponent();
    }

    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
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

    public string Reason
    {
        get => (string)GetValue(ReasonProperty);
        set => SetValue(ReasonProperty, value);
    }

    public string ErrorMessage
    {
        get => (string)GetValue(ErrorMessageProperty);
        set => SetValue(ErrorMessageProperty, value);
    }

    public ICommand? ConfirmCommand
    {
        get => (ICommand?)GetValue(ConfirmCommandProperty);
        set => SetValue(ConfirmCommandProperty, value);
    }

    public ICommand? CancelCommand
    {
        get => (ICommand?)GetValue(CancelCommandProperty);
        set => SetValue(CancelCommandProperty, value);
    }
}
