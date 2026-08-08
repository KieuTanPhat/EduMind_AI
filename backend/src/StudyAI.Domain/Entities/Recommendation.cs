using StudyAI.Domain.Common;

namespace StudyAI.Domain.Entities;

public sealed class Recommendation : Entity
{
    private Recommendation() { }

    public Recommendation(Guid userId, string title, string description)
    {
        UserId = userId;
        Title = title;
        Description = description;
    }

    public Guid UserId { get; private set; }

    public string Title { get; private set; } = null!;

    public string Description { get; private set; } = null!;

    public bool IsCompleted { get; private set; }

    public DateTime? ExpiresAtUtc { get; private set; }

    public User User { get; private set; } = null!;
}
