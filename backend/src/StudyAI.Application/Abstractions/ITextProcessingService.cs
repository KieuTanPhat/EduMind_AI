namespace StudyAI.Application.Abstractions;

public interface ITextProcessingService
{
    string Clean(string text);

    IReadOnlyList<string> Chunk(string text, int maxCharacters = 12000, int overlapCharacters = 500);
}
