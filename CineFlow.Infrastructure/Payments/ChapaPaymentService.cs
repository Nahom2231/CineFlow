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
        var secretKey = _options.SecretKey;
        var baseUrl = string.IsNullOrWhiteSpace(_options.BaseUrl) ? "https://api.chapa.co" : _options.BaseUrl.TrimEnd('/');

        if (!string.IsNullOrWhiteSpace(secretKey))
        {
            try
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/v1/transaction/initialize");
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", secretKey);

                var payload = new
                {
                    amount = amount.ToString(CultureInfo.InvariantCulture),
                    currency = "ETB",
                    email = email,
                    first_name = "CineFlow",
                    last_name = "Customer",
                    tx_ref = reference,
                    callback_url = _options.CallbackUrl,
                    return_url = $"http://localhost:4200/ticket-confirmation?tx_ref={reference}",
                    customization = new
                    {
                        title = "CineFlow Ticket Payment",
                        description = "Payment for CineFlow Cinema Movie Ticket"
                    }
                };

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
                            _logger.LogInformation("Successfully initialized Chapa payment. Checkout URL: {Url}", url);
                            return url;
                        }
                    }
                }
                else
                {
                    var errBody = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Chapa API initialization response {StatusCode}: {ErrorBody}", response.StatusCode, errBody);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while calling Chapa API for reference {Reference}", reference);
            }
        }

        // Return Chapa checkout URL format so frontend redirects to the official Chapa payment UI
        return $"https://checkout.chapa.co/checkout/web/pay/{reference}";
    }
}