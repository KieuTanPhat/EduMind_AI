using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StudyAI.Application.Abstractions;

namespace StudyAI.Infrastructure.Email;

public sealed class SmtpEmailService : IEmailService
{
    private readonly EmailOptions _options;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IOptions<EmailOptions> options, ILogger<SmtpEmailService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string?> SendEmailVerificationAsync(string recipientEmail, string recipientName, string token, CancellationToken cancellationToken)
    {
        var verificationUrl = $"{_options.FrontendBaseUrl.TrimEnd('/')}/verify-email?token={Uri.EscapeDataString(token)}";
        if (string.IsNullOrWhiteSpace(_options.SmtpHost))
        {
            _logger.LogWarning("Email SMTP is not configured. Verification link for {Email}: {VerificationUrl}", recipientEmail, verificationUrl);
            return verificationUrl;
        }

        using var client = new SmtpClient(_options.SmtpHost, _options.SmtpPort)
        {
            EnableSsl = _options.UseSsl,
            Credentials = new NetworkCredential(_options.Username, _options.Password)
        };
        using var mail = new MailMessage
        {
            From = new MailAddress(_options.FromAddress, _options.FromName),
            Subject = "Xác nhận email EduMind AI",
            IsBodyHtml = true,
            Body = $"<p>Xin chào {WebUtility.HtmlEncode(recipientName)},</p><p>Hãy click vào liên kết dưới đây để xác nhận email và bắt đầu học với EduMind AI:</p><p><a href=\"{WebUtility.HtmlEncode(verificationUrl)}\">Xác nhận email</a></p><p>Liên kết có hiệu lực trong 24 giờ.</p>"
        };
        mail.To.Add(recipientEmail);
        await client.SendMailAsync(mail, cancellationToken);
        return null;
    }

    public async Task<string?> SendEmailVerificationOtpAsync(string recipientEmail, string recipientName, string code, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.SmtpHost))
        {
            _logger.LogWarning("Email SMTP is not configured. Verification OTP for {Email}: {Code}", recipientEmail, code);
            return code;
        }

        using var client = new SmtpClient(_options.SmtpHost, _options.SmtpPort)
        {
            EnableSsl = _options.UseSsl,
            Credentials = new NetworkCredential(_options.Username, _options.Password)
        };
        using var mail = new MailMessage
        {
            From = new MailAddress(_options.FromAddress, _options.FromName),
            Subject = "Mã OTP xác nhận email EduMind AI",
            IsBodyHtml = true,
            Body = $"<p>Xin chào {WebUtility.HtmlEncode(recipientName)},</p><p>Mã OTP xác nhận email của bạn là:</p><p style=\"font-size:28px;font-weight:bold;letter-spacing:8px\">{WebUtility.HtmlEncode(code)}</p><p>Mã có hiệu lực trong 10 phút và tối đa 5 lần nhập.</p>"
        };
        mail.To.Add(recipientEmail);
        await client.SendMailAsync(mail, cancellationToken);
        return null;
    }
}
