using StudyAI.Domain.Common;

namespace StudyAI.Domain.Entities;

public sealed class UserTopicProgress : Entity
{
    private UserTopicProgress() { }

    public UserTopicProgress(Guid userId, Guid topicId)
    {
        UserId = userId;
        TopicId = topicId;
    }

    public Guid UserId { get; private set; }

    public Guid TopicId { get; private set; }

    public decimal AverageScore { get; private set; }

    public int Attempts { get; private set; }

    public User User { get; private set; } = null!;

    public Topic Topic { get; private set; } = null!;
}
