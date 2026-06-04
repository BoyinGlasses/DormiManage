using System.Globalization;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using DormitoryManagement.Application.Abstractions.Payments;
using DormitoryManagement.Application.DTOs.Payments;
using QRCoder;

namespace DormitoryManagement.Infrastructure.Services;

public sealed class PayOsService : IPayOsService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly PayOsOptions _options;

    public PayOsService(PayOsOptions options)
    {
        _options = options;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(NormalizeBaseUrl(options.BaseUrl), UriKind.Absolute)
        };
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.DefaultRequestHeaders.Add("x-client-id", options.ClientId);
        _httpClient.DefaultRequestHeaders.Add("x-api-key", options.ApiKey);
    }

    public async Task<PayOsPaymentLinkDto> CreatePaymentLinkAsync(PayOsCreatePaymentRequest request, CancellationToken ct = default)
    {
        EnsureApiConfiguration();
        if (request.Amount <= 0m)
        {
            throw new InvalidOperationException("Payment amount must be greater than zero.");
        }

        if (request.OrderCode <= 0)
        {
            throw new InvalidOperationException("PayOS order code must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            throw new InvalidOperationException("PayOS payment description is required.");
        }

        var amount = decimal.ToInt32(decimal.Round(request.Amount, 0, MidpointRounding.AwayFromZero));
        var payload = new CreatePaymentLinkApiRequest
        {
            OrderCode = request.OrderCode,
            Amount = amount,
            Description = request.Description.Trim(),
            ReturnUrl = _options.ReturnUrl.Trim(),
            CancelUrl = _options.CancelUrl.Trim(),
            BuyerName = request.BuyerName.Trim(),
            Items =
            [
                new CreatePaymentLinkItemApiRequest
                {
                    Name = string.IsNullOrWhiteSpace(request.ItemName) ? "Dormitory invoice" : request.ItemName.Trim(),
                    Quantity = 1,
                    Price = amount
                }
            ]
        };
        payload.Signature = ComputeSignature(new Dictionary<string, string>
        {
            ["amount"] = payload.Amount.ToString(CultureInfo.InvariantCulture),
            ["cancelUrl"] = payload.CancelUrl,
            ["description"] = payload.Description,
            ["orderCode"] = payload.OrderCode.ToString(CultureInfo.InvariantCulture),
            ["returnUrl"] = payload.ReturnUrl
        });

        using var response = await _httpClient.PostAsync(
            "/v2/payment-requests",
            new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json"),
            ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        response.EnsureSuccessStatusCode();
        var envelope = JsonSerializer.Deserialize<ApiEnvelope<CreatePaymentLinkApiResponse>>(body, JsonOptions)
            ?? throw new InvalidOperationException("PayOS returned an empty response.");
        var data = EnsureSuccess(envelope, body);
        if (string.IsNullOrWhiteSpace(data.QrCode))
        {
            throw new InvalidOperationException("PayOS did not return QR data.");
        }

        return new PayOsPaymentLinkDto
        {
            OrderCode = data.OrderCode,
            PaymentLinkId = data.PaymentLinkId ?? string.Empty,
            CheckoutUrl = data.CheckoutUrl ?? string.Empty,
            QrCode = data.QrCode,
            QrDataUrl = RenderQrDataUrl(data.QrCode),
            Status = data.Status ?? string.Empty
        };
    }

    public async Task<PayOsPaymentStatusDto> GetPaymentLinkAsync(long orderCode, CancellationToken ct = default)
    {
        EnsureApiConfiguration();
        using var response = await _httpClient.GetAsync($"/v2/payment-requests/{orderCode}", ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        response.EnsureSuccessStatusCode();
        var envelope = JsonSerializer.Deserialize<ApiEnvelope<PaymentLinkStatusApiResponse>>(body, JsonOptions)
            ?? throw new InvalidOperationException("PayOS returned an empty response.");
        var data = EnsureSuccess(envelope, body);

        return new PayOsPaymentStatusDto
        {
            OrderCode = data.OrderCode,
            PaymentLinkId = data.Id ?? string.Empty,
            Status = data.Status ?? string.Empty,
            Amount = data.Amount,
            AmountPaid = data.AmountPaid,
            Description = data.Description ?? string.Empty,
            CheckoutUrl = data.CheckoutUrl ?? string.Empty,
            QrCode = data.QrCode ?? string.Empty
        };
    }

    public async Task ConfirmWebhookAsync(string webhookUrl, CancellationToken ct = default)
    {
        EnsureApiConfiguration();
        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            throw new InvalidOperationException("PayOS webhook URL is required.");
        }

        var payload = JsonSerializer.Serialize(new { webhookUrl = webhookUrl.Trim() }, JsonOptions);
        using var response = await _httpClient.PostAsync(
            "/confirm-webhook",
            new StringContent(payload, Encoding.UTF8, "application/json"),
            ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        response.EnsureSuccessStatusCode();
        var envelope = JsonSerializer.Deserialize<ApiEnvelope<JsonElement>>(body, JsonOptions)
            ?? throw new InvalidOperationException("PayOS returned an empty webhook confirmation response.");
        _ = EnsureSuccess(envelope, body);
    }

    public PayOsWebhookEventDto ParseWebhook(string payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            throw new InvalidOperationException("PayOS webhook payload is required.");
        }

        var wrapper = JsonSerializer.Deserialize<WebhookWrapper>(payload, JsonOptions)
            ?? throw new InvalidOperationException("PayOS webhook payload is invalid.");
        if (wrapper.Data.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException("PayOS webhook payload does not contain data.");
        }

        var signature = wrapper.Signature ?? string.Empty;
        var computed = ComputeSignature(wrapper.Data);
        if (!CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(signature), Encoding.UTF8.GetBytes(computed)))
        {
            throw new InvalidOperationException("PayOS webhook signature is invalid.");
        }

        var data = wrapper.Data.Deserialize<WebhookData>(JsonOptions)
            ?? throw new InvalidOperationException("PayOS webhook data is invalid.");
        return new PayOsWebhookEventDto
        {
            OrderCode = data.OrderCode,
            Amount = data.Amount,
            Description = data.Description ?? string.Empty,
            Reference = data.Reference ?? string.Empty,
            TransactionDateTime = ParseWebhookTransactionDateTime(data.TransactionDateTime),
            Status = data.Code ?? string.Empty
        };
    }

    private void EnsureApiConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_options.ClientId)
            || string.IsNullOrWhiteSpace(_options.ApiKey)
            || string.IsNullOrWhiteSpace(_options.ChecksumKey))
        {
            throw new InvalidOperationException("PayOS configuration is missing ClientId, ApiKey, or ChecksumKey.");
        }

        if (string.IsNullOrWhiteSpace(_options.ReturnUrl) || string.IsNullOrWhiteSpace(_options.CancelUrl))
        {
            throw new InvalidOperationException("PayOS ReturnUrl and CancelUrl must be configured.");
        }
    }

    private string ComputeSignature(IReadOnlyDictionary<string, string> values)
    {
        var message = string.Join("&", values.OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .Select(pair => pair.Key + "=" + pair.Value));
        return ComputeHmac(message);
    }

    private string ComputeSignature(JsonElement data)
    {
        var values = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var property in data.EnumerateObject())
        {
            values[property.Name] = SerializeSignatureValue(property.Value);
        }

        return ComputeSignature(values);
    }

    private string ComputeHmac(string message)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.ChecksumKey));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string SerializeSignatureValue(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.String => value.GetString() ?? string.Empty,
        JsonValueKind.Number => value.GetRawText(),
        JsonValueKind.True => "true",
        JsonValueKind.False => "false",
        JsonValueKind.Null => string.Empty,
        _ => value.GetRawText()
    };

    private static string NormalizeBaseUrl(string? baseUrl)
    {
        var value = string.IsNullOrWhiteSpace(baseUrl) ? "https://api-merchant.payos.vn" : baseUrl.Trim();
        return value.EndsWith('/') ? value[..^1] : value;
    }

    private static DateTime ParseWebhookTransactionDateTime(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return default;
        }

        var trimmed = value.Trim();
        string[] supportedFormats =
        [
            "yyyy-MM-dd HH:mm:ss",
            "yyyy-MM-ddTHH:mm:ss",
            "yyyy-MM-ddTHH:mm:ss.FFFFFFFK"
        ];
        if (DateTime.TryParseExact(
            trimmed,
            supportedFormats,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AllowWhiteSpaces,
            out var exactDateTime))
        {
            return exactDateTime;
        }

        if (DateTime.TryParse(
            trimmed,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AllowWhiteSpaces,
            out var parsedDateTime))
        {
            return parsedDateTime;
        }

        throw new InvalidOperationException("PayOS webhook transaction date is invalid.");
    }

    private static T EnsureSuccess<T>(ApiEnvelope<T> envelope, string responseBody)
    {
        if (!string.Equals(envelope.Code, "00", StringComparison.OrdinalIgnoreCase) || envelope.Data is null)
        {
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(envelope.Desc)
                ? $"PayOS request failed: {responseBody}"
                : $"PayOS request failed: {envelope.Desc}");
        }

        return envelope.Data;
    }

    private static string RenderQrDataUrl(string qrCode)
    {
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(qrCode, QRCodeGenerator.ECCLevel.Q);
        var png = new PngByteQRCode(data);
        var bytes = png.GetGraphic(20);
        return "data:image/png;base64," + Convert.ToBase64String(bytes);
    }

    private sealed class ApiEnvelope<T>
    {
        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("desc")]
        public string? Desc { get; set; }

        [JsonPropertyName("data")]
        public T? Data { get; set; }
    }

    private sealed class CreatePaymentLinkApiRequest
    {
        [JsonPropertyName("orderCode")]
        public long OrderCode { get; set; }

        [JsonPropertyName("amount")]
        public int Amount { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("cancelUrl")]
        public string CancelUrl { get; set; } = string.Empty;

        [JsonPropertyName("returnUrl")]
        public string ReturnUrl { get; set; } = string.Empty;

        [JsonPropertyName("buyerName")]
        public string BuyerName { get; set; } = string.Empty;

        [JsonPropertyName("items")]
        public List<CreatePaymentLinkItemApiRequest> Items { get; set; } = new();

        [JsonPropertyName("signature")]
        public string Signature { get; set; } = string.Empty;
    }

    private sealed class CreatePaymentLinkItemApiRequest
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("price")]
        public int Price { get; set; }
    }

    private sealed class CreatePaymentLinkApiResponse
    {
        [JsonPropertyName("bin")]
        public string? Bin { get; set; }

        [JsonPropertyName("accountNumber")]
        public string? AccountNumber { get; set; }

        [JsonPropertyName("accountName")]
        public string? AccountName { get; set; }

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("orderCode")]
        public long OrderCode { get; set; }

        [JsonPropertyName("currency")]
        public string? Currency { get; set; }

        [JsonPropertyName("paymentLinkId")]
        public string? PaymentLinkId { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("checkoutUrl")]
        public string? CheckoutUrl { get; set; }

        [JsonPropertyName("qrCode")]
        public string? QrCode { get; set; }
    }

    private sealed class PaymentLinkStatusApiResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("orderCode")]
        public long OrderCode { get; set; }

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("amountPaid")]
        public decimal AmountPaid { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("checkoutUrl")]
        public string? CheckoutUrl { get; set; }

        [JsonPropertyName("qrCode")]
        public string? QrCode { get; set; }
    }

    private sealed class WebhookWrapper
    {
        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("desc")]
        public string? Desc { get; set; }

        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public JsonElement Data { get; set; }

        [JsonPropertyName("signature")]
        public string? Signature { get; set; }
    }

    private sealed class WebhookData
    {
        [JsonPropertyName("orderCode")]
        public long OrderCode { get; set; }

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("reference")]
        public string? Reference { get; set; }

        [JsonPropertyName("transactionDateTime")]
        public string? TransactionDateTime { get; set; }

        [JsonPropertyName("code")]
        public string? Code { get; set; }
    }
}
