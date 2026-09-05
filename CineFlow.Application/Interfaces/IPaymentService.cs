namespace Application.Interfaces;

public record PaymentInitializeRequest(
    decimal Amount,
    string Email,
    string Reference,
    string? FirstName = null,
    string? LastName = null,
    string? PhoneNumber = null,
    string? Currency = null,
    string? ReturnUrl = null,
    string? CallbackUrl = null);

public record PaymentVerificationResult(
    bool Success,
    string Reference,
    string Status,
    string Message,
    decimal? Amount = null,
    string? Currency = null,
    string? PaymentMethod = null);

public interface IPaymentService
{
    Task<string> InitializePaymentAsync(
        decimal amount,
        string email,
        string reference);

    Task<string> InitializePaymentAsync(PaymentInitializeRequest request);

    Task<PaymentVerificationResult> VerifyPaymentAsync(string reference);
}