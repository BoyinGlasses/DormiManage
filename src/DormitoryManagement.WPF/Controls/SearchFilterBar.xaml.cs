using System.Windows;
using System.Windows.Controls;

namespace DormitoryManagement.WPF.Controls;

public partial class SearchFilterBar : UserControl
{
    public static readonly DependencyProperty FilterContentProperty = DependencyProperty.Register(nameof(FilterContent), typeof(object), typeof(SearchFilterBar), new PropertyMetadata(null));

    public SearchFilterBar()
    {
        InitializeComponent();
    }

    public object? FilterContent
    {
        get => GetValue(FilterContentProperty);
        set => SetValue(FilterContentProperty, value);
    }
}
