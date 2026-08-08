namespace StudyAI.Infrastructure.AI;

public sealed class OpenAiOptions
{
    public const string SectionName = "OpenAI";

    public string ApiKey { get; set; } = string.Empty;

    public string Model { get; set; } = "gpt-5.4-nano";

    public string BaseUrl { get; set; } = "https://api.openai.com/v1/";

    public int MaxOutputTokens { get; set; } = 4096;
}
