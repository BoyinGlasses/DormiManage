using DormitoryManagement.Application.Abstractions.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace DormitoryManagement.Infrastructure.Services;

public sealed class MailKitEmailSender : IEmailSender
{
    private readonly EmailOptions _options;

    public MailKitEmailSender(EmailOptions options)
    {
        _options = options;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        ValidateOptions();

        var mimeMessage = new MimeMessage();
        mimeMessage.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
        mimeMessage.To.Add(MailboxAddress.Parse(message.To));
        mimeMessage.Subject = message.Subject;
        mimeMessage.Body = new BodyBuilder
        {
            TextBody = message.BodyText,
            HtmlBody = message.BodyHtml
        }.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_options.SmtpHost, _options.SmtpPort, SecureSocketOptions.StartTls, ct);
        await client.AuthenticateAsync(_options.SmtpUsername, _options.SmtpPassword, ct);
        await client.SendAsync(mimeMessage, ct);
        await client.DisconnectAsync(quit: true, ct);
    }

    private void ValidateOptions()
    {
        if (string.IsNullOrWhiteSpace(_options.SmtpHost))
        {
            throw new InvalidOperationException("Email:SmtpHost is missing.");
        }

        if (_options.SmtpPort <= 0)
        {
            throw new InvalidOperationException("Email:SmtpPort is invalid.");
        }

        if (string.IsNullOrWhiteSpace(_options.SmtpUsername))
        {
            throw new InvalidOperationException("Email:SmtpUsername is missing.");
        }

        if (string.IsNullOrWhiteSpace(_options.SmtpPassword))
        {
            throw new InvalidOperationException("Email:SmtpPassword is missing.");
        }

        if (string.IsNullOrWhiteSpace(_options.FromAddress))
        {
            throw new InvalidOperationException("Email:FromAddress is missing.");
        }
    }
}
