using System.IO;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text;
using System.Text.Json;
using DormitoryManagement.Application.Abstractions.Payments;
using DormitoryManagement.Application.DTOs.Payments;
using DormitoryManagement.Application.Services.Payments;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DormitoryManagement.WPF.Services;

public sealed class PayOsWebhookHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly PayOsOptions _options;
    private readonly ILogger<PayOsWebhookHostedService> _logger;
    private HttpListener? _listener;

    public PayOsWebhookHostedService(
        IServiceScopeFactory scopeFactory,
        PayOsOptions options,
        ILogger<PayOsWebhookHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options;
        _logger = logger;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        if (_options.AutoConfirmWebhook && !string.IsNullOrWhiteSpace(_options.WebhookUrl))
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var payOsService = scope.ServiceProvider.GetRequiredService<IPayOsService>();
                await payOsService.ConfirmWebhookAsync(_options.WebhookUrl, cancellationToken);
                _logger.LogInformation("PayOS webhook confirmed for {WebhookUrl}", _options.WebhookUrl);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to confirm PayOS webhook URL {WebhookUrl}", _options.WebhookUrl);
            }
        }

        if (string.IsNullOrWhiteSpace(_options.WebhookListenPrefix) || string.IsNullOrWhiteSpace(_options.ChecksumKey))
        {
            _logger.LogInformation("PayOS webhook listener skipped because WebhookListenPrefix or ChecksumKey is not configured.");
            return;
        }

        try
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add(_options.WebhookListenPrefix);
            _listener.Start();
            _logger.LogInformation("PayOS webhook listener started on {Prefix}", _options.WebhookListenPrefix);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to start PayOS webhook listener on {Prefix}", _options.WebhookListenPrefix);
        }

        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_listener is null || !_listener.IsListening)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                continue;
            }

            HttpListenerContext context;
            try
            {
                context = await _listener.GetContextAsync();
            }
            catch (Exception) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "PayOS webhook listener accept failed.");
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                continue;
            }

            _ = Task.Run(() => HandleRequestAsync(context, stoppingToken), stoppingToken);
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        if (_listener is not null)
        {
            try
            {
                _listener.Stop();
                _listener.Close();
            }
            catch
            {
                // ignore shutdown errors
            }
        }

        return base.StopAsync(cancellationToken);
    }

    private async Task HandleRequestAsync(HttpListenerContext context, CancellationToken ct)
    {
        try
        {
            if (!HttpMethodsMatch(context.Request.HttpMethod, "POST"))
            {
                await WriteJsonAsync(context.Response, 405, new { error = "Method not allowed" }, ct);
                return;
            }

            using var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding ?? Encoding.UTF8);
            var payload = await reader.ReadToEndAsync(ct);
            using var scope = _scopeFactory.CreateScope();
            var payOsService = scope.ServiceProvider.GetRequiredService<IPayOsService>();
            var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();
            var webhook = payOsService.ParseWebhook(payload);
            if (string.IsNullOrWhiteSpace(webhook.Description) || webhook.Amount <= 0m || string.IsNullOrWhiteSpace(webhook.Reference))
            {
                await WriteJsonAsync(context.Response, 200, new { ok = true, ignored = true }, ct);
                return;
            }

            var result = await paymentService.ProcessBankTransferAsync(new BankTransferNotificationDto
            {
                TransactionId = webhook.Reference,
                Amount = webhook.Amount,
                Description = webhook.Description,
                TransactionDate = webhook.TransactionDateTime == default ? DateTime.UtcNow : webhook.TransactionDateTime
            }, ct);
            await WriteJsonAsync(context.Response, 200, new { ok = true, result.Status, result.Matched, result.Duplicate }, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "PayOS webhook handling failed.");
            await WriteJsonAsync(context.Response, 400, new { error = ex.Message }, ct);
        }
    }

    private static bool HttpMethodsMatch(string? actual, string expected) =>
        string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase);

    private static async Task WriteJsonAsync(HttpListenerResponse response, int statusCode, object body, CancellationToken ct)
    {
        response.StatusCode = statusCode;
        response.ContentType = "application/json";
        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(body));
        response.ContentLength64 = bytes.Length;
        await response.OutputStream.WriteAsync(bytes, ct);
        response.OutputStream.Close();
    }
}


