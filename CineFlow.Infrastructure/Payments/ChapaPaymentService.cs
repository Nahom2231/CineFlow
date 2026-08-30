using Application.Interfaces;
using Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace Infrastructure.Payments;

public class ChapaPaymentService : IPaymentService
{
    private readonly ChapaOptions _options;
    private readonly HttpClient _httpClient;

    public ChapaPaymentService(
        IOptions<ChapaOptions> options,
        HttpClient httpClient)
    {
        _options = options.Value;
        _httpClient = httpClient;
    }

    public async Task<string> InitializePaymentAsync(
        decimal amount,
        string email,
        string reference)
    {
        var secretKey = _options.SecretKey;

        // Call Chapa here.
        // The secret key remains inside the backend.

        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                secretKey);

        // Chapa API request...

        return reference;
    }
}