using DormitoryManagement.Application.Abstractions.Auth;
using DormitoryManagement.WPF.Common;

namespace DormitoryManagement.WPF.ViewModels.Profile;

public sealed class ProfileViewModel : ViewModelBase
{
    public ProfileViewModel(ICurrentUserService currentUser)
    {
        var user = currentUser.CurrentUser;
        DisplayName = user?.FullName ?? currentUser.FullName ?? currentUser.UserName ?? "Guest";
        Username = user?.Username ?? currentUser.UserName ?? "-";
        Email = user?.Email ?? currentUser.Email ?? "-";
        RoleName = user?.RoleName ?? currentUser.Roles.FirstOrDefault() ?? "-";
        IdentityLabel = user?.StudentId?.ToString("N")
            ?? user?.ManagerId?.ToString("N")
            ?? user?.BuildingId?.ToString("N")
            ?? user?.UserId.ToString("N")
            ?? "-";
        AvatarInitial = DisplayName[..1].ToUpperInvariant();
    }

    public string DisplayName { get; }
    public string Username { get; }
    public string Email { get; }
    public string RoleName { get; }
    public string IdentityLabel { get; }
    public string AvatarInitial { get; }
}
