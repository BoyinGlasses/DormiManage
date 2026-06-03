namespace DormitoryManagement.WPF.Navigation;

public sealed class SessionState
{
    public event EventHandler? Changed;

    public void NotifyChanged() => Changed?.Invoke(this, EventArgs.Empty);
}
