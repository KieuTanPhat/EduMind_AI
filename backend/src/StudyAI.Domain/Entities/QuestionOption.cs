using StudyAI.Domain.Common;

namespace StudyAI.Domain.Entities;

public sealed class QuestionOption : Entity
{
    private QuestionOption() { }

    public QuestionOption(Guid questionId, string text, bool isCorrect)
    {
        QuestionId = questionId;
        Text = text;
        IsCorrect = isCorrect;
    }

    public Guid QuestionId { get; private set; }

    public string Text { get; private set; } = null!;

    public bool IsCorrect { get; private set; }

    public Question Question { get; private set; } = null!;
}
