using StudyAI.Domain.Common;

namespace StudyAI.Domain.Entities;

public sealed class LearningProgress : Entity
{
    private LearningProgress() { }

    public LearningProgress(Guid userId, Guid documentId)
    {
        UserId = userId;
        DocumentId = documentId;
    }

    public Guid UserId { get; private set; }

    public Guid DocumentId { get; private set; }

    public int CompletionPercentage { get; private set; }

    public int StudyMinutes { get; private set; }

    public User User { get; private set; } = null!;

    public Document Document { get; private set; } = null!;
}
