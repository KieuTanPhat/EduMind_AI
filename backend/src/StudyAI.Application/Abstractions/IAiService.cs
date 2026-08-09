namespace StudyAI.Application.Abstractions;

public sealed record AiGenerationRequest(
    string Operation,
    string DocumentContext,
    string Prompt,
    bool StructuredJson,
    int? MaxOutputTokens = null);

public sealed record AiGenerationResult(
    string Text,
    string Model,
    int InputTokens,
    int OutputTokens);

public interface IAiService
{
    Task<AiGenerationResult> GenerateAsync(AiGenerationRequest request, CancellationToken cancellationToken);
}
