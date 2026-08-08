using StudyAI.Domain.Common;

namespace StudyAI.Domain.Entities;

public sealed class Question : Entity
{
    private Question() { }

    public Question(Guid quizId, string content, string explanation)
    {
        QuizId = quizId;
        Content = content;
        Explanation = explanation;
    }

    public Guid QuizId { get; private set; }

    public string Content { get; private set; } = null!;

    public string Explanation { get; private set; } = null!;

    public Quiz Quiz { get; private set; } = null!;

    public ICollection<QuestionOption> Options { get; private set; } = new List<QuestionOption>();

    public ICollection<UserAnswer> UserAnswers { get; private set; } = new List<UserAnswer>();
}
