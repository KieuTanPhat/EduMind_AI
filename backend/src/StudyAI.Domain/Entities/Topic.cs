using StudyAI.Domain.Common;

namespace StudyAI.Domain.Entities;

public sealed class Topic : Entity
{
    private Topic() { }

    public Topic(string name)
    {
        Name = name;
        NormalizedName = name.ToUpperInvariant();
    }

    public string Name { get; private set; } = null!;

    public string NormalizedName { get; private set; } = null!;

    public ICollection<UserTopicProgress> UserProgress { get; private set; } = new List<UserTopicProgress>();
}
