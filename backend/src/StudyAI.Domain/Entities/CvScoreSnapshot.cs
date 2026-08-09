using StudyAI.Domain.Common;

namespace StudyAI.Domain.Entities;

public sealed class CvScoreSnapshot : Entity
{
    private CvScoreSnapshot() { }

    public CvScoreSnapshot(
        Guid userId,
        Guid documentId,
        string targetRole,
        string experienceLevel,
        string jobDescriptionHash,
        string responseJson,
        string model,
        int inputTokens,
        int outputTokens)
    {
        UserId = userId;
        DocumentId = documentId;
        TargetRole = targetRole;
        ExperienceLevel = experienceLevel;
        JobDescriptionHash = jobDescriptionHash;
        ResponseJson = responseJson;
        Model = model;
        InputTokens = inputTokens;
        OutputTokens = outputTokens;
    }

    public Guid UserId { get; private set; }
    public Guid DocumentId { get; private set; }
    public string TargetRole { get; private set; } = null!;
    public string ExperienceLevel { get; private set; } = null!;
    public string JobDescriptionHash { get; private set; } = null!;
    public string ResponseJson { get; private set; } = null!;
    public string Model { get; private set; } = null!;
    public int InputTokens { get; private set; }
    public int OutputTokens { get; private set; }
}
