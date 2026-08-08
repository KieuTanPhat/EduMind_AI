namespace StudyAI.Application.Abstractions;

public interface IEmailService
{
    Task SendEmailVerificationAsync(string recipientEmail, string recipientName, string token, CancellationToken cancellationToken);
}
