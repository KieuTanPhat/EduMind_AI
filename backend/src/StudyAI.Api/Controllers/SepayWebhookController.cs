using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyAI.Application.Features.Billing.Commands;
using StudyAI.Contracts.Billing;

namespace StudyAI.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/payments/sepay")]
public sealed class SepayWebhookController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IConfiguration _configuration;

    public SepayWebhookController(ISender sender, IConfiguration configuration)
    {
        _sender = sender;
        _configuration = configuration;
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> Receive(CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(Request.Body, Encoding.UTF8);
        var rawBody = await reader.ReadToEndAsync(cancellationToken);
        if (!VerifyRequest(rawBody))
        {
            return Unauthorized(new { success = false, message = "Invalid SePay webhook authentication." });
        }

        SepayWebhookPayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<SepayWebhookPayload>(rawBody, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        }
        catch (JsonException)
        {
            return BadRequest(new { success = false, message = "Invalid JSON payload." });
        }

        if (payload is null)
        {
            return BadRequest(new { success = false, message = "Empty webhook payload." });
        }

        var expectedAccount = _configuration["SEPAY_BANK_ACCOUNT_NUMBER"] ?? _configuration["SePay:BankAccountNumber"];
        if (!string.IsNullOrWhiteSpace(expectedAccount) && !string.Equals(expectedAccount.Trim(), payload.AccountNumber?.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { success = false, message = "Webhook account does not match the configured receiving account." });
        }

        var result = await _sender.Send(new ProcessSepayWebhookCommand(payload), cancellationToken);
        return Ok(new { success = true, result.Processed, result.Message });
    }

    private bool VerifyRequest(string rawBody)
    {
        var secret = _configuration["SEPAY_WEBHOOK_SECRET"] ?? _configuration["SePay:WebhookSecret"];
        if (!string.IsNullOrWhiteSpace(secret))
        {
            if (!long.TryParse(Request.Headers["X-SePay-Timestamp"].FirstOrDefault(), out var timestamp)) return false;
            if (Math.Abs(DateTimeOffset.UtcNow.ToUnixTimeSeconds() - timestamp) > 300) return false;

            var signature = Request.Headers["X-SePay-Signature"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(signature)) return false;
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var signedBytes = Encoding.UTF8.GetBytes($"{timestamp}.{rawBody}");
            var expected = $"sha256={Convert.ToHexString(hmac.ComputeHash(signedBytes)).ToLowerInvariant()}";
            return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(expected), Encoding.UTF8.GetBytes(signature));
        }

        var apiKey = _configuration["SEPAY_WEBHOOK_API_KEY"] ?? _configuration["SePay:WebhookApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey)) return false;
        var authorization = Request.Headers["Authorization"].FirstOrDefault() ?? string.Empty;
        return authorization.StartsWith("Apikey ", StringComparison.OrdinalIgnoreCase)
            && CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(apiKey), Encoding.UTF8.GetBytes(authorization[7..].Trim()));
    }
}
