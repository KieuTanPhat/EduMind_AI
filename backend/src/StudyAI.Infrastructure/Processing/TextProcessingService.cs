using System.Text.RegularExpressions;
using StudyAI.Application.Abstractions;

namespace StudyAI.Infrastructure.Processing;

public sealed class TextProcessingService : ITextProcessingService
{
    private static readonly Regex RepeatedWhitespace = new(@"[ \t]+", RegexOptions.Compiled);
    private static readonly Regex RepeatedNewlines = new(@"\n{3,}", RegexOptions.Compiled);
    private static readonly Regex NewlineWhitespace = new(@"[ \t]*\n[ \t]*", RegexOptions.Compiled);

    public string Clean(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var normalized = text.Replace("\r\n", "\n").Replace('\r', '\n');
        normalized = RepeatedWhitespace.Replace(normalized, " ");
        normalized = RepeatedNewlines.Replace(normalized, "\n\n");
        normalized = NewlineWhitespace.Replace(normalized, "\n");
        return normalized.Trim();
    }

    public IReadOnlyList<string> Chunk(string text, int maxCharacters = 12000, int overlapCharacters = 500)
    {
        var cleaned = Clean(text);
        if (cleaned.Length == 0)
        {
            return Array.Empty<string>();
        }

        if (maxCharacters <= 0 || overlapCharacters < 0 || overlapCharacters >= maxCharacters)
        {
            throw new ArgumentOutOfRangeException(nameof(maxCharacters));
        }

        var chunks = new List<string>();
        var start = 0;
        while (start < cleaned.Length)
        {
            var end = Math.Min(start + maxCharacters, cleaned.Length);
            if (end < cleaned.Length)
            {
                var boundary = cleaned.LastIndexOfAny([' ', '\n', '.', ',', ';'], end - 1, Math.Min(maxCharacters, end - start));
                if (boundary > start + (maxCharacters / 2))
                {
                    end = boundary;
                }
            }

            chunks.Add(cleaned[start..end].Trim());
            if (end >= cleaned.Length)
            {
                break;
            }

            start = Math.Max(end - overlapCharacters, start + 1);
        }

        return chunks;
    }
}
