using System.Net;
using DormitoryManagement.Application.Abstractions.Auth;
using DormitoryManagement.Application.Abstractions.Data;
using DormitoryManagement.Application.Abstractions.Repositories;
using DormitoryManagement.Application.Abstractions.Services;
using DormitoryManagement.Application.Common;
using DormitoryManagement.Application.DTOs.Auth;
using DormitoryManagement.Application.Validation;
using DormitoryManagement.Domain.Constants;
using DormitoryManagement.Domain.Entities;
using DormitoryManagement.Domain.Enums;

namespace DormitoryManagement.Application.Services.Auth;

public sealed class AccountRegistrationService : IAccountRegistrationService
{
    private const int OtpExpiryMinutes = 5;
    private const int ResendCooldownSeconds = 60;
    private const int MaxOtpAttempts = 5;
    private const string RegisteredDescription = "Student registered new account.";

    private readonly IUserRepository _users;
    private readonly IStudentRepository _students;
    private readonly IPendingAccountRegistrationRepository _pendingRegistrations;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuditLogService _auditLog;
    private readonly IEmailSender _emailSender;
    private readonly IDateTimeProvider _clock;
    private readonly IOtpGenerator _otpGenerator;

    public AccountRegistrationService(
        IUserRepository users,
        IStudentRepository students,
        IPendingAccountRegistrationRepository pendingRegistrations,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IAuditLogService auditLog,
        IEmailSender emailSender,
        IDateTimeProvider clock,
        IOtpGenerator otpGenerator)
    {
        _users = users;
        _students = students;
        _pendingRegistrations = pendingRegistrations;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _auditLog = auditLog;
        _emailSender = emailSender;
        _clock = clock;
        _otpGenerator = otpGenerator;
    }

    public async Task<StartAccountRegistrationResult> StartStudentAccountRegistrationAsync(RegisterAccountRequest request, CancellationToken ct = default)
    {
        var validationError = ValidateRequest(request);
        if (validationError is not null)
        {
            return StartAccountRegistrationResult.Failed(validationError);
        }

        var normalized = Normalize(request);
        if (normalized.ErrorMessage is not null)
        {
            return StartAccountRegistrationResult.Failed(normalized.ErrorMessage);
        }

        var now = _clock.UtcNow;
        if (CleanupExpiredByIdentity(normalized.Email, normalized.Username, normalized.StudentCode, now))
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }
        var duplicate = await FindExistingDuplicateAsync(normalized.Email, normalized.Username, normalized.StudentCode, ct);
        if (duplicate is not null)
        {
            return StartAccountRegistrationResult.Failed(duplicate);
        }

        var conflicts = FindPendingByIdentity(normalized.Email, normalized.Username, normalized.StudentCode);
        var pendingDuplicate = FindPendingDuplicateMessage(conflicts, normalized);
        if (pendingDuplicate is not null)
        {
            return StartAccountRegistrationResult.Failed(pendingDuplicate);
        }

        if (conflicts.Count > 1)
        {
            return StartAccountRegistrationResult.Failed("A pending registration already exists for this email. Please use the latest verification code or wait for it to expire.");
        }


        var otpCode = _otpGenerator.GenerateCode();
        var expiresAt = now.AddMinutes(OtpExpiryMinutes);
        var resendAvailableAt = now.AddSeconds(ResendCooldownSeconds);
        var pendingRegistration = conflicts.SingleOrDefault();

        if (pendingRegistration is null)
        {
            pendingRegistration = CreatePendingRegistration(normalized, otpCode, now, expiresAt);
            await _pendingRegistrations.AddAsync(pendingRegistration, ct);
        }
        else
        {
            ApplyPendingRegistrationUpdate(pendingRegistration, normalized, otpCode, now, expiresAt);
            _pendingRegistrations.Update(pendingRegistration);
        }

        await _unitOfWork.SaveChangesAsync(ct);
        var emailError = await TrySendOtpEmailAsync(pendingRegistration.Email, pendingRegistration.FullName, otpCode, ct);
        if (emailError is not null)
        {
            return StartAccountRegistrationResult.Failed("Verification email could not be sent. Please check email settings and try again.", emailError);
        }

