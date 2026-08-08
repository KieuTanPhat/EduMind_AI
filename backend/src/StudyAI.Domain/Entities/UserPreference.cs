using StudyAI.Domain.Common;

namespace StudyAI.Domain.Entities;

public sealed class UserPreference : Entity
{
    private UserPreference() { }

    public UserPreference(Guid userId)
    {
        UserId = userId;
    }

    public Guid UserId { get; private set; }

    public string LearningLevel { get; private set; } = "beginner";

    public string LearningGoal { get; private set; } = "general";

    public string PreferredLanguage { get; private set; } = "vi";

    public User User { get; private set; } = null!;

    public void Update(string learningLevel, string learningGoal, string preferredLanguage)
    {
        LearningLevel = learningLevel;
        LearningGoal = learningGoal;
        PreferredLanguage = preferredLanguage;
        Touch(DateTime.UtcNow);
    }
}
