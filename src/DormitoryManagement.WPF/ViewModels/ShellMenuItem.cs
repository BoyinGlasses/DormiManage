using DormitoryManagement.WPF.Common;

namespace DormitoryManagement.WPF.ViewModels;

public sealed class ShellMenuItem : ObservableObject
{
    private bool _isActive;

    public ShellMenuItem(string title, string key, string icon = "")
    {
        Title = title;
        Key = key;
        Icon = icon;
    }

    public string Title { get; }
    public string Key { get; }
    public string Icon { get; }

    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }
}