        return StartAccountRegistrationResult.Success(pendingRegistration.Id, MaskEmail(pendingRegistration.Email), expiresAt, resendAvailableAt);
    }

    public async Task<RegisterAccountResult> VerifyStudentAccountOtpAsync(Guid pendingRegistrationId, string otpCode, CancellationToken ct = default)
    {
        if (pendingRegistrationId == Guid.Empty)
        {
            return RegisterAccountResult.Failed("Verification session was not found.");
        }

        if (!IsValidOtpFormat(otpCode))
        {
            return RegisterAccountResult.Failed("Verification code must be 6 digits.");
        }

        await using var tx = await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var pending = await _pendingRegistrations.GetByIdForUpdateAsync(pendingRegistrationId, ct);
            if (pending is null)
            {
                await tx.RollbackAsync(ct);
                return RegisterAccountResult.Failed("Verification session was not found or has expired.");
            }

            var now = _clock.UtcNow;
            if (pending.ExpiresAt <= now)
            {
                _pendingRegistrations.Remove(pending);
                await _unitOfWork.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
                return RegisterAccountResult.Failed("Verification code has expired. Please request a new code.");
            }

            if (pending.AttemptCount >= MaxOtpAttempts)
            {
                await tx.RollbackAsync(ct);
                return RegisterAccountResult.Failed("Too many incorrect verification attempts. Please request a new code.");
            }

            if (!_passwordHasher.VerifyPassword(pending.OtpHash, otpCode.Trim()))
            {
                pending.AttemptCount++;
                _pendingRegistrations.Update(pending);
                await _unitOfWork.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
                return pending.AttemptCount >= MaxOtpAttempts
                    ? RegisterAccountResult.Failed("Too many incorrect verification attempts. Please request a new code.")
                    : RegisterAccountResult.Failed("Verification code is incorrect.");
            }

            var duplicate = await FindExistingDuplicateAsync(pending.Email, pending.Username, pending.StudentCode, ct);
            if (duplicate is not null)
            {
                _pendingRegistrations.Remove(pending);
                await _unitOfWork.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
                return RegisterAccountResult.Failed(duplicate);
            }

            var studentRole = _unitOfWork.Repository<Role>().Query()
                .FirstOrDefault(role => role.Name == RoleNames.Student);
            if (studentRole is null)
            {
                await tx.RollbackAsync(ct);
                return RegisterAccountResult.Failed("Student role is not configured.");
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = pending.FullName,
                Email = pending.Email,
                Username = pending.Username,
                PasswordHash = pending.PasswordHash,
                RoleId = studentRole.Id,
                Role = studentRole,
                Status = UserStatus.Active,
                FailedLoginCount = 0,
                LockedUntil = null,
                CreatedAt = now,
                IsDeleted = false
            };
            var student = new Student
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                User = user,
                StudentCode = pending.StudentCode,
                FullName = pending.FullName,
                Email = pending.Email,
                PhoneNumber = pending.PhoneNumber,
                Gender = pending.Gender,
                DateOfBirth = pending.DateOfBirth?.Date,
                Status = StudentStatus.NotRegistered,
                CreatedAt = now,
                IsDeleted = false
            };
            user.Student = student;

            await _users.AddAsync(user, ct);
            await _students.AddAsync(student, ct);
            _pendingRegistrations.Remove(pending);
            await _unitOfWork.SaveChangesAsync(ct);
            await _auditLog.WriteAsync("RegisterAccount", "User", user.Id, RegisteredDescription, ct);
            await _auditLog.WriteAsync("RegisterAccount", "Student", student.Id, RegisteredDescription, ct);
            await tx.CommitAsync(ct);
            return RegisterAccountResult.Success(user.Id, student.Id);
        }
        catch (Exception ex) when (LooksLikeUniqueConstraintViolation(ex))
        {
            await tx.RollbackAsync(ct);
            return RegisterAccountResult.Failed("Account information is already registered.");
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<StartAccountRegistrationResult> ResendStudentAccountOtpAsync(Guid pendingRegistrationId, CancellationToken ct = default)
    {
        if (pendingRegistrationId == Guid.Empty)
        {
            return StartAccountRegistrationResult.Failed("Verification session was not found.");
        }

        await using var tx = await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var pending = await _pendingRegistrations.GetByIdForUpdateAsync(pendingRegistrationId, ct);
            if (pending is null)
            {
                await tx.RollbackAsync(ct);
                return StartAccountRegistrationResult.Failed("Verification session was not found or has expired.");
            }

            var now = _clock.UtcNow;
            if (pending.ExpiresAt <= now)
            {
                _pendingRegistrations.Remove(pending);
                await _unitOfWork.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
                return StartAccountRegistrationResult.Failed("Verification code has expired. Please submit registration again.");
            }

            var resendAvailableAt = pending.LastSentAt.AddSeconds(ResendCooldownSeconds);
            if (resendAvailableAt > now)
            {
                var seconds = Math.Max(1, (int)Math.Ceiling((resendAvailableAt - now).TotalSeconds));
                await tx.RollbackAsync(ct);
                return StartAccountRegistrationResult.Failed($"Please wait {seconds} seconds before requesting a new code.");
            }

            var otpCode = _otpGenerator.GenerateCode();
            var expiresAt = now.AddMinutes(OtpExpiryMinutes);
            pending.OtpHash = _passwordHasher.HashPassword(otpCode);
            pending.ExpiresAt = expiresAt;
            pending.LastSentAt = now;
            pending.AttemptCount = 0;
            _pendingRegistrations.Update(pending);
            await _unitOfWork.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            var emailError = await TrySendOtpEmailAsync(pending.Email, pending.FullName, otpCode, ct);
            if (emailError is not null)
            {
                return StartAccountRegistrationResult.Failed("Verification email could not be sent. Please check email settings and try again.", emailError);
            }

            return StartAccountRegistrationResult.Success(pending.Id, MaskEmail(pending.Email), expiresAt, now.AddSeconds(ResendCooldownSeconds));
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    private static string? ValidateRequest(RegisterAccountRequest request)
    {
        try
        {
            RequestValidator.ValidateAndThrow(request);
            return null;
        }
        catch (ValidationException ex)
        {
            return string.Join(Environment.NewLine, ex.Errors);
        }
    }

    private NormalizedRegistration Normalize(RegisterAccountRequest request)
    {
        var normalized = new NormalizedRegistration
        {
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            Username = request.Username.Trim().ToLowerInvariant(),
            StudentCode = request.StudentCode.Trim().ToUpperInvariant(),
            PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim(),
            Gender = string.IsNullOrWhiteSpace(request.Gender) ? null : request.Gender.Trim(),
            DateOfBirth = request.DateOfBirth?.Date,
            Password = request.Password
        };

        if (normalized.DateOfBirth.HasValue && normalized.DateOfBirth.Value > _clock.UtcNow.Date)
        {
            normalized.ErrorMessage = "Date of birth cannot be in the future.";
        }

        return normalized;
    }

    private PendingAccountRegistration CreatePendingRegistration(NormalizedRegistration registration, string otpCode, DateTime now, DateTime expiresAt) =>
        new()
        {
            Id = Guid.NewGuid(),
            FullName = registration.FullName,
            Email = registration.Email,
            Username = registration.Username,
            StudentCode = registration.StudentCode,
            PhoneNumber = registration.PhoneNumber,
            Gender = registration.Gender,
            DateOfBirth = registration.DateOfBirth,
            PasswordHash = _passwordHasher.HashPassword(registration.Password),
            OtpHash = _passwordHasher.HashPassword(otpCode),
            ExpiresAt = expiresAt,
            LastSentAt = now,
            AttemptCount = 0,
            CreatedAt = now
        };

    private void ApplyPendingRegistrationUpdate(PendingAccountRegistration pending, NormalizedRegistration registration, string otpCode, DateTime now, DateTime expiresAt)
    {
        pending.FullName = registration.FullName;
        pending.Email = registration.Email;
        pending.Username = registration.Username;
        pending.StudentCode = registration.StudentCode;
        pending.PhoneNumber = registration.PhoneNumber;
        pending.Gender = registration.Gender;
        pending.DateOfBirth = registration.DateOfBirth;
        pending.PasswordHash = _passwordHasher.HashPassword(registration.Password);
        pending.OtpHash = _passwordHasher.HashPassword(otpCode);
        pending.ExpiresAt = expiresAt;
        pending.LastSentAt = now;
        pending.AttemptCount = 0;
        pending.UpdatedAt = now;
    }

    private async Task<string?> FindExistingDuplicateAsync(string email, string username, string studentCode, CancellationToken ct)
    {
        if (await _users.GetByEmailAsync(email, ct) is not null)
        {
            return "Email is already registered.";
        }

        if (await _users.GetByUsernameAsync(username, ct) is not null)
        {
            return "Username is already registered.";
        }

        if (await _students.GetByStudentCodeAsync(studentCode, ct) is not null)
        {
            return "Student code is already registered.";
        }

        return null;
    }

    private bool CleanupExpiredByIdentity(string email, string username, string studentCode, DateTime now)
    {
        var expired = _pendingRegistrations.Query()
            .Where(pending => pending.ExpiresAt <= now
                && (pending.Email == email || pending.Username == username || pending.StudentCode == studentCode))
            .ToList();

        if (expired.Count > 0)
        {
            _pendingRegistrations.RemoveRange(expired);
            return true;
        }

        return false;
    }
    private static string? FindPendingDuplicateMessage(IReadOnlyList<PendingAccountRegistration> conflicts, NormalizedRegistration registration)
    {
        if (conflicts.Any(pending => pending.Username == registration.Username && !IsSamePendingOwner(pending, registration)))
        {
            return "Username is already registered.";
        }

        if (conflicts.Any(pending => pending.StudentCode == registration.StudentCode && !IsSamePendingOwner(pending, registration)))
        {
            return "Student code is already registered.";
        }

        return null;
    }

    private IReadOnlyList<PendingAccountRegistration> FindPendingByIdentity(string email, string username, string studentCode) =>
        _pendingRegistrations.Query()
            .Where(pending => pending.Email == email || pending.Username == username || pending.StudentCode == studentCode)
            .ToList();

    private static bool IsSamePendingOwner(PendingAccountRegistration pending, NormalizedRegistration registration) =>
        pending.Email == registration.Email;

    private Task SendOtpEmailAsync(string email, string fullName, string otpCode, CancellationToken ct)
    {
        var safeName = WebUtility.HtmlEncode(fullName);
        var safeOtp = WebUtility.HtmlEncode(otpCode);
        return _emailSender.SendAsync(new EmailMessage
        {
            To = email,
            Subject = "DormitoryManagement verification code",
            BodyText = $"Hello {fullName},\n\nYour DormitoryManagement verification code is {otpCode}.\nIt expires in 5 minutes.\n\nIf you did not request this account, ignore this email.",
            BodyHtml = $"""
                <div style="font-family:Segoe UI,Arial,sans-serif;color:#111827;line-height:1.5">
                  <h2 style="color:#145a32">DormitoryManagement</h2>
                  <p>Hello {safeName},</p>
                  <p>Your verification code is:</p>
                  <p style="font-size:28px;font-weight:700;letter-spacing:6px">{safeOtp}</p>
                  <p>This code expires in 5 minutes.</p>
                  <p>If you did not request this account, ignore this email.</p>
                </div>
                """
        }, ct);
    }

    private async Task<string?> TrySendOtpEmailAsync(string email, string fullName, string otpCode, CancellationToken ct)
    {
        try
        {
            await SendOtpEmailAsync(email, fullName, otpCode, ct);
            return null;
        }
        catch (Exception ex) when (!ct.IsCancellationRequested)
        {
            return BuildDiagnosticMessage(ex);
        }
    }

    private static string BuildDiagnosticMessage(Exception ex)
    {
        var messages = new List<string>();
        for (var current = ex; current is not null; current = current.InnerException!)
        {
            messages.Add($"{current.GetType().Name}: {current.Message}");
        }

        return string.Join(" | ", messages);
    }

    private static bool IsValidOtpFormat(string otpCode) =>
        otpCode.Trim().Length == 6 && otpCode.Trim().All(char.IsDigit);

    private static string MaskEmail(string email)
    {
        var at = email.IndexOf('@');
        if (at <= 0)
        {
            return "***";
        }

        var local = email[..at];
        var domain = email[at..];
        return local.Length == 1 ? $"*{domain}" : $"{local[0]}***{domain}";
    }

    private static bool LooksLikeUniqueConstraintViolation(Exception ex)
    {
        for (var current = ex; current is not null; current = current.InnerException!)
        {
            var message = current.Message;
            if (message.Contains("duplicate", StringComparison.OrdinalIgnoreCase)
                || message.Contains("unique", StringComparison.OrdinalIgnoreCase)
                || message.Contains("IX_users_email", StringComparison.OrdinalIgnoreCase)
                || message.Contains("IX_users_username", StringComparison.OrdinalIgnoreCase)
                || message.Contains("IX_students_student_code", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private sealed class NormalizedRegistration
    {
        public string FullName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string Username { get; init; } = string.Empty;
        public string StudentCode { get; init; } = string.Empty;
        public string? PhoneNumber { get; init; }
        public string? Gender { get; init; }
        public DateTime? DateOfBirth { get; init; }
        public string Password { get; init; } = string.Empty;
        public string? ErrorMessage { get; set; }
    }
}
