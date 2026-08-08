namespace StudyAI.Infrastructure.Storage;

public sealed class R2Options
{
    public const string SectionName = "Storage:R2";

    public string Endpoint { get; set; } = string.Empty;

    public string Bucket { get; set; } = string.Empty;

    public string AccessKeyId { get; set; } = string.Empty;

    public string SecretAccessKey { get; set; } = string.Empty;
}
