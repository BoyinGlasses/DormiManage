using DormitoryManagement.Application.Abstractions.Auth;
using DormitoryManagement.Application.Abstractions.Data;
using DormitoryManagement.Application.Abstractions.Repositories;
using DormitoryManagement.Application.Abstractions.Services;
using DormitoryManagement.Application.Common;
using DormitoryManagement.Application.DTOs.Auth;
using DormitoryManagement.Application.Validation;
using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Application.Services.Auth;

public sealed class AuthService : IAuthService
{
    private const string InvalidLoginMessage = "Tài khoản hoặc mật khẩu không đúng.";
    private const string DisabledMessage = "Tài khoản đã bị vô hiệu hóa.";
    private const string LockedMessage = "Tài khoản đang bị khóa tạm thời.";

    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ISessionService _sessionService;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditLogService _auditLog;
    private readonly IDateTimeProvider _clock;
    private readonly SecurityOptions _securityOptions;

    public AuthService(
        IUserRepository users,
        IPasswordHasher passwordHasher,
        ISessionService sessionService,
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork,
        IAuditLogService auditLog,
        IDateTimeProvider clock,
        SecurityOptions securityOptions)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _sessionService = sessionService;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
        _auditLog = auditLog;
        _clock = clock;
        _securityOptions = securityOptions;
    }

    public async Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        RequestValidator.ValidateAndThrow(request);

        var loginValue = request.EmailOrUsernameOrStudentCode.Trim();
        var user = await _users.GetByEmailOrUsernameAsync(loginValue, ct)
            ?? await _users.GetByStudentCodeAsync(loginValue, ct);

        if (user is null)
        {
            return LoginResult.Failed(InvalidLoginMessage, LoginFailureReason.AccountNotFound);
        }

        var now = _clock.UtcNow;

        if (user.Status == UserStatus.Disabled)
        {
            return LoginResult.Failed(DisabledMessage, LoginFailureReason.Disabled);
        }

        if (user.LockedUntil.HasValue && user.LockedUntil.Value > now)
        {
            return LoginResult.Failed(LockedMessage, LoginFailureReason.Locked);
        }

        if (user.Status == UserStatus.Locked)
        {
            if (user.LockedUntil.HasValue && user.LockedUntil.Value <= now)
            {
                user.Status = UserStatus.Active;
                user.FailedLoginCount = 0;
                user.LockedUntil = null;
                await _users.UpdateAsync(user, ct);
                await _unitOfWork.SaveChangesAsync(ct);
                await _auditLog.WriteAsync("Auth.AccountUnlocked", "User", user.Id, "Lockout period expired.", ct);
            }
            else
            {
                return LoginResult.Failed(LockedMessage, LoginFailureReason.Locked);
            }
        }

        if (user.Status != UserStatus.Active)
        {
            return LoginResult.Failed(LockedMessage, LoginFailureReason.Locked);
        }

        if (!_passwordHasher.VerifyPassword(user.PasswordHash, request.Password))
        {
            user.FailedLoginCount++;
            if (user.FailedLoginCount >= MaxFailedLoginAttempts)
            {
                user.Status = UserStatus.Locked;
                user.LockedUntil = now.AddMinutes(LockoutMinutes);
                await _auditLog.WriteAsync("Auth.AccountLocked", "User", user.Id, "Too many failed login attempts.", ct);
            }
            else if (user.FailedLoginCount >= MaxFailedLoginAttempts - 1)
            {
                await _auditLog.WriteAsync("Auth.LoginFailedManyTimes", "User", user.Id, "Login failed repeatedly.", ct);
            }

            await _users.UpdateAsync(user, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return LoginResult.Failed(InvalidLoginMessage, LoginFailureReason.InvalidPassword);
        }

        user.FailedLoginCount = 0;
        user.LockedUntil = null;
        user.Status = UserStatus.Active;
        user.LastLoginAt = now;
        await _users.UpdateAsync(user, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var currentUser = ToCurrentUser(user);
        _sessionService.SetCurrentUser(currentUser);
        await _auditLog.WriteAsync("Auth.LoginSucceeded", "User", user.Id, null, ct);
        var result = LoginResult.Success(currentUser);
        return result;
    }

    public Task LogoutAsync(CancellationToken ct = default)
    {
        _sessionService.Clear();
        return Task.CompletedTask;
    }

    public async Task ChangePasswordAsync(ChangePasswordRequest request, CancellationToken ct = default)
    {
        RequestValidator.ValidateAndThrow(request);
        if (!_currentUser.UserId.HasValue)
        {
            throw new InvalidOperationException("No active session.");
        }

        var user = await _users.GetByIdAsync(_currentUser.UserId.Value, ct)
            ?? throw new InvalidOperationException("Current user was not found.");

        if (!_passwordHasher.VerifyPassword(user.PasswordHash, request.CurrentPassword))
        {
            throw new InvalidOperationException("Current password is invalid.");
        }

        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        await _users.UpdateAsync(user, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        await _auditLog.WriteAsync("Auth.PasswordChanged", nameof(user), user.Id, null, ct);
    }

    private int MaxFailedLoginAttempts => Math.Max(1, _securityOptions.MaxFailedLoginAttempts);
    private int LockoutMinutes => Math.Max(1, _securityOptions.LockoutMinutes);

    private static CurrentUserDto ToCurrentUser(DormitoryManagement.Domain.Entities.User user) =>
        new()
        {
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            RoleName = user.Role?.Name ?? string.Empty,
            StudentId = user.Student?.Id,
            ManagerId = user.Manager?.Id,
            BuildingId = user.Manager?.BuildingId
        };
}
