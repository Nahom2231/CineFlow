using System;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Application.Interfaces;
using Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Payments;

public class ChapaPaymentService : IPaymentService
{
    private readonly ChapaOptions _options;
    private readonly HttpClient _httpClient;
    private readonly ILogger<ChapaPaymentService> _logger;

    public ChapaPaymentService(
        IOptions<ChapaOptions> options,
        HttpClient httpClient,
        ILogger<ChapaPaymentService> logger)
    {
        _options = options.Value;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string> InitializePaymentAsync(
        decimal amount,
        string email,
        string reference)
    {
        return await InitializePaymentAsync(new PaymentInitializeRequest(amount, email, reference));
    }

    public async Task<string> InitializePaymentAsync(PaymentInitializeRequest request)
    {
        var secretKey = _options.SecretKey;
        var baseUrl = string.IsNullOrWhiteSpace(_options.BaseUrl) ? "https://api.chapa.co" : _options.BaseUrl.TrimEnd('/');
        var reference = request.Reference;

        var baseReturnUrl = !string.IsNullOrWhiteSpace(request.ReturnUrl)
            ? request.ReturnUrl
            : (!string.IsNullOrWhiteSpace(_options.ReturnUrl) ? _options.ReturnUrl : "http://localhost:4200/ticket-confirmation");

        var separator = baseReturnUrl.Contains('?') ? "&" : "?";
        var returnUrl = $"{baseReturnUrl}{separator}tx_ref={Uri.EscapeDataString(reference)}";

        var callbackUrl = !string.IsNullOrWhiteSpace(request.CallbackUrl)
            ? request.CallbackUrl
            : _options.CallbackUrl;

        if (!string.IsNullOrWhiteSpace(secretKey))
        {
            try
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/v1/transaction/initialize");
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", secretKey);

                var payload = new Dictionary<string, object>
                {
                    ["amount"] = request.Amount.ToString(CultureInfo.InvariantCulture),
                    ["currency"] = string.IsNullOrWhiteSpace(request.Currency) ? "ETB" : request.Currency.Trim().ToUpper(),
                    ["email"] = request.Email.Trim(),
                    ["first_name"] = string.IsNullOrWhiteSpace(request.FirstName) ? "CineFlow" : request.FirstName.Trim(),
                    ["last_name"] = string.IsNullOrWhiteSpace(request.LastName) ? "Customer" : request.LastName.Trim(),
                    ["tx_ref"] = reference,
                    ["return_url"] = returnUrl,
                    ["customization"] = new
                    {
                        title = "CineFlow Ticket",
                        description = "CineFlow Movie Ticket"
                    }
                };

                if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
                {
                    payload["phone_number"] = request.PhoneNumber.Trim();
                }

                if (!string.IsNullOrWhiteSpace(callbackUrl))
                {
                    payload["callback_url"] = callbackUrl;
                }

                requestMessage.Content = JsonContent.Create(payload);
                var response = await _httpClient.SendAsync(requestMessage);

                if (response.IsSuccessStatusCode)
                {
                    var jsonDoc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
                    if (jsonDoc.RootElement.TryGetProperty("data", out var dataEl) &&
                        dataEl.TryGetProperty("checkout_url", out var checkoutUrlEl))
                    {
                        var url = checkoutUrlEl.GetString();
                        if (!string.IsNullOrWhiteSpace(url))
                        {
                            _logger.LogInformation("Successfully initialized Chapa payment for {Reference}. Checkout URL: {Url}", reference, url);
                            return url;
                        }
                    }
                }
                else
                {
                    var errBody = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Chapa API initialization error {StatusCode} for {Reference}: {ErrorBody}",
                         response.StatusCode, reference, errBody);
                    throw new InvalidOperationException($"Chapa payment initialization failed: {errBody}");
                }
            }
            catch (Exception ex) when (ex is not InvalidOperationException)
            {
                _logger.LogError(ex, "Error while calling Chapa API for reference {Reference}", reference);
                throw new InvalidOperationException($"Chapa API connection error: {ex.Message}", ex);
            }
        }

        // Development fallback only when SecretKey is not configured
        _logger.LogInformation("Chapa SecretKey not set. Using simulation return URL for reference {Reference}", reference);
        return returnUrl;
    }

    public async Task<PaymentVerificationResult> VerifyPaymentAsync(string reference)
    {
        var secretKey = _options.SecretKey;
        var baseUrl = string.IsNullOrWhiteSpace(_options.BaseUrl) ? "https://api.chapa.co" : _options.BaseUrl.TrimEnd('/');

        if (!string.IsNullOrWhiteSpace(secretKey))
        {
            try
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{baseUrl}/v1/transaction/verify/{Uri.EscapeDataString(reference)}");
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", secretKey);

                var response = await _httpClient.SendAsync(requestMessage);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    using var jsonDoc = JsonDocument.Parse(content);
                    var root = jsonDoc.RootElement;
                    var message = root.TryGetProperty("message", out var msgEl) ? msgEl.GetString() ?? string.Empty : string.Empty;
                    var status = root.TryGetProperty("status", out var stEl) ? stEl.GetString() ?? string.Empty : string.Empty;

                    decimal? amount = null;
                    string? currency = null;
                    string? paymentMethod = null;

                    if (root.TryGetProperty("data", out var dataEl))
                    {
                        if (dataEl.TryGetProperty("status", out var dataStatusEl))
                        {
                            status = dataStatusEl.GetString() ?? status;
                        }
                        if (dataEl.TryGetProperty("amount", out var amountEl))
                        {
                            if (amountEl.ValueKind == JsonValueKind.Number && amountEl.TryGetDecimal(out var parsedAmount))
                            {
                                amount = parsedAmount;
                            }
                            else if (amountEl.ValueKind == JsonValueKind.String && decimal.TryParse(amountEl.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var strAmount))
                            {
                                amount = strAmount;
                            }
                        }
                        if (dataEl.TryGetProperty("currency", out var currEl))
                        {
                            currency = currEl.GetString();
                        }
                        if (dataEl.TryGetProperty("method", out var methodEl))
                        {
                            paymentMethod = methodEl.GetString();
                        }
                    }

                    var isSuccess = string.Equals(status, "success", StringComparison.OrdinalIgnoreCase);
                    _logger.LogInformation("Chapa verification for {Reference}: Status={Status}, Success={IsSuccess}", reference, status, isSuccess);

                    return new PaymentVerificationResult(
                        Success: isSuccess,
                        Reference: reference,
                        Status: status,
                        Message: message,
                        Amount: amount,
                        Currency: currency,
                        PaymentMethod: paymentMethod);
                }
                else
                {
                    _logger.LogWarning("Chapa API verification failed with status {StatusCode}: {Body}", response.StatusCode, content);
                    return new PaymentVerificationResult(
                        Success: false,
                        Reference: reference,
                        Status: "failed",
                        Message: $"Chapa verification returned {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while verifying Chapa payment for reference {Reference}", reference);
                return new PaymentVerificationResult(
                    Success: false,
                    Reference: reference,
                    Status: "error",
                    Message: ex.Message);
            }
        }

        // Development / simulation fallback when SecretKey is not set
        _logger.LogInformation("Chapa SecretKey not set. Returning verified simulation status for {Reference}", reference);
        return new PaymentVerificationResult(
            Success: true,
            Reference: reference,
            Status: "success",
            Message: "Payment verified successfully (simulation mode)",
            Amount: null,
            Currency: "ETB",
            PaymentMethod: "Chapa Gateway");
    }
}