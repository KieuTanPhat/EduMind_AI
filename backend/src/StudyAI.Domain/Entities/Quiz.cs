using StudyAI.Domain.Common;

namespace StudyAI.Domain.Entities;

public sealed class Quiz : Entity
{
    private Quiz() { }

    public Quiz(Guid documentId, string title, string model)
    {
        DocumentId = documentId;
        Title = title;
        Model = model;
    }

    public Guid DocumentId { get; private set; }

    public string Title { get; private set; } = null!;

    public string Model { get; private set; } = null!;

    public Document Document { get; private set; } = null!;

    public ICollection<Question> Questions { get; private set; } = new List<Question>();

    public ICollection<QuizAttempt> Attempts { get; private set; } = new List<QuizAttempt>();
}
