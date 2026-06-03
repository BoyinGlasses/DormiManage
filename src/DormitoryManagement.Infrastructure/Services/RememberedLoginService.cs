using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DormitoryManagement.Application.Abstractions.Auth;
using DormitoryManagement.Application.DTOs.Auth;

namespace DormitoryManagement.Infrastructure.Services;

public sealed class RememberedLoginService : IRememberedLoginService
{
    private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("DormitoryManagement.RememberedLogin.v1");
    private readonly string _storagePath;
    private readonly IStringProtector _protector;

    public RememberedLoginService()
        : this(CreateDefaultStoragePath(), new DpapiStringProtector(Entropy))
    {
    }

    internal RememberedLoginService(string storagePath, IStringProtector protector)
    {
        _storagePath = storagePath;
        _protector = protector;
    }

    public RememberedLoginState Load()
    {
        if (!File.Exists(_storagePath))
        {
            return RememberedLoginState.Empty;
        }

        try
        {
            var dto = JsonSerializer.Deserialize<StoredRememberedLogin>(File.ReadAllText(_storagePath));
            if (dto is null || string.IsNullOrWhiteSpace(dto.EmailOrStudentCode))
            {
                return RememberedLoginState.Empty;
            }

            if (string.IsNullOrWhiteSpace(dto.ProtectedPassword))
            {
                return new RememberedLoginState(dto.EmailOrStudentCode, string.Empty, false);
            }

            var password = _protector.Unprotect(dto.ProtectedPassword);
            return string.IsNullOrEmpty(password)
                ? new RememberedLoginState(dto.EmailOrStudentCode, string.Empty, false)
                : new RememberedLoginState(dto.EmailOrStudentCode, password, true);
        }
        catch
        {
            Clear();
            return RememberedLoginState.Empty;
        }
    }

    public void SaveFullLogin(string emailOrStudentCode, string password)
    {
        if (string.IsNullOrWhiteSpace(emailOrStudentCode) || string.IsNullOrEmpty(password))
        {
            Clear();
            return;
        }

        Save(new StoredRememberedLogin(emailOrStudentCode.Trim(), _protector.Protect(password)));
    }

    public void SaveEmailOnly(string emailOrStudentCode)
    {
        if (string.IsNullOrWhiteSpace(emailOrStudentCode))
        {
            Clear();
            return;
        }

        Save(new StoredRememberedLogin(emailOrStudentCode.Trim(), null));
    }

    public void DowngradeToEmailOnly(string emailOrStudentCode)
    {
        if (Load().HasEmailOrStudentCode)
        {
            SaveEmailOnly(emailOrStudentCode);
        }
    }

    public void Clear()
    {
        if (File.Exists(_storagePath))
        {
            File.Delete(_storagePath);
        }
    }

    private void Save(StoredRememberedLogin dto)
    {
        var directory = Path.GetDirectoryName(_storagePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(_storagePath, JsonSerializer.Serialize(dto));
    }

    private static string CreateDefaultStoragePath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appData, "DormitoryManagement", "remembered-login.json");
    }

    private sealed record StoredRememberedLogin(string EmailOrStudentCode, string? ProtectedPassword);

    private sealed class DpapiStringProtector : IStringProtector
    {
        private readonly byte[] _entropy;

        public DpapiStringProtector(byte[] entropy)
        {
            _entropy = entropy;
        }

        public string Protect(string value)
        {
            if (!OperatingSystem.IsWindows())
            {
                throw new PlatformNotSupportedException("Remembered login protection requires Windows DPAPI.");
            }

            var bytes = Encoding.UTF8.GetBytes(value);
            var protectedBytes = ProtectedData.Protect(bytes, _entropy, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(protectedBytes);
        }

        public string Unprotect(string protectedValue)
        {
            if (!OperatingSystem.IsWindows())
            {
                throw new PlatformNotSupportedException("Remembered login protection requires Windows DPAPI.");
            }

            var protectedBytes = Convert.FromBase64String(protectedValue);
            var bytes = ProtectedData.Unprotect(protectedBytes, _entropy, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}

internal interface IStringProtector
{
    string Protect(string value);
    string Unprotect(string protectedValue);
}
