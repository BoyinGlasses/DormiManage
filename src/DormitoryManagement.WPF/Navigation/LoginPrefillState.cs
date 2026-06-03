namespace DormitoryManagement.WPF.Navigation;

public interface ILoginPrefillState
{
    void SetEmail(string email);
    string? ConsumeEmail();
}

public sealed class LoginPrefillState : ILoginPrefillState
{
    private string? _email;

    public void SetEmail(string email)
    {
        _email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
    }

    public string? ConsumeEmail()
    {
        var email = _email;
        _email = null;
        return email;
    }
}
