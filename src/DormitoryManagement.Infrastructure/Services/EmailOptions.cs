using Microsoft.Extensions.Configuration;

namespace DormitoryManagement.Infrastructure.Services;

public sealed class EmailOptions
{
    public const string SectionName = "Email";

    public string SmtpHost { get; init; } = "smtp.gmail.com";
    public int SmtpPort { get; init; } = 587;
    public string SmtpUsername { get; init; } = string.Empty;
    public string SmtpPassword { get; init; } = string.Empty;
    public string FromAddress { get; init; } = string.Empty;
    public string FromName { get; init; } = "DormitoryManagement";

    public static EmailOptions FromConfiguration(IConfiguration configuration)
    {
        var section = configuration.GetSection(SectionName);
        var port = int.TryParse(GetSetting(section, nameof(SmtpPort), "587"), out var parsedPort) ? parsedPort : 587;
        return new EmailOptions
        {
            SmtpHost = GetSetting(section, nameof(SmtpHost), "smtp.gmail.com"),
            SmtpPort = port,
            SmtpUsername = GetSetting(section, nameof(SmtpUsername), string.Empty),
            SmtpPassword = GetSetting(section, nameof(SmtpPassword), string.Empty),
            FromAddress = GetSetting(section, nameof(FromAddress), GetSetting(section, nameof(SmtpUsername), string.Empty)),
            FromName = GetSetting(section, nameof(FromName), "DormitoryManagement")
        };
    }

    private static string GetSetting(IConfigurationSection section, string key, string fallback)
    {
        var envKey = $"{SectionName}__{key}";
        var value = Environment.GetEnvironmentVariable(envKey, EnvironmentVariableTarget.Process)
            ?? Environment.GetEnvironmentVariable(envKey, EnvironmentVariableTarget.User)
            ?? Environment.GetEnvironmentVariable(envKey, EnvironmentVariableTarget.Machine)
            ?? section[key];

        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }
}
