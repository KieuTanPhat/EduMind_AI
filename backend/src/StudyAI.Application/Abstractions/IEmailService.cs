namespace StudyAI.Application.Abstractions;

public interface IEmailService
{
    Task<string?> SendEmailVerificationAsync(string recipientEmail, string recipientName, string token, CancellationToken cancellationToken);
}
