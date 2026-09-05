using System.Security.Claims;
using Application.Interfaces;
using Infrastructure.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CineFlow.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly IEncryptionService _encryptionService;
    private readonly ChapaOptions _chapaOptions;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(
        IPaymentService paymentService,
        IEncryptionService encryptionService,
        IOptions<ChapaOptions> chapaOptions,
        ILogger<PaymentController> logger)
    {
        _paymentService = paymentService;
        _encryptionService = encryptionService;
        _chapaOptions = chapaOptions.Value;
        _logger = logger;
    }

    #region DTOs
    public record InitializePaymentRequest(
        decimal Amount,
        string Email,
        string? FirstName,
        string? LastName,
        string? PhoneNumber,
        string? Currency,
        string? Reference,
        Guid? ScheduleId,
        string? SeatNumber,
        string? ReturnUrl = null,
        string? CallbackUrl = null);

    public record EncryptDataRequest(string PlainText);

    public record DecryptDataRequest(string CipherText);

    public record ChapaCallbackPayload(
        string? Event,
        string? Reference,
        string? Status,
        string? TxRef,
        decimal? Amount,
        string? Currency,
        string? PaymentMethod);
    #endregion

    /// <summary>
    /// Initializes a payment transaction via the Chapa payment gateway.
    /// </summary>
    [HttpPost("initialize")]
    [AllowAnonymous]
    public async Task<IActionResult> InitializePayment([FromBody] InitializePaymentRequest request)
    {
        if (request == null)
        {
            return BadRequest(new { Message = "Payment request payload cannot be empty." });
        }

        if (request.Amount <= 0)
        {
            return BadRequest(new { Message = "Payment amount must be greater than zero." });
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest(new { Message = "Customer email is required for payment initialization." });
        }

        // Generate a unique transaction reference if one is not provided
        var reference = string.IsNullOrWhiteSpace(request.Reference)
            ? $"CF-TXN-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(100000, 999999)}"
            : request.Reference.Trim();

        try
        {
            _logger.LogInformation("Initializing payment of {Amount} for {Email} with reference {Reference}",
                request.Amount, request.Email, reference);

            // Call IPaymentService (ChapaPaymentService)
            var checkoutUrlOrRef = await _paymentService.InitializePaymentAsync(new PaymentInitializeRequest(
                Amount: request.Amount,
                Email: request.Email,
                Reference: reference,
                FirstName: request.FirstName,
                LastName: request.LastName,
                PhoneNumber: request.PhoneNumber,
                Currency: request.Currency,
                ReturnUrl: request.ReturnUrl,
                CallbackUrl: request.CallbackUrl));

            // Encrypt reference using IEncryptionService for secure client verification roundtrips
            var encryptedReference = _encryptionService.Encrypt(reference);

            return Ok(new
            {
                Success = true,
                Message = "Payment initialized successfully.",
                Reference = reference,
                EncryptedReference = encryptedReference,
                CheckoutUrl = checkoutUrlOrRef,
                PublicKey = _chapaOptions.PublicKey,
                CallbackUrl = _chapaOptions.CallbackUrl
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize payment for reference {Reference}", reference);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                Success = false,
                Message = "An error occurred while initializing payment.",
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// Verifies a payment transaction by reference or encrypted reference token.
    /// </summary>
    [HttpGet("verify/{reference}")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyPayment(string reference)
    {
        if (string.IsNullOrWhiteSpace(reference))
        {
            return BadRequest(new { Message = "Payment reference is required." });
        }

        try
        {
            // If reference is encrypted, decrypt it safely using IEncryptionService
            var decryptedReference = reference;
            try
            {
                decryptedReference = _encryptionService.Decrypt(reference);
            }
            catch
            {
                // Fallback to raw reference if not encrypted
                decryptedReference = reference;
            }

            _logger.LogInformation("Verifying payment with reference {DecryptedReference}", decryptedReference);

            var verification = await _paymentService.VerifyPaymentAsync(decryptedReference);

            return Ok(new
            {
                Success = verification.Success,
                Reference = verification.Reference,
                Status = verification.Status,
                Message = verification.Message,
                Amount = verification.Amount,
                Currency = verification.Currency,
                PaymentMethod = verification.PaymentMethod
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during payment verification for reference {Reference}", reference);
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                Success = false,
                Message = "Failed to verify payment.",
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// Webhook/Callback endpoint for Chapa payment notifications.
    /// </summary>
    [HttpPost("callback")]
    [AllowAnonymous]
    public IActionResult ProcessCallback([FromBody] ChapaCallbackPayload payload)
    {
        var txnRef = payload?.Reference ?? payload?.TxRef ?? "unknown";
        var status = payload?.Status ?? "unknown";

        if (Request.Headers.TryGetValue("x-chapa-signature", out var signature))
        {
            _logger.LogInformation("Webhook signature received: {Signature}", signature.ToString());
        }

        _logger.LogInformation("Received Chapa payment callback webhook for reference {Reference}, status {Status}, amount {Amount}",
            txnRef, status, payload?.Amount);

        return Ok(new
        {
            Success = true,
            Reference = txnRef,
            Status = status,
            Message = "Payment callback received and processed successfully."
        });
    }

    /// <summary>
    /// GET callback endpoint when users are redirected back from Chapa checkout.
    /// Redirects browser users to the frontend ticket-confirmation page.
    /// </summary>
    [HttpGet("callback")]
    [AllowAnonymous]
    public IActionResult HandleRedirectCallback(
        [FromQuery] string? trx_ref,
        [FromQuery] string? tx_ref,
        [FromQuery] string? status)
    {
        var reference = !string.IsNullOrWhiteSpace(trx_ref) ? trx_ref : tx_ref;
        var txnStatus = status ?? "success";

        _logger.LogInformation("Redirect callback received for transaction {Reference} with status {Status}",
            reference, txnStatus);

        var acceptHeader = Request.Headers.Accept.ToString();
        var isBrowserRequest = string.IsNullOrWhiteSpace(acceptHeader) ||
                               acceptHeader.Contains("text/html") ||
                               acceptHeader.Contains("*/*");

        if (isBrowserRequest && !string.IsNullOrWhiteSpace(reference))
        {
            var baseReturnUrl = !string.IsNullOrWhiteSpace(_chapaOptions.ReturnUrl)
                ? _chapaOptions.ReturnUrl
                : "http://localhost:4200/ticket-confirmation";

            var separator = baseReturnUrl.Contains('?') ? "&" : "?";
            var targetUrl = $"{baseReturnUrl}{separator}tx_ref={Uri.EscapeDataString(reference)}&status={Uri.EscapeDataString(txnStatus)}";

            _logger.LogInformation("Redirecting browser from Chapa callback to frontend: {TargetUrl}", targetUrl);
            return Redirect(targetUrl);
        }

        return Ok(new
        {
            Success = true,
            TransactionReference = reference,
            Status = txnStatus,
            Message = "Transaction redirect received."
        });
    }

    /// <summary>
    /// Gets public payment configuration (public key, callback URL, base URL).
    /// </summary>
    [HttpGet("config")]
    [AllowAnonymous]
    public IActionResult GetPaymentConfig()
    {
        return Ok(new
        {
            PublicKey = _chapaOptions.PublicKey,
            BaseUrl = _chapaOptions.BaseUrl,
            CallbackUrl = _chapaOptions.CallbackUrl
        });
    }

    /// <summary>
    /// Securely encrypts sensitive payment payloads or reference tokens using IEncryptionService.
    /// </summary>
    [HttpPost("encrypt")]
    [Authorize]
    public IActionResult EncryptData([FromBody] EncryptDataRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.PlainText))
        {
            return BadRequest(new { Message = "PlainText cannot be empty." });
        }

        var encrypted = _encryptionService.Encrypt(request.PlainText);
        return Ok(new { CipherText = encrypted });
    }

    /// <summary>
    /// Securely decrypts sensitive payment payloads or reference tokens using IEncryptionService.
    /// </summary>
    [HttpPost("decrypt")]
    [Authorize]
    public IActionResult DecryptData([FromBody] DecryptDataRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.CipherText))
        {
            return BadRequest(new { Message = "CipherText cannot be empty." });
        }

        var decrypted = _encryptionService.Decrypt(request.CipherText);
        return Ok(new { PlainText = decrypted });
    }
}
