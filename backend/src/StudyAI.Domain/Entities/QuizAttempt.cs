using StudyAI.Domain.Common;

namespace StudyAI.Domain.Entities;

public sealed class QuizAttempt : Entity
{
    private QuizAttempt() { }

    public QuizAttempt(Guid quizId, Guid userId)
    {
        QuizId = quizId;
        UserId = userId;
        StartedAtUtc = DateTime.UtcNow;
    }

    public Guid QuizId { get; private set; }

    public Guid UserId { get; private set; }

    public int Score { get; private set; }

    public int TotalQuestions { get; private set; }

    public DateTime StartedAtUtc { get; private set; }

    public DateTime? CompletedAtUtc { get; private set; }

    public Quiz Quiz { get; private set; } = null!;

    public User User { get; private set; } = null!;

    public ICollection<UserAnswer> Answers { get; private set; } = new List<UserAnswer>();

    public void Complete(int score, int totalQuestions, DateTime completedAtUtc)
    {
        Score = score;
        TotalQuestions = totalQuestions;
        CompletedAtUtc = completedAtUtc;
        Touch(completedAtUtc);
    }
}
