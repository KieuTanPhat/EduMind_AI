using System.Text.Json;
using System.Text.RegularExpressions;
using StudyAI.Application.Common.Exceptions;

namespace StudyAI.Application.Features.AI;

internal static class AiJsonHelpers
{
    private static readonly Regex CodeFence = new("^```(?:json)?\\s*|\\s*```$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static JsonDocument Parse(string raw)
    {
        var json = CodeFence.Replace(raw.Trim(), string.Empty).Trim();
        try
        {
            return JsonDocument.Parse(json);
        }
        catch (JsonException exception)
        {
            throw new BadRequestException($"AI returned invalid JSON: {exception.Message}");
        }
    }

    public static string RequiredString(JsonElement element, string name, int maxLength = 4000)
    {
        if (!element.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.String)
        {
            throw new BadRequestException($"AI output is missing the required field '{name}'.");
        }

        var result = value.GetString()?.Trim();
        if (string.IsNullOrWhiteSpace(result))
        {
            throw new BadRequestException($"AI output contains an empty field '{name}'.");
        }

        return result.Length > maxLength ? result[..maxLength] : result;
    }

    public static string? OptionalString(JsonElement element, string name, int maxLength = 4000)
    {
        if (!element.TryGetProperty(name, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        var result = value.ValueKind == JsonValueKind.String ? value.GetString()?.Trim() : null;
        return string.IsNullOrWhiteSpace(result) ? null : result[..Math.Min(result.Length, maxLength)];
    }
}
