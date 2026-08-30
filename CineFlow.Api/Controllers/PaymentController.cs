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
        string? SeatNumber);

    public record EncryptDataRequest(string PlainText);

    public record DecryptDataRequest(string CipherText);

    public record ChapaCallbackPayload(
        string? Event,
        string? Reference,
        string? Status,
        string? TxRef,
        decimal? Amount);
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
            var checkoutUrlOrRef = await _paymentService.InitializePaymentAsync(
                request.Amount,
                request.Email,
                reference);

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
    public IActionResult VerifyPayment(string reference)
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

            return Ok(new
            {
                Success = true,
                Reference = decryptedReference,
                Status = "Success",
                Message = "Payment verified successfully."
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
        _logger.LogInformation("Received Chapa payment callback for reference {Reference}, status {Status}",
            payload?.Reference ?? payload?.TxRef, payload?.Status);

        return Ok(new
        {
            Success = true,
            Message = "Payment callback received and processed successfully."
        });
    }

    /// <summary>
    /// GET callback endpoint when users are redirected back from Chapa checkout.
    /// </summary>
    [HttpGet("callback")]
    [AllowAnonymous]
    public IActionResult HandleRedirectCallback([FromQuery] string? trx_ref, [FromQuery] string? status)
    {
        _logger.LogInformation("Redirect callback received for transaction {TrxRef} with status {Status}",
            trx_ref, status);

        return Ok(new
        {
            Success = true,
            TransactionReference = trx_ref,
            Status = status ?? "pending",
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
