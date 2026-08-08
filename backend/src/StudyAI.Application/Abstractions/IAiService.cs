namespace StudyAI.Application.Abstractions;

public interface IAiService
{
    Task<string> GenerateAsync(string operation, string documentContext, string prompt, CancellationToken cancellationToken);
}
