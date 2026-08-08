namespace StudyAI.Application.Abstractions;

public interface IEmailService
{
    Task<string?> SendEmailVerificationAsync(string recipientEmail, string recipientName, string token, CancellationToken cancellationToken);
    Task<string?> SendEmailVerificationOtpAsync(string recipientEmail, string recipientName, string code, CancellationToken cancellationToken);
}
