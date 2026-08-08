using StudyAI.Domain.Common;

namespace StudyAI.Domain.Entities;

public sealed class UserAnswer : Entity
{
    private UserAnswer() { }

    public UserAnswer(Guid quizAttemptId, Guid questionId, Guid? selectedOptionId, bool isCorrect)
    {
        QuizAttemptId = quizAttemptId;
        QuestionId = questionId;
        SelectedOptionId = selectedOptionId;
        IsCorrect = isCorrect;
    }

    public Guid QuizAttemptId { get; private set; }

    public Guid QuestionId { get; private set; }

    public Guid? SelectedOptionId { get; private set; }

    public bool IsCorrect { get; private set; }

    public QuizAttempt QuizAttempt { get; private set; } = null!;

    public Question Question { get; private set; } = null!;

    public QuestionOption? SelectedOption { get; private set; }
}
